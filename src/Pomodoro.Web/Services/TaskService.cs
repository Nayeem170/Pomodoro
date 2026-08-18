using System.Web;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class TaskService : ITaskService, ITimerEventSubscriber, IAsyncDisposable
{
    private const string ColorPalette = "#4285F4,#0B8043,#E67C73,#9C27B0,#F59E0B,#EC407A,#AB47BC,#FF5722,#795548";

    private readonly ITaskRepository _taskRepository;
    private readonly IIndexedDbService _indexedDb;
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleTasksService _googleTasksService;
    private readonly ILogger<TaskService> _logger;
    private readonly IPomodoroMetaRepository _sidecarRepo;

    private Timer? _midnightTimer;
    private List<GoogleListCacheEntry> _cachedGoogleLists = [];
    private GoogleTasksSettings _googleTasksSettings = new(new Dictionary<string, ListSetting>());
    private Dictionary<string, PomodoroMeta>? _sidecarCache;
    private bool _sidecarCacheDirty = true;

    public event Action? OnChange;

    public List<TaskItem> Tasks => _appState.Tasks.Where(t => !t.IsDeleted).ToList();
    public IReadOnlyList<TaskItem> AllTasks => _appState.Tasks;
    public Guid? CurrentTaskId => _appState.CurrentTaskId;
    public TaskItem? CurrentTask => _appState.CurrentTask;

    public IReadOnlyList<TaskListRef> TaskLists
    {
        get
        {
            var allTasks = _appState.Tasks;
            var roots = BuildRootLookup(allTasks);

            bool InTab(TaskItem t, bool scheduled) =>
                !t.IsDeleted && IsFromVisibleSource(t) &&
                (scheduled ? HasScheduleDate(roots(t)) : !HasSpecificScheduleDate(roots(t)) && OccursToday(roots(t)));

            return
            [
                new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)",
                    allTasks.Count(t => InTab(t, false)), true, true),
                new(Constants.TaskLists.ScheduleListId, "Schedule", "#a78bfa",
                    allTasks.Count(t => InTab(t, true)), true, true)
            ];
        }
    }

    public IReadOnlyList<TaskListRef> GoogleLists =>
        _cachedGoogleLists
            .Select(e => new TaskListRef(
                e.Id,
                e.Title,
                e.Color,
                _appState.Tasks.Count(t => t.GoogleListId == e.Id && !t.IsDeleted),
                e.IsVisible,
                false))
            .ToList();

    public TaskListRef? CurrentList => TaskLists.FirstOrDefault(l => l.Id == _appState.CurrentListId);
    public string? CurrentListId => _appState.CurrentListId;

    public TaskService(
        ITaskRepository taskRepository,
        IIndexedDbService indexedDb,
        AppState appState,
        IServiceProvider serviceProvider,
        IPomodoroMetaRepository pomodoroMetaRepo,
        IGoogleTasksService googleTasksService,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _indexedDb = indexedDb;
        _appState = appState;
        _serviceProvider = serviceProvider;
        _googleTasksService = googleTasksService;
        _logger = logger;
        _sidecarRepo = pomodoroMetaRepo;
    }

    public async Task InitializeAsync()
    {
        var tasks = await _taskRepository.GetAllIncludingDeletedAsync();
        if (tasks != null && tasks.Count > 0)
        {
            _appState.Tasks = tasks;
        }

        var appState = await _indexedDb.GetAsync<AppStateRecord>(Constants.Storage.AppStateStore, Constants.Storage.DefaultSettingsId);
        if (appState?.CurrentTaskId.HasValue == true)
        {
            var taskId = appState.CurrentTaskId.Value;
            if (_appState.Tasks.Any(t => t.Id == taskId))
            {
                _appState.CurrentTaskId = taskId;
            }
        }

        if (!string.IsNullOrEmpty(appState?.CurrentListId))
        {
            _appState.CurrentListId = appState.CurrentListId;
        }

        await LoadGoogleTasksSettingsAsync();

        await RestoreCachedGoogleListsFromSettingsAsync();

        await ActivateDueRecurringAndScheduledTasks();
        ScheduleMidnightReactivation();

        if (await _googleTasksService.IsConnectedAsync())
        {
            await RefreshGoogleListsAsync();
        }
        else
        {
            await EnsureLocalListSelectedAsync();
        }

        NotifyStateChanged();
    }

    public async Task ReloadAsync()
    {
        var tasks = await _taskRepository.GetAllIncludingDeletedAsync();
        _appState.Tasks = tasks ?? new List<TaskItem>();

        if (_appState.CurrentTaskId.HasValue)
        {
            if (!_appState.Tasks.Any(t => t.Id == _appState.CurrentTaskId.Value))
            {
                _appState.CurrentTaskId = null;
            }
        }

        NotifyStateChanged();
    }

    public async Task AddTaskAsync(string name)
    {
        var sanitized = SanitizeTaskName(name);

        if (string.IsNullOrEmpty(sanitized) || sanitized.Length > Constants.UI.MaxTaskNameLength)
        {
            return;
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = sanitized,
            CreatedAt = DateTime.UtcNow,
            TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes,
            PomodoroCount = Constants.Tasks.InitialPomodoroCount
        };

        await SaveTaskAsync(task);
        _appState.InsertTask(task, Constants.Tasks.InsertAtBeginning);
        _appState.CurrentTaskId = task.Id;
        await SaveCurrentTaskIdAsync();
        NotifyStateChanged();
        MarkDirty();
    }

    public async Task AddTaskAsync(string name, string? listId)
    {
        if (!string.IsNullOrEmpty(listId) && listId != Constants.TaskLists.LocalPomodoroListId && listId != Constants.TaskLists.ScheduleListId)
        {
            await AddGoogleTaskAsync(name, listId);
            return;
        }

        await AddTaskAsync(name);
    }

    private async Task AddGoogleTaskAsync(string name, string listId)
    {
        var sanitized = SanitizeTaskName(name);
        if (string.IsNullOrEmpty(sanitized) || sanitized.Length > Constants.UI.MaxTaskNameLength)
            return;

        var googleTask = new GoogleTask { Title = sanitized };
        var inserted = await _googleTasksService.InsertTaskAsync(listId, googleTask);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = inserted.Title,
            GoogleTaskId = inserted.Id,
            GoogleListId = listId,
            ETag = inserted.ETag,
            UpdatedAt = ParseGoogleDateTime(inserted.Updated),
            Notes = inserted.Notes,
            DueDate = ParseGoogleDate(inserted.Due),
            IsCompleted = inserted.Status == "completed",
            CreatedAt = DateTime.UtcNow,
            TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes,
            PomodoroCount = Constants.Tasks.InitialPomodoroCount
        };

        await SaveTaskAsync(task);
        _appState.InsertTask(task, Constants.Tasks.InsertAtBeginning);
        _appState.CurrentTaskId = task.Id;
        await SaveCurrentTaskIdAsync();
        NotifyStateChanged();
    }

    public async Task<Guid?> AddSubtaskAsync(string name, Guid parentTaskId)
    {
        var sanitized = SanitizeTaskName(name);
        if (string.IsNullOrEmpty(sanitized) || sanitized.Length > Constants.UI.MaxTaskNameLength)
            return null;

        var parent = _appState.FindTaskById(parentTaskId);
        if (parent == null) return null;

        var subtask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = sanitized,
            CreatedAt = DateTime.UtcNow,
            TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes,
            PomodoroCount = Constants.Tasks.InitialPomodoroCount,
            ParentTaskId = parentTaskId
        };

        if (parent.IsGoogleTask && !string.IsNullOrEmpty(parent.GoogleListId) && !string.IsNullOrEmpty(parent.GoogleTaskId))
        {
            var inserted = await _googleTasksService.InsertTaskAsync(
                parent.GoogleListId,
                new GoogleTask { Title = sanitized },
                parent.GoogleTaskId);

            subtask.Name = inserted.Title;
            subtask.GoogleTaskId = inserted.Id;
            subtask.GoogleListId = parent.GoogleListId;
            subtask.GoogleParentTaskId = parent.GoogleTaskId;
            subtask.GooglePosition = inserted.Position;
            subtask.ETag = inserted.ETag;
            subtask.UpdatedAt = ParseGoogleDateTime(inserted.Updated);
        }

        await SaveTaskAsync(subtask);
        _appState.InsertTask(subtask, Constants.Tasks.InsertAtEnd);
        NotifyStateChanged();
        MarkDirty();
        return subtask.Id;
    }

    public async Task ReparentTaskAsync(Guid taskId, Guid? newParentId)
    {
        if (newParentId == taskId) return;

        var task = _appState.FindTaskById(taskId);
        if (task == null) return;

        string? newGoogleParentId = null;
        if (newParentId.HasValue)
        {
            var parent = _appState.FindTaskById(newParentId.Value);
            if (parent == null) return;
            newGoogleParentId = parent.GoogleTaskId;
        }

        if (task.IsGoogleTask && !string.IsNullOrEmpty(task.GoogleListId) && !string.IsNullOrEmpty(task.GoogleTaskId))
        {
            var moved = await _googleTasksService.MoveTaskAsync(task.GoogleListId, task.GoogleTaskId, newGoogleParentId);
            if (moved != null)
            {
                _appState.UpdateTask(taskId, t =>
                {
                    t.GooglePosition = moved.Position;
                    t.ETag = moved.ETag;
                });
            }
        }

        _appState.UpdateTask(taskId, t =>
        {
            t.ParentTaskId = newParentId;
            t.GoogleParentTaskId = newGoogleParentId;
        });

        var updated = _appState.FindTaskById(taskId);
        if (updated != null)
        {
            await SaveTaskAsync(updated);
        }

        NotifyStateChanged();
        MarkDirty();
    }

    public async Task PromoteTaskAsync(Guid taskId)
    {
        var task = _appState.FindTaskById(taskId);
        if (task == null || !task.ParentTaskId.HasValue) return;

        var parent = _appState.FindTaskById(task.ParentTaskId.Value);
        var grandparentId = parent?.ParentTaskId;

        if (parent != null)
        {
            _appState.UpdateTask(taskId, t =>
            {
                t.ScheduledDate = parent.ScheduledDate;
                t.Repeat = parent.Repeat != null
                    ? new RepeatRule
                    {
                        Type = parent.Repeat.Type,
                        CustomDays = parent.Repeat.CustomDays,
                        Weekdays = parent.Repeat.Weekdays,
                        MonthlyDay = parent.Repeat.MonthlyDay,
                        StartDate = parent.Repeat.StartDate,
                        EndDate = parent.Repeat.EndDate,
                        IsPaused = parent.Repeat.IsPaused,
                        PausedDate = parent.Repeat.PausedDate
                    }
                    : null;
            });
        }

        await ReparentTaskAsync(taskId, grandparentId);
    }

    public async Task DemoteTaskAsync(Guid taskId, Guid targetSiblingId)
    {
        if (taskId == targetSiblingId) return;

        var task = _appState.FindTaskById(taskId);
        if (task == null) return;

        var target = _appState.FindTaskById(targetSiblingId);
        if (target == null) return;
        if (target.ParentTaskId != task.ParentTaskId) return;

        var targetDepth = GetTaskDepth(targetSiblingId);
        var movedSubtreeHeight = GetMaxSubtreeDepth(taskId);

        if (targetDepth + 1 + movedSubtreeHeight > Constants.Tasks.MaxSubtaskDepth) return;

        _appState.UpdateTask(taskId, t =>
        {
            t.Repeat = null;
            t.ScheduledDate = null;
            t.FollowsParentRepeat = true;
        });

        await ReparentTaskAsync(taskId, targetSiblingId);
    }

    public async Task SetFollowsParentRepeatAsync(Guid taskId, bool value)
    {
        var task = _appState.FindTaskById(taskId);
        if (task == null || !task.IsSubtask) return;

        _appState.UpdateTask(taskId, t => t.FollowsParentRepeat = value);
        await SaveTaskAsync(task.WithUpdates(c => c.FollowsParentRepeat = value));
        NotifyStateChanged();
    }

    public async Task<bool> ReorderTaskAsync(Guid taskId, Guid targetId, bool insertBefore)
    {
        if (taskId == targetId) return false;

        var task = _appState.FindTaskById(taskId);
        var target = _appState.FindTaskById(targetId);
        if (task == null || target == null) return false;
        if (task.IsDeleted || target.IsDeleted) return false;

        var group = TaskGrouping.GetSiblingGroup(_appState.Tasks, task);
        if (group.Count < 2 || group.Any(t => t.IsGoogleTask)) return false;

        var ordered = TaskGrouping.GetOrderedSiblingGroup(_appState.Tasks, task).ToList();

        if (ordered.All(t => t.SortOrder == 0))
        {
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i] = await PersistSortOrderAsync(ordered[i], (i + 1) * Constants.Tasks.InitialSortStep);
            }
        }

        var draggedIndex = ordered.FindIndex(t => t.Id == taskId);
        var without = ordered.Where(t => t.Id != taskId).ToList();
        var targetIndex = without.FindIndex(t => t.Id == targetId);
        if (targetIndex < 0) return false;

        var insertIndex = insertBefore ? targetIndex : targetIndex + 1;
        if (insertIndex == draggedIndex)
        {
            MarkDirty();
            return true;
        }

        var dragged = ordered[draggedIndex];
        var prev = insertIndex > 0 ? without[insertIndex - 1] : null;
        var next = insertIndex < without.Count ? without[insertIndex] : null;

        if (prev != null && next != null && next.SortOrder - prev.SortOrder < 2)
        {
            var finalOrder = new List<TaskItem>(without);
            finalOrder.Insert(insertIndex, dragged);
            for (var i = 0; i < finalOrder.Count; i++)
            {
                var desired = (i + 1) * Constants.Tasks.InitialSortStep;
                if (finalOrder[i].SortOrder != desired)
                {
                    await PersistSortOrderAsync(finalOrder[i], desired);
                }
            }
        }
        else
        {
            var newOrder = prev == null
                ? next!.SortOrder - Constants.Tasks.SortGap
                : next == null
                    ? prev.SortOrder + Constants.Tasks.SortGap
                    : prev.SortOrder + (next.SortOrder - prev.SortOrder) / 2;
            await PersistSortOrderAsync(dragged, newOrder);
        }

        NotifyStateChanged();
        MarkDirty();
        return true;
    }

    private async Task<TaskItem> PersistSortOrderAsync(TaskItem task, int sortOrder)
    {
        _appState.UpdateTask(task.Id, t => t.SortOrder = sortOrder);
        var updated = task.WithUpdates(c => c.SortOrder = sortOrder);
        await SaveTaskAsync(updated);
        return updated;
    }

    private int GetTaskDepth(Guid taskId)
    {
        var depth = 0;
        var currentId = taskId;
        var seen = new HashSet<Guid>();
        while (seen.Add(currentId))
        {
            var current = _appState.FindTaskById(currentId);
            if (current == null || !current.ParentTaskId.HasValue) break;
            currentId = current.ParentTaskId.Value;
            depth++;
        }
        return depth;
    }

    private int GetMaxSubtreeDepth(Guid taskId)
    {
        var children = _appState.Tasks.Where(t => t.ParentTaskId == taskId).ToList();
        if (children.Count == 0) return 0;
        return 1 + children.Max(c => GetMaxSubtreeDepth(c.Id));
    }

    public async Task MaterializeSingleAsync(TaskItem occurrence)
    {
        if (!occurrence.RepeatSeriesId.HasValue || !occurrence.OccurrenceDate.HasValue) return;

        var occurrenceDate = occurrence.OccurrenceDate.Value.Date;
        var alreadyMaterialized = _appState.Tasks.Any(t =>
            t.RepeatSeriesId == occurrence.RepeatSeriesId &&
            t.OccurrenceDate.HasValue &&
            t.OccurrenceDate.Value.Date == occurrenceDate);
        if (alreadyMaterialized) return;

        var materialized = occurrence.WithUpdates(c =>
        {
            c.Id = Guid.NewGuid();
            c.CreatedAt = DateTime.UtcNow;
            c.Repeat = null;
            c.ScheduledDate = occurrenceDate;
            c.OccurrenceDate = occurrenceDate;
        });

        await SaveTaskAsync(materialized);
        _appState.InsertTask(materialized, Constants.Tasks.InsertAtEnd);

        var idMapping = new Dictionary<Guid, Guid> { [occurrence.Id] = materialized.Id };
        var descendants = GetDescendantsByParentId(occurrence.Id, _appState.Tasks);
        foreach (var desc in descendants)
        {
            if (!desc.ParentTaskId.HasValue || !idMapping.TryGetValue(desc.ParentTaskId.Value, out var clonedParentId))
                continue;

            var clonedDesc = desc.WithUpdates(c =>
            {
                c.Id = Guid.NewGuid();
                c.CreatedAt = DateTime.UtcNow;
                c.ParentTaskId = clonedParentId;
                c.Repeat = null;
                c.ScheduledDate = occurrenceDate;
                c.OccurrenceDate = occurrenceDate;
            });

            idMapping[desc.Id] = clonedDesc.Id;
            await SaveTaskAsync(clonedDesc);
            _appState.InsertTask(clonedDesc, Constants.Tasks.InsertAtEnd);
        }

        NotifyStateChanged();
        MarkDirty();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        var name = (task.Name ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(name) || name.Length > Constants.UI.MaxTaskNameLength)
        {
            return;
        }

        var existingTask = _appState.FindTaskById(task.Id);
        if (existingTask == null) return;

        var taskToSave = existingTask.WithUpdates(c =>
        {
            c.Name = name;
            c.Notes = task.Notes;
            c.DueDate = task.DueDate;
            c.ScheduledDate = task.ScheduledDate;
            c.Repeat = task.Repeat;
        });

        var googlePushPatch = BuildPatch(existingTask, taskToSave);

        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(task.Id, t =>
        {
            t.Name = taskToSave.Name;
            t.Notes = taskToSave.Notes;
            t.DueDate = taskToSave.DueDate;
            t.ScheduledDate = taskToSave.ScheduledDate;
            t.Repeat = taskToSave.Repeat;
        });

        if (taskToSave.IsGoogleTask && !string.IsNullOrEmpty(taskToSave.GoogleTaskId) && googlePushPatch != null)
        {
            var result = await PushGooglePatchAsync(taskToSave, googlePushPatch);
            if (result != null)
            {
                var updatedEtag = result.ETag;
                _appState.UpdateTask(task.Id, t => t.ETag = updatedEtag);
                var updated = _appState.FindTaskById(task.Id);
                if (updated != null)
                {
                    var withEtag = updated.WithUpdates(c => c.ETag = updatedEtag);
                    await SaveTaskAsync(withEtag);
                }
            }
        }
        else
        {
            MarkDirty();
        }

        NotifyStateChanged();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;

        if (existingTask.IsGoogleTask && !string.IsNullOrEmpty(existingTask.GoogleTaskId))
        {
            try
            {
                await _googleTasksService.DeleteTaskAsync(existingTask.GoogleListId!, existingTask.GoogleTaskId);
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Failed to delete Google task {TaskId}, marking local dirty", existingTask.GoogleTaskId);
                existingTask = existingTask.WithUpdates(c => c.IsLocalDirty = true);
                _appState.UpdateTask(taskId, t => t.IsLocalDirty = true);
            }
        }

        var toDelete = _appState.Tasks
            .Where(t => t.Id == taskId || IsDescendantOf(t, taskId))
            .ToList();

        foreach (var task in toDelete)
        {
            var taskToSave = task.WithUpdates(c =>
            {
                c.IsDeleted = true;
                c.DeletedAt = DateTime.UtcNow;
            });
            await SaveTaskAsync(taskToSave);

            _appState.UpdateTask(task.Id, t =>
            {
                t.IsDeleted = true;
                t.DeletedAt = DateTime.UtcNow;
            });
        }

        if (toDelete.Any(t => t.Id == _appState.CurrentTaskId))
        {
            _appState.CurrentTaskId = null;
            await SaveCurrentTaskIdAsync();
        }

        if (!existingTask.IsGoogleTask)
            MarkDirty();

        NotifyStateChanged();
    }

    public async Task RestoreTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null || !existingTask.IsDeleted) return;

        var toRestore = _appState.Tasks
            .Where(t => t.Id == taskId || IsDescendantOf(t, taskId))
            .ToList();

        foreach (var task in toRestore)
        {
            var restored = task.WithUpdates(c =>
            {
                c.IsDeleted = false;
                c.DeletedAt = null;
            });
            await SaveTaskAsync(restored);
            _appState.UpdateTask(task.Id, t =>
            {
                t.IsDeleted = false;
                t.DeletedAt = null;
            });
        }

        MarkDirty();
        NotifyStateChanged();
    }

    private bool IsDescendantOf(TaskItem task, Guid ancestorId)
    {
        var current = task;
        var seen = new HashSet<Guid>();
        while (current.ParentTaskId.HasValue)
        {
            if (current.ParentTaskId.Value == ancestorId) return true;
            if (!seen.Add(current.Id)) break;
            var parent = _appState.FindTaskById(current.ParentTaskId.Value);
            if (parent == null) break;
            current = parent;
        }
        return false;
    }

    private static List<TaskItem> GetDescendantsByParentId(Guid rootId, IReadOnlyList<TaskItem> all)
    {
        var childrenByParent = all
            .Where(t => t.ParentTaskId.HasValue)
            .GroupBy(t => t.ParentTaskId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TaskItem>)g.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt).ToList());

        var result = new List<TaskItem>();
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);
        var seen = new HashSet<Guid> { rootId };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!childrenByParent.TryGetValue(current, out var children)) continue;
            foreach (var child in children)
            {
                if (!seen.Add(child.Id)) continue;
                result.Add(child);
                queue.Enqueue(child.Id);
            }
        }

        return result;
    }

    public async Task CompleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;

        var incompleteSubtaskIds = _appState.Tasks
            .Where(t => !t.IsDeleted
                && !t.IsCompleted
                && (t.ParentTaskId == taskId
                    || (!string.IsNullOrEmpty(existingTask.GoogleTaskId)
                        && t.GoogleParentTaskId == existingTask.GoogleTaskId)))
            .Select(t => t.Id)
            .ToList();
        if (incompleteSubtaskIds.Count > 0)
        {
            throw new InvalidOperationException(Constants.Messages.CompleteSubtasksFirst);
        }

        var isRecurring = existingTask.IsRecurring && existingTask.Repeat is { IsActive: true };

        if (isRecurring)
        {
            existingTask.Repeat!.LastCompletedDate = DateTime.Now.Date;
        }

        var taskToSave2 = existingTask.WithUpdates(c =>
        {
            c.IsCompleted = true;
            c.CompletedAt = DateTime.UtcNow;
        });
        await SaveTaskAsync(taskToSave2);

        _appState.UpdateTask(taskId, t =>
        {
            t.IsCompleted = true;
            t.CompletedAt = DateTime.UtcNow;
        });
        if (existingTask.IsGoogleTask && !string.IsNullOrEmpty(existingTask.GoogleTaskId))
        {
            var patch = new GoogleTaskPatch(null, null, "completed");
            await PushGooglePatchAsync(existingTask, patch);
        }
        else
        {
            MarkDirty();
        }

        await CascadeCompletionUpwardAsync(taskId, new HashSet<Guid> { taskId });
        NotifyStateChanged();
    }

    public async Task UncompleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;

        var taskToSave = existingTask.WithUpdates(c =>
        {
            c.IsCompleted = false;
            c.CompletedAt = null;
        });
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(taskId, t =>
        {
            t.IsCompleted = false;
            t.CompletedAt = null;
        });
        if (existingTask.IsGoogleTask && !string.IsNullOrEmpty(existingTask.GoogleTaskId))
        {
            var patch = new GoogleTaskPatch(null, null, "needsAction");
            await PushGooglePatchAsync(existingTask, patch);
        }
        else
        {
            MarkDirty();
        }

        await CascadeUncompletionUpwardAsync(taskId, new HashSet<Guid> { taskId });
        NotifyStateChanged();
    }

    private async Task CascadeCompletionUpwardAsync(Guid childId, HashSet<Guid> visited)
    {
        var child = _appState.FindTaskById(childId);
        if (child == null || !child.ParentTaskId.HasValue) return;

        var parentId = child.ParentTaskId.Value;
        if (!visited.Add(parentId)) return;

        var parent = _appState.FindTaskById(parentId);
        if (parent == null || parent.IsCompleted) return;

        var hasIncompleteSubtask = _appState.Tasks.Any(t => !t.IsDeleted
            && !t.IsCompleted
            && (t.ParentTaskId == parentId
                || (!string.IsNullOrEmpty(parent.GoogleTaskId)
                    && t.GoogleParentTaskId == parent.GoogleTaskId)));
        if (hasIncompleteSubtask) return;

        if (parent.IsRecurring && parent.Repeat is { IsActive: true })
        {
            parent.Repeat!.LastCompletedDate = DateTime.Now.Date;
        }

        var parentToSave = parent.WithUpdates(c =>
        {
            c.IsCompleted = true;
            c.CompletedAt = DateTime.UtcNow;
        });
        await SaveTaskAsync(parentToSave);

        _appState.UpdateTask(parentId, t =>
        {
            t.IsCompleted = true;
            t.CompletedAt = DateTime.UtcNow;
        });

        if (parent.IsGoogleTask && !string.IsNullOrEmpty(parent.GoogleTaskId))
        {
            await PushGooglePatchAsync(parent, new GoogleTaskPatch(null, null, "completed"));
        }
        else
        {
            MarkDirty();
        }

        await CascadeCompletionUpwardAsync(parentId, visited);
    }

    private async Task CascadeUncompletionUpwardAsync(Guid childId, HashSet<Guid> visited)
    {
        var child = _appState.FindTaskById(childId);
        if (child == null || !child.ParentTaskId.HasValue) return;

        var parentId = child.ParentTaskId.Value;
        if (!visited.Add(parentId)) return;

        var parent = _appState.FindTaskById(parentId);
        if (parent == null || !parent.IsCompleted) return;

        var parentToSave = parent.WithUpdates(c =>
        {
            c.IsCompleted = false;
            c.CompletedAt = null;
        });
        await SaveTaskAsync(parentToSave);

        _appState.UpdateTask(parentId, t =>
        {
            t.IsCompleted = false;
            t.CompletedAt = null;
        });

        if (parent.IsGoogleTask && !string.IsNullOrEmpty(parent.GoogleTaskId))
        {
            await PushGooglePatchAsync(parent, new GoogleTaskPatch(null, null, "needsAction"));
        }
        else
        {
            MarkDirty();
        }

        await CascadeUncompletionUpwardAsync(parentId, visited);
    }

    public async Task SelectTaskAsync(Guid taskId)
    {
        var task = _appState.FindTaskById(taskId);

        if (task != null && !task.IsCompleted)
        {
            _appState.CurrentTaskId = taskId;
            await SaveCurrentTaskIdAsync();
            NotifyStateChanged();
        }
    }

    public async Task AddTimeToTaskAsync(Guid taskId, int minutes)
    {
        if (minutes <= 0) return;

        var task = _appState.FindTaskById(taskId);
        if (task == null) return;

        if (task.IsGoogleTask && !string.IsNullOrEmpty(task.GoogleTaskId))
        {
            var meta = await _sidecarRepo.GetAsync(task.GoogleTaskId);
            meta = new PomodoroMeta(
                task.GoogleTaskId,
                meta?.PomodoroCount + 1 ?? 1,
                meta?.TotalFocusMinutes + minutes ?? minutes,
                meta?.Priority ?? Priority.None);
            await _sidecarRepo.SaveAsync(meta);
            InvalidateSidecarCache();
            _appState.UpdateTask(taskId, t => { t.LastWorkedOn = DateTime.UtcNow; });
        }
        else
        {
            var updated = _appState.UpdateTask(taskId, t =>
            {
                t.TotalFocusMinutes += minutes;
                t.PomodoroCount++;
                t.LastWorkedOn = DateTime.UtcNow;
            });

            if (!updated) return;

            await SaveTaskAsync(task);
        }

        NotifyStateChanged();
    }

    public async Task SaveAsync()
    {
        var tasksToSave = _appState.Tasks.ToList();
        await _indexedDb.PutAllAsync(Constants.Storage.TasksStore, tasksToSave);
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksForListAsync(string listId)
    {
        if (!IsKnownList(listId))
        {
            listId = Constants.TaskLists.LocalPomodoroListId;
        }

        var allTasks = _appState.Tasks;
        var roots = BuildRootLookup(allTasks);
        var wantScheduled = listId == Constants.TaskLists.ScheduleListId;

        IEnumerable<TaskItem> filtered = allTasks.Where(t =>
            !t.IsDeleted && IsFromVisibleSource(t) &&
            (wantScheduled ? HasScheduleDate(roots(t)) : !HasSpecificScheduleDate(roots(t)) && OccursToday(roots(t))));

        var tasks = filtered.ToList();

        var hasGoogleTasks = tasks.Any(t => t.IsGoogleTask);
        if (hasGoogleTasks)
        {
            var metaDict = await GetSidecarCacheAsync();

            tasks = tasks.Select(t =>
            {
                if (!string.IsNullOrEmpty(t.GoogleTaskId) && metaDict.TryGetValue(t.GoogleTaskId, out var meta))
                {
                    return t.WithUpdates(c =>
                    {
                        c.PomodoroCount = meta.PomodoroCount;
                        c.TotalFocusMinutes = meta.TotalFocusMinutes;
                        c.Priority = meta.Priority;
                    });
                }
                return t;
            }).ToList();
        }

        return tasks;
    }

    public async Task SelectListAsync(string listId)
    {
        _appState.CurrentListId = listId;
        await SaveCurrentTaskIdAsync();
        NotifyStateChanged();
    }

    public async Task RefreshGoogleListsAsync()
    {
        if (!await _googleTasksService.IsConnectedAsync())
        {
            _cachedGoogleLists = [];
            await EnsureCurrentListSelectableAsync();
            return;
        }

        try
        {
            var googleLists = await _googleTasksService.GetTaskListsAsync();
            var remoteLists = googleLists?.ToList() ?? [];

            var updatedCache = new List<GoogleListCacheEntry>();
            var palette = ColorPalette.Split(',');

            for (var i = 0; i < remoteLists.Count; i++)
            {
                var gList = remoteLists[i];
                var listId = gList.Id;
                var settingsEntry = _googleTasksSettings.Lists.GetValueOrDefault(listId);

                if (settingsEntry != null)
                {
                    updatedCache.Add(new GoogleListCacheEntry(listId, gList.Title, settingsEntry.Color, settingsEntry.IsVisible));
                }
                else
                {
                    var color = palette[i % palette.Length];
                    updatedCache.Add(new GoogleListCacheEntry(listId, gList.Title, color, true));

                    _googleTasksSettings.Lists[listId] = new ListSetting(true, color, null);
                }
            }

            _cachedGoogleLists = updatedCache;
            await SaveGoogleTasksSettingsAsync();
            await SaveGoogleListsCacheAsync();
            InvalidateSidecarCache();

            foreach (var gList in remoteLists)
            {
                try
                {
                    var googleTasks = await _googleTasksService.GetTasksAsync(gList.Id);
                    if (googleTasks == null) continue;

                    var remoteIds = googleTasks.Select(t => t.Id).ToHashSet();
                    var existingInList = (await _taskRepository.GetByGoogleListIdAsync(gList.Id)).ToList();

                    foreach (var gTask in googleTasks)
                    {
                        if (gTask.Hidden && gTask.Status == "completed")
                        {
                            var hiddenLocal = existingInList.FirstOrDefault(t => t.GoogleTaskId == gTask.Id)
                                ?? _appState.Tasks.FirstOrDefault(t => t.GoogleTaskId == gTask.Id);
                            if (hiddenLocal != null && !hiddenLocal.IsDeleted)
                            {
                                var deleted = hiddenLocal.WithUpdates(c =>
                                {
                                    c.IsDeleted = true;
                                    c.DeletedAt = DateTime.UtcNow;
                                });
                                await _taskRepository.SaveAsync(deleted);
                                _appState.UpdateTask(hiddenLocal.Id, t =>
                                {
                                    t.IsDeleted = true;
                                    t.DeletedAt = DateTime.UtcNow;
                                });
                            }
                            continue;
                        }

                        var local = existingInList.FirstOrDefault(t => t.GoogleTaskId == gTask.Id)
                            ?? _appState.Tasks.FirstOrDefault(t => t.GoogleTaskId == gTask.Id);
                        if (local != null)
                        {
                            if (local.IsLocalDirty)
                            {
                                if (local.IsDeleted)
                                {
                                    continue;
                                }

                                var localMatchesRemote =
                                    local.Name == gTask.Title &&
                                    local.IsCompleted == (gTask.Status == "completed") &&
                                    (local.Notes ?? "") == (gTask.Notes ?? "") &&
                                    local.DueDate == ParseGoogleDate(gTask.Due);

                                if (localMatchesRemote)
                                {
                                    var cleared = local.WithUpdates(c =>
                                    {
                                        c.ETag = gTask.ETag;
                                        c.UpdatedAt = ParseGoogleDateTime(gTask.Updated);
                                        c.GoogleParentTaskId = gTask.Parent;
                                        c.GooglePosition = gTask.Position;
                                        c.IsLocalDirty = false;
                                    });
                                    await _taskRepository.SaveAsync(cleared);
                                    _appState.UpdateTask(local.Id, t =>
                                    {
                                        t.ETag = cleared.ETag;
                                        t.UpdatedAt = cleared.UpdatedAt;
                                        t.GoogleParentTaskId = cleared.GoogleParentTaskId;
                                        t.GooglePosition = cleared.GooglePosition;
                                        t.IsLocalDirty = false;
                                    });
                                }
                            }
                            else
                            {
                                var updated = local.WithUpdates(c =>
                                {
                                    c.Name = gTask.Title;
                                    c.IsCompleted = gTask.Status == "completed";
                                    c.Notes = gTask.Notes;
                                    c.DueDate = ParseGoogleDate(gTask.Due);
                                    c.ETag = gTask.ETag;
                                    c.UpdatedAt = ParseGoogleDateTime(gTask.Updated);
                                    c.GoogleListId = gList.Id;
                                    c.GoogleParentTaskId = gTask.Parent;
                                    c.GooglePosition = gTask.Position;
                                    c.IsDeleted = false;
                                    c.DeletedAt = null;
                                });
                                await _taskRepository.SaveAsync(updated);
                                _appState.UpdateTask(local.Id, t =>
                                {
                                    t.Name = updated.Name;
                                    t.IsCompleted = updated.IsCompleted;
                                    t.Notes = updated.Notes;
                                    t.DueDate = updated.DueDate;
                                    t.ETag = updated.ETag;
                                    t.UpdatedAt = updated.UpdatedAt;
                                    t.GoogleListId = updated.GoogleListId;
                                    t.GoogleParentTaskId = updated.GoogleParentTaskId;
                                    t.GooglePosition = updated.GooglePosition;
                                    t.IsDeleted = false;
                                    t.DeletedAt = null;
                                });
                            }
                        }
                        else
                        {
                            var newTask = MapGoogleTaskToTaskItem(gTask, gList.Id);
                            await _taskRepository.SaveAsync(newTask);
                            _appState.InsertTask(newTask, Constants.Tasks.InsertAtEnd);
                        }
                    }

                    foreach (var orphan in existingInList.Where(t => !string.IsNullOrEmpty(t.GoogleTaskId) && !remoteIds.Contains(t.GoogleTaskId)))
                    {
                        var deleted = orphan.WithUpdates(c =>
                        {
                            c.IsDeleted = true;
                            c.DeletedAt = DateTime.UtcNow;
                        });
                        await _taskRepository.SaveAsync(deleted);
                        _appState.UpdateTask(orphan.Id, t =>
                        {
                            t.IsDeleted = true;
                            t.DeletedAt = DateTime.UtcNow;
                        });
                    }

                    await ResolveGoogleParentIdsAsync(gList.Id);
                }
                catch (Exception ex) when (ex is not UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Failed to refresh Google list {ListId}, skipping", gList.Id);
                }
            }
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Failed to refresh Google task lists");
        }
        finally
        {
            await EnsureCurrentListSelectableAsync();
        }
    }

    private async Task EnsureCurrentListSelectableAsync()
    {
        var current = _appState.CurrentListId;
        if (!string.IsNullOrEmpty(current) && !IsKnownList(current))
        {
            await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
        }
    }

    private async Task EnsureLocalListSelectedAsync()
    {
        var current = _appState.CurrentListId;
        if (!string.IsNullOrEmpty(current) && current != Constants.TaskLists.LocalPomodoroListId && current != Constants.TaskLists.ScheduleListId)
        {
            await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
        }
    }

    private static bool IsKnownList(string? listId) =>
        listId == Constants.TaskLists.LocalPomodoroListId ||
        listId == Constants.TaskLists.ScheduleListId;

    private bool IsFromVisibleSource(TaskItem task) =>
        !task.IsGoogleTask ||
        _cachedGoogleLists.Any(l => l.Id == task.GoogleListId && l.IsVisible);

    private static bool HasScheduleDate(TaskItem task) =>
        task.ScheduledDate.HasValue ||
        task.DueDate.HasValue ||
        task.Repeat is { Type: not RepeatType.None };

    private static bool HasSpecificScheduleDate(TaskItem task) =>
        task.ScheduledDate.HasValue ||
        task.DueDate.HasValue;

    private static bool OccursToday(TaskItem task) => task.OccursToday;

    /// <summary>
    /// Builds a task-to-root-ancestor resolver. Subtasks carry no date of their own, so
    /// tab routing is decided by their root; classifying per-task would split a parent and
    /// its children across tabs and leave the children orphaned when the tree is rebuilt.
    /// </summary>
    private static Func<TaskItem, TaskItem> BuildRootLookup(IReadOnlyList<TaskItem> tasks)
    {
        var byId = new Dictionary<Guid, TaskItem>();
        foreach (var t in tasks) byId[t.Id] = t;

        var byGoogleId = new Dictionary<string, TaskItem>();
        foreach (var t in tasks)
        {
            if (!string.IsNullOrEmpty(t.GoogleTaskId))
                byGoogleId[t.GoogleTaskId] = t;
        }

        var resolved = new Dictionary<Guid, TaskItem>();

        return task =>
        {
            if (resolved.TryGetValue(task.Id, out var cached)) return cached;

            var current = task;
            var seen = new HashSet<Guid> { current.Id };

            while (true)
            {
                TaskItem? parent = null;

                if (current.ParentTaskId.HasValue)
                    byId.TryGetValue(current.ParentTaskId.Value, out parent);

                if (parent == null && !string.IsNullOrEmpty(current.GoogleParentTaskId))
                    byGoogleId.TryGetValue(current.GoogleParentTaskId, out parent);

                if (parent == null || parent.IsDeleted || !seen.Add(parent.Id)) break;

                current = parent;
            }

            resolved[task.Id] = current;
            return current;
        };
    }

    private async Task SaveTaskAsync(TaskItem task)
    {
        await _taskRepository.SaveAsync(task);
    }

    private async Task SaveCurrentTaskIdAsync()
    {
        var appStateRecord = new AppStateRecord
        {
            Id = Constants.Storage.DefaultSettingsId,
            CurrentTaskId = _appState.CurrentTaskId,
            CurrentListId = _appState.CurrentListId
        };
        await _indexedDb.PutAsync(Constants.Storage.AppStateStore, appStateRecord);
    }

    public async Task HandleTimerCompletedAsync(TimerCompletedEventArgs args)
    {
        if (args.SessionType != SessionType.Pomodoro || !args.TaskId.HasValue)
            return;

        await AddTimeToTaskAsync(args.TaskId.Value, args.DurationMinutes);
    }

    private static DateTime? ParseGoogleDateTime(string? iso)
    {
        if (string.IsNullOrEmpty(iso)) return null;
        if (DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
        return null;
    }

    private static DateTime? ParseGoogleDate(string? date)
    {
        if (string.IsNullOrEmpty(date)) return null;
        if (DateTime.TryParse(date, out var dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return null;
    }

    private static TaskItem MapGoogleTaskToTaskItem(GoogleTask g, string listId)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = g.Title,
            GoogleTaskId = g.Id,
            GoogleListId = listId,
            GoogleParentTaskId = g.Parent,
            GooglePosition = g.Position,
            ETag = g.ETag,
            UpdatedAt = ParseGoogleDateTime(g.Updated),
            Notes = g.Notes,
            DueDate = ParseGoogleDate(g.Due),
            IsCompleted = g.Status == "completed",
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.None,
            TotalFocusMinutes = 0,
            PomodoroCount = 0
        };
    }

    private async Task ResolveGoogleParentIdsAsync(string googleListId)
    {
        var tasksInList = _appState.Tasks
            .Where(t => t.GoogleListId == googleListId && !t.IsDeleted)
            .ToList();

        var googleIdToLocalId = tasksInList
            .Where(t => !string.IsNullOrEmpty(t.GoogleTaskId))
            .ToDictionary(t => t.GoogleTaskId!, t => t.Id);

        foreach (var task in tasksInList)
        {
            Guid? resolvedParentId = null;
            if (!string.IsNullOrEmpty(task.GoogleParentTaskId)
                && googleIdToLocalId.TryGetValue(task.GoogleParentTaskId, out var localParentId))
            {
                resolvedParentId = localParentId;
            }

            if (task.ParentTaskId != resolvedParentId)
            {
                var taskToSave = task.WithUpdates(c => c.ParentTaskId = resolvedParentId);
                await _taskRepository.SaveAsync(taskToSave);
                _appState.UpdateTask(task.Id, t => t.ParentTaskId = resolvedParentId);
            }
        }
    }

    private static DateTime? ComputeNextOccurrence(RepeatRule rule)
    {
        if (rule.Type == RepeatType.None) return null;
        if (rule.EndDate.HasValue && rule.EndDate.Value < DateTime.Now.Date) return null;

        var baseDate = (rule.LastCompletedDate ?? DateTime.Now.Date).Date;

        var next = rule.Type switch
        {
            RepeatType.Daily => baseDate.AddDays(1),
            RepeatType.Weekly => ComputeNextWeekday(baseDate, rule.Weekdays),
            RepeatType.Custom => baseDate.AddDays(rule.CustomDays > 0 ? rule.CustomDays : Constants.Repeat.DefaultCustomDays),
            RepeatType.Monthly => ComputeNextMonthly(baseDate, rule.MonthlyDay),
            _ => (DateTime?)null
        };

        if (next.HasValue && rule.EndDate.HasValue && next.Value > rule.EndDate.Value)
            return null;

        return next;
    }

    private static DateTime ComputeNextWeekday(DateTime baseDate, DayOfWeek[] weekdays)
    {
        if (weekdays.Length == 0) return baseDate.AddDays(7);

        var sorted = weekdays.OrderBy(d => d).ToArray();

        for (var i = 0; i < 14; i++)
        {
            var candidate = baseDate.AddDays(i + 1);
            if (sorted.Contains(candidate.DayOfWeek))
                return candidate;
        }

        return baseDate.AddDays(7);
    }

    private static DateTime ComputeNextMonthly(DateTime baseDate, int? monthlyDay)
    {
        var day = monthlyDay ?? Constants.Repeat.DefaultMonthlyDay;
        var nextMonth = baseDate.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var actualDay = Math.Min(day, daysInMonth);
        return new DateTime(nextMonth.Year, nextMonth.Month, actualDay);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private void ScheduleMidnightReactivation()
    {
        _midnightTimer?.Dispose();

        var delay = GetDelayUntilMidnight();

        _midnightTimer = new Timer(
            _ => { _ = HandleMidnightTimerCallbackAsync(); },
            null,
            delay,
            Timeout.InfiniteTimeSpan);
    }

    internal static TimeSpan GetDelayUntilMidnight()
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var delay = nextMidnight - now;
        return delay < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : delay;
    }

    internal async Task HandleMidnightTimerCallbackAsync()
    {
        try
        {
            await OnMidnightElapsedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Midnight recurring task reactivation failed");
        }
        finally
        {
            ScheduleMidnightReactivation();
        }
    }

    internal async Task OnMidnightElapsedAsync()
    {
        await ActivateDueRecurringAndScheduledTasks();
        NotifyStateChanged();
    }

    private async Task ActivateDueRecurringAndScheduledTasks()
    {
        var today = DateTime.Now.Date;
        var changed = false;

        foreach (var task in _appState.Tasks)
        {
            if (task.IsDeleted) continue;

            if (task.IsRecurring && task.IsCompleted && task.Repeat is { IsActive: true })
            {
                var nextOccurrence = ComputeNextOccurrence(task.Repeat);
                task.Repeat.NextOccurrence = nextOccurrence;

                if (nextOccurrence.HasValue && nextOccurrence.Value <= today)
                {
                    task.IsCompleted = false;
                    task.CompletedAt = null;
                    task.TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes;
                    task.PomodoroCount = Constants.Tasks.InitialPomodoroCount;
                    task.LastWorkedOn = null;
                    changed = true;
                }
            }

            if (task.IsScheduled && task.IsCompleted && task.ScheduledDate.HasValue && task.ScheduledDate.Value <= today)
            {
                task.IsCompleted = false;
                task.CompletedAt = null;
                changed = true;
            }
        }

        ReconcileFollowsParentSubtasks(ref changed);

        if (changed)
        {
            await SaveAsync();
        }
    }

    /// <summary>
    /// Resets FollowsParentRepeat subtasks whose completion is stale (from a previous
    /// repeat cycle). Handles both fresh cascade (parent just reactivated) and stuck
    /// state (parent reactivated before this feature existed).
    /// </summary>
    private void ReconcileFollowsParentSubtasks(ref bool changed)
    {
        foreach (var task in _appState.Tasks)
        {
            if (task.IsDeleted || task.IsCompleted) continue;
            if (!task.IsRecurring || task.Repeat is not { IsActive: true }) continue;
            if (!task.Repeat.LastCompletedDate.HasValue) continue;

            var reactivationDate = ComputeNextOccurrence(task.Repeat);
            if (!reactivationDate.HasValue) continue;

            ReconcileSubtree(task.Id, reactivationDate.Value, ref changed);
        }
    }

    private void ReconcileSubtree(Guid parentId, DateTime reactivationDate, ref bool changed)
    {
        var childrenToReset = _appState.Tasks
            .Where(t => !t.IsDeleted
                && t.ParentTaskId == parentId
                && t.FollowsParentRepeat
                && t.IsCompleted
                && (t.CompletedAt == null || t.CompletedAt.Value.Date < reactivationDate))
            .ToList();

        foreach (var child in childrenToReset)
        {
            child.IsCompleted = false;
            child.CompletedAt = null;
            child.TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes;
            child.PomodoroCount = Constants.Tasks.InitialPomodoroCount;
            child.LastWorkedOn = null;
            changed = true;

            ReconcileSubtree(child.Id, reactivationDate, ref changed);
        }
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }

    private ICloudSyncService? _cloudSyncService;

    private void MarkDirty()
    {
        _cloudSyncService ??= _serviceProvider.GetService<ICloudSyncService>();
        _cloudSyncService?.ScheduleSyncAsync();
    }

    private static GoogleTaskPatch? BuildPatch(TaskItem existing, TaskItem updated)
    {
        string? title = null;
        string? notes = null;
        string? status = null;
        string? due = null;

        if (existing.Name != updated.Name) title = updated.Name;
        if (existing.Notes != updated.Notes) notes = updated.Notes ?? "";
        if (existing.IsCompleted != updated.IsCompleted)
            status = updated.IsCompleted ? "completed" : "needsAction";
        if (existing.DueDate != updated.DueDate)
            due = updated.DueDate.HasValue ? updated.DueDate.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") : "";

        if (title == null && notes == null && status == null && due == null)
            return null;

        return new GoogleTaskPatch(title, notes, status, due);
    }

    private async Task<GoogleTask?> PushGooglePatchAsync(TaskItem task, GoogleTaskPatch patch)
    {
        try
        {
            var result = await _googleTasksService.PatchTaskAsync(
                task.GoogleListId!, task.GoogleTaskId!, patch, task.ETag);
            if (result != null)
            {
                var updated = _appState.FindTaskById(task.Id);
                if (updated != null)
                {
                    var saved = updated.WithUpdates(c =>
                    {
                        c.ETag = result.ETag;
                        c.IsLocalDirty = false;
                    });
                    await _taskRepository.SaveAsync(saved);
                    _appState.UpdateTask(task.Id, t =>
                    {
                        t.ETag = saved.ETag;
                        t.IsLocalDirty = false;
                    });
                }
            }
            return result;
        }
        catch (Exception ex) when (ex.Message.Contains("412") && task.ETag != null)
        {
            _logger.LogWarning("ETag conflict on task {TaskId}, pulling to reconcile", task.GoogleTaskId);
            await RefreshGoogleListsAsync();
            return null;
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Failed to push Google patch for task {TaskId}, marking local dirty", task.GoogleTaskId);
            var dirty = _appState.FindTaskById(task.Id);
            if (dirty != null)
            {
                var saved = dirty.WithUpdates(c => c.IsLocalDirty = true);
                await _taskRepository.SaveAsync(saved);
                _appState.UpdateTask(task.Id, t => t.IsLocalDirty = true);
            }
            return null;
        }
    }

    private static string SanitizeTaskName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        return HttpUtility.HtmlEncode(name.Trim());
    }

    private async Task LoadGoogleTasksSettingsAsync()
    {
        var settings = await _indexedDb.GetAsync<GoogleTasksSettings>(Constants.Storage.GoogleTasksSettingsStore, Constants.Storage.DefaultSettingsId);
        if (settings != null)
            _googleTasksSettings = settings;
    }

    private async Task SaveGoogleTasksSettingsAsync()
    {
        await _indexedDb.PutAsync(Constants.Storage.GoogleTasksSettingsStore, _googleTasksSettings);
    }

    public async Task UpdateListVisibilityAsync(string listId, bool isVisible)
    {
        var lists = new Dictionary<string, ListSetting>(_googleTasksSettings.Lists);
        if (lists.ContainsKey(listId))
        {
            lists[listId] = lists[listId] with { IsVisible = isVisible };
        }
        else
        {
            var cachedEntry = _cachedGoogleLists.FirstOrDefault(e => e.Id == listId);
            lists[listId] = new ListSetting(isVisible, cachedEntry?.Color ?? "var(--pomodoro-color)", null);
        }

        _googleTasksSettings = new GoogleTasksSettings(lists);
        await SaveGoogleTasksSettingsAsync();

        var entry = _cachedGoogleLists.FirstOrDefault(e => e.Id == listId);
        if (entry != null)
        {
            _cachedGoogleLists[_cachedGoogleLists.IndexOf(entry)] = entry with { IsVisible = isVisible };
        }

        NotifyStateChanged();
    }

    private async Task SaveGoogleListsCacheAsync()
    {
        var data = _cachedGoogleLists.Select(l => l.Id).ToList();
        _googleTasksSettings = _googleTasksSettings with { ListIds = data };
        await SaveGoogleTasksSettingsAsync();
    }

    private async Task RestoreCachedGoogleListsFromSettingsAsync()
    {
        if (_googleTasksSettings.ListIds is { Count: > 0 })
        {
            var listsDict = _googleTasksSettings.Lists;
            _cachedGoogleLists = _googleTasksSettings.ListIds.Select(id =>
            {
                var settingsEntry = listsDict.GetValueOrDefault(id);
                return new GoogleListCacheEntry(id, id, settingsEntry?.Color ?? "var(--pomodoro-color)", settingsEntry?.IsVisible ?? true);
            }).ToList();
        }
    }

    private void InvalidateSidecarCache()
    {
        _sidecarCacheDirty = true;
    }

    private async Task<Dictionary<string, PomodoroMeta>> GetSidecarCacheAsync()
    {
        if (_sidecarCache != null && !_sidecarCacheDirty)
            return _sidecarCache!;

        var allMeta = await _sidecarRepo.GetAllAsync();
        _sidecarCache = allMeta
            .Where(m => !string.IsNullOrEmpty(m.GoogleTaskId))
            .GroupBy(m => m.GoogleTaskId)
            .ToDictionary(g => g.Key, g => g.Last());
        _sidecarCacheDirty = false;
        return _sidecarCache;
    }

    public class AppStateRecord
    {
        public string Id { get; set; } = Constants.Storage.DefaultSettingsId;
        public Guid? CurrentTaskId { get; set; }
        public string? CurrentListId { get; set; }
    }

    public ValueTask DisposeAsync()
    {
        _midnightTimer?.Dispose();
        _midnightTimer = null;
        return ValueTask.CompletedTask;
    }
}
