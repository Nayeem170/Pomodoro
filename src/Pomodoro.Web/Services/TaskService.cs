using System.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for managing tasks using IndexedDB for persistent storage
/// Implements ITimerEventSubscriber to handle timer completion events
/// </summary>
public class TaskService : ITaskService, ITimerEventSubscriber
{
    private readonly ITaskRepository _taskRepository;
    private readonly IIndexedDbService _indexedDb;
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;

    public event Action? OnChange;

    public List<TaskItem> Tasks => _appState.Tasks.Where(t => !t.IsDeleted).ToList();
    public IReadOnlyList<TaskItem> AllTasks => _appState.Tasks; // Includes soft-deleted tasks for history
    public Guid? CurrentTaskId => _appState.CurrentTaskId;
    public TaskItem? CurrentTask => _appState.CurrentTask;

    public TaskService(ITaskRepository taskRepository, IIndexedDbService indexedDb, AppState appState, IServiceProvider serviceProvider)
    {
        _taskRepository = taskRepository;
        _indexedDb = indexedDb;
        _appState = appState;
        _serviceProvider = serviceProvider;
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

        await ActivateDueRecurringAndScheduledTasks();
        NotifyStateChanged();
    }

    /// <summary>
    /// Reloads all task data from storage, refreshing the in-memory cache.
    /// Call this after import operations to reflect changes.
    /// </summary>
    public async Task ReloadAsync()
    {
        // Reload tasks from repository
        var tasks = await _taskRepository.GetAllIncludingDeletedAsync();
        _appState.Tasks = tasks ?? new List<TaskItem>();

        // Clear current task selection if the task no longer exists
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

        // Validate that the task name is not empty after sanitization and within length limit
        if (string.IsNullOrEmpty(sanitized) || sanitized.Length > Constants.UI.MaxTaskNameLength)
        {
            return;
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = sanitized,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false,
            TotalFocusMinutes = Constants.Tasks.InitialFocusMinutes,
            PomodoroCount = Constants.Tasks.InitialPomodoroCount
        };

        // Persist first to ensure cache and storage stay consistent
        await SaveTaskAsync(task);

        // Update in-memory state only after successful persistence
        _appState.InsertTask(task, Constants.Tasks.InsertAtBeginning);
        _appState.CurrentTaskId = task.Id;
        await SaveCurrentTaskIdAsync();
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

        var taskToSave = new TaskItem
        {
            Id = existingTask.Id,
            Name = name,
            CreatedAt = existingTask.CreatedAt,
            IsCompleted = existingTask.IsCompleted,
            TotalFocusMinutes = existingTask.TotalFocusMinutes,
            PomodoroCount = existingTask.PomodoroCount,
            LastWorkedOn = existingTask.LastWorkedOn,
            IsDeleted = existingTask.IsDeleted,
            DeletedAt = existingTask.DeletedAt,
            Repeat = existingTask.Repeat,
            ScheduledDate = existingTask.ScheduledDate
        };
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(task.Id, t => t.Name = name);
        NotifyStateChanged();
        MarkDirty();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;

        var taskToSave = new TaskItem
        {
            Id = existingTask.Id,
            Name = existingTask.Name,
            CreatedAt = existingTask.CreatedAt,
            IsCompleted = existingTask.IsCompleted,
            TotalFocusMinutes = existingTask.TotalFocusMinutes,
            PomodoroCount = existingTask.PomodoroCount,
            LastWorkedOn = existingTask.LastWorkedOn,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            Repeat = existingTask.Repeat,
            ScheduledDate = existingTask.ScheduledDate
        };
        await SaveTaskAsync(taskToSave);

        // Update in-memory state only after successful persistence
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

        var isRecurring = existingTask.IsRecurring && existingTask.Repeat is { IsActive: true };

        if (isRecurring)
        {
            existingTask.Repeat!.LastCompletedDate = DateTime.UtcNow;
            var nextOccurrence = ComputeNextOccurrence(existingTask.Repeat!);
            existingTask.Repeat.NextOccurrence = nextOccurrence;

            if (nextOccurrence.HasValue)
            {
                existingTask.IsCompleted = true;
                await SaveTaskAsync(existingTask);
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

        var taskToSave = new TaskItem
        {
            Id = existingTask.Id,
            Name = existingTask.Name,
            CreatedAt = existingTask.CreatedAt,
            IsCompleted = true,
            TotalFocusMinutes = existingTask.TotalFocusMinutes,
            PomodoroCount = existingTask.PomodoroCount,
            LastWorkedOn = existingTask.LastWorkedOn,
            IsDeleted = existingTask.IsDeleted,
            DeletedAt = existingTask.DeletedAt,
            Repeat = existingTask.Repeat,
            ScheduledDate = existingTask.ScheduledDate
        };
        await SaveTaskAsync(taskToSave);

        _appState.UpdateTask(taskId, t => t.IsCompleted = true);
        NotifyStateChanged();
        MarkDirty();
    }

    public async Task UncompleteTaskAsync(Guid taskId)
    {
        var existingTask = _appState.FindTaskById(taskId);
        if (existingTask == null) return;

        var taskToSave = new TaskItem
        {
            Id = existingTask.Id,
            Name = existingTask.Name,
            CreatedAt = existingTask.CreatedAt,
            IsCompleted = false,
            TotalFocusMinutes = existingTask.TotalFocusMinutes,
            PomodoroCount = existingTask.PomodoroCount,
            LastWorkedOn = existingTask.LastWorkedOn,
            IsDeleted = existingTask.IsDeleted,
            DeletedAt = existingTask.DeletedAt,
            Repeat = existingTask.Repeat,
            ScheduledDate = existingTask.ScheduledDate
        };
        await SaveTaskAsync(taskToSave);

        // Update in-memory state only after successful persistence
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
        // Validate that minutes is a positive value
        if (minutes <= 0)
        {
            return;
        }

        var updated = _appState.UpdateTask(taskId, t =>
        {
            t.TotalFocusMinutes += minutes;
            t.PomodoroCount++;
            t.LastWorkedOn = DateTime.UtcNow;
        });

        if (updated)
        {
            var task = _appState.FindTaskById(taskId);
            if (task != null)
            {
                await SaveTaskAsync(task);
            }
            NotifyStateChanged();
        }
    }

    public async Task SaveAsync()
    {
        // Save all tasks to IndexedDB (thread-safe copy)
        var tasksToSave = _appState.Tasks.ToList();
        await _indexedDb.PutAllAsync(Constants.Storage.TasksStore, tasksToSave);
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
            CurrentTaskId = _appState.CurrentTaskId
        };
        await _indexedDb.PutAsync(Constants.Storage.AppStateStore, appStateRecord);
    }

    /// <summary>
    /// Handles timer completion events from ITimerEventSubscriber
    /// Updates task time when a pomodoro completes
    /// </summary>
    public async Task HandleTimerCompletedAsync(TimerCompletedEventArgs args)
    {
        // Only process completed pomodoro sessions with a task
        if (args.SessionType != SessionType.Pomodoro || !args.TaskId.HasValue)
            return;

        await AddTimeToTaskAsync(args.TaskId.Value, args.DurationMinutes);
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

    /// <summary>
    /// Record for storing app state in IndexedDB
    /// </summary>
    public class AppStateRecord
    {
        public string Id { get; set; } = Constants.Storage.DefaultSettingsId;
        public Guid? CurrentTaskId { get; set; }
    }
}
