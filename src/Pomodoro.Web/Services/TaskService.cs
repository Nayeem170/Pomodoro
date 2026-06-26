using System.Web;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class TaskService : ITaskService, ITimerEventSubscriber
{
    private const string ColorPalette = "#4285F4,#0B8043,#E67C73,#9C27B0,#F59E0B,#EC407A,#AB47BC,#FF5722,#795548";

    private readonly ITaskRepository _taskRepository;
    private readonly IIndexedDbService _indexedDb;
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleTasksService _googleTasksService;
    private readonly ILogger<TaskService> _logger;
    private readonly IPomodoroMetaRepository _sidecarRepo;

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
            var lists = new List<TaskListRef>
            {
                new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)",
                    allTasks.Count(t => !t.IsGoogleTask && !t.IsScheduled && !t.IsDeleted), true, true),
                new(Constants.TaskLists.ScheduleListId, "Schedule", "#eab308",
                    allTasks.Count(t => t.IsScheduled && !t.IsDeleted), true, true)
            };

            foreach (var entry in _cachedGoogleLists)
            {
                var count = allTasks.Count(t => t.GoogleListId == entry.Id && !t.IsDeleted);
                lists.Add(new TaskListRef(entry.Id, entry.Title, entry.Color, count, entry.IsVisible, false));
            }

            return lists;
        }
    }

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

        await ActivateDueRecurringAndScheduledTasks();

        if (await _googleTasksService.IsConnectedAsync())
        {
            await RefreshGoogleListsAsync();
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
            throw new NotSupportedException(Constants.Sync.TasksReadOnlyInPhase);
        }

        await AddTaskAsync(name);
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

        var taskToSave = existingTask.WithUpdates(c => c.Name = name);
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(task.Id, t => t.Name = name);
        NotifyStateChanged();
        MarkDirty();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;
        if (existingTask.IsGoogleTask) return;

        var taskToSave = existingTask.WithUpdates(c =>
        {
            c.IsDeleted = true;
            c.DeletedAt = DateTime.UtcNow;
        });
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(taskId, t =>
        {
            t.IsDeleted = true;
            t.DeletedAt = DateTime.UtcNow;
        });

        if (_appState.CurrentTaskId == taskId)
        {
            _appState.CurrentTaskId = null;
            await SaveCurrentTaskIdAsync();
        }

        NotifyStateChanged();
        MarkDirty();
    }

    public async Task CompleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;
        if (existingTask.IsGoogleTask) return;

        var isRecurring = existingTask.IsRecurring && existingTask.Repeat is { IsActive: true };

        if (isRecurring)
        {
            existingTask.Repeat!.LastCompletedDate = DateTime.UtcNow;
            var nextOccurrence = ComputeNextOccurrence(existingTask.Repeat!);
            existingTask.Repeat.NextOccurrence = nextOccurrence;

            if (nextOccurrence.HasValue)
            {
                var taskToSave = existingTask.WithUpdates(c =>
                {
                    c.IsCompleted = true;
                    c.Repeat!.LastCompletedDate = DateTime.UtcNow;
                    c.Repeat.NextOccurrence = nextOccurrence;
                });
                await SaveTaskAsync(taskToSave);
                _appState.UpdateTask(taskId, t =>
                {
                    t.IsCompleted = true;
                    t.Repeat!.LastCompletedDate = DateTime.UtcNow;
                    t.Repeat.NextOccurrence = nextOccurrence;
                });
                NotifyStateChanged();
                MarkDirty();
                return;
            }
        }

        var taskToSave2 = existingTask.WithUpdates(c => c.IsCompleted = true);
        await SaveTaskAsync(taskToSave2);

        _appState.UpdateTask(taskId, t => t.IsCompleted = true);
        NotifyStateChanged();
        MarkDirty();
    }

    public async Task UncompleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;
        if (existingTask.IsGoogleTask) return;

        var taskToSave = existingTask.WithUpdates(c => c.IsCompleted = false);
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(taskId, t => t.IsCompleted = false);
        NotifyStateChanged();
        MarkDirty();
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
        var allTasks = _appState.Tasks;
        IEnumerable<TaskItem> filtered = listId switch
        {
            Constants.TaskLists.LocalPomodoroListId => allTasks.Where(t => !t.IsGoogleTask && !t.IsScheduled && !t.IsDeleted),
            Constants.TaskLists.ScheduleListId => allTasks.Where(t => t.IsScheduled && !t.IsDeleted),
            _ => allTasks.Where(t => t.GoogleListId == listId && !t.IsDeleted)
        };

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
            InvalidateSidecarCache();

            if (remoteLists.Count > 0 &&
                !string.IsNullOrEmpty(_appState.CurrentListId) &&
                _appState.CurrentListId != Constants.TaskLists.LocalPomodoroListId &&
                _appState.CurrentListId != Constants.TaskLists.ScheduleListId &&
                !updatedCache.Any(l => l.Id == _appState.CurrentListId))
            {
                await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
            }

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
                        var local = existingInList.FirstOrDefault(t => t.GoogleTaskId == gTask.Id)
                            ?? _appState.Tasks.FirstOrDefault(t => t.GoogleTaskId == gTask.Id);
                        if (local != null)
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
                                t.IsDeleted = false;
                                t.DeletedAt = null;
                            });
                        }
                        else
                        {
                            var newTask = MapGoogleTaskToTaskItem(gTask, gList.Id);
                            await _taskRepository.SaveAsync(newTask);
                            _appState.InsertTask(newTask, Constants.Tasks.InsertAtEnd);
                        }
                    }

                    foreach (var orphan in existingInList.Where(t => !remoteIds.Contains(t.GoogleTaskId)))
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

    private static DateTime? ComputeNextOccurrence(RepeatRule rule)
    {
        if (rule.Type == RepeatType.None) return null;
        if (rule.EndDate.HasValue && rule.EndDate.Value < DateTime.UtcNow.Date) return null;

        var baseDate = rule.LastCompletedDate ?? DateTime.UtcNow.Date;

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
        var current = baseDate.DayOfWeek;

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

    private async Task ActivateDueRecurringAndScheduledTasks()
    {
        var today = DateTime.UtcNow.Date;
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
                    task.TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes;
                    task.PomodoroCount = Constants.Tasks.InitialPomodoroCount;
                    task.LastWorkedOn = null;
                    changed = true;
                }
            }

            if (task.IsScheduled && task.IsCompleted && task.ScheduledDate.HasValue && task.ScheduledDate.Value <= today)
            {
                task.IsCompleted = false;
                changed = true;
            }
        }

        if (changed)
        {
            await SaveAsync();
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

        if (!isVisible && _appState.CurrentListId == listId)
        {
            var fallback = _cachedGoogleLists.FirstOrDefault(l => l.IsVisible);
            if (fallback != null)
                await SelectListAsync(fallback.Id);
            else
                await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
        }

        NotifyStateChanged();
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
        _sidecarCache = allMeta.ToDictionary(m => m.GoogleTaskId);
        _sidecarCacheDirty = false;
        return _sidecarCache;
    }

    public class AppStateRecord
    {
        public string Id { get; set; } = Constants.Storage.DefaultSettingsId;
        public Guid? CurrentTaskId { get; set; }
        public string? CurrentListId { get; set; }
    }
}
