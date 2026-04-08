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

    public event Action? OnChange;
    
    public List<TaskItem> Tasks => _appState.Tasks.Where(t => !t.IsDeleted).ToList();
    public IReadOnlyList<TaskItem> AllTasks => _appState.Tasks; // Includes soft-deleted tasks for history
    public Guid? CurrentTaskId => _appState.CurrentTaskId;
    public TaskItem? CurrentTask => _appState.CurrentTask;

    public TaskService(ITaskRepository taskRepository, IIndexedDbService indexedDb, AppState appState)
    {
        _taskRepository = taskRepository;
        _indexedDb = indexedDb;
        _appState = appState;
    }

    public async Task InitializeAsync()
    {
        // Load tasks from repository
        var tasks = await _taskRepository.GetAllIncludingDeletedAsync();
        if (tasks != null && tasks.Count > 0)
        {
            _appState.Tasks = tasks;
        }
        
        // Load current task from app state store
        var appState = await _indexedDb.GetAsync<AppStateRecord>(Constants.Storage.AppStateStore, Constants.Storage.DefaultSettingsId);
        if (appState?.CurrentTaskId.HasValue == true)
        {
            var taskId = appState.CurrentTaskId.Value;
            // Use thread-safe access to check if task exists
            if (_appState.Tasks.Any(t => t.Id == taskId))
            {
                _appState.CurrentTaskId = taskId;
            }
        }
        
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
        
        // Insert task at beginning (thread-safe via AppState method)
        _appState.InsertTask(task, Constants.Tasks.InsertAtBeginning);
        
        // Auto-select the new task
        _appState.CurrentTaskId = task.Id;
        
        await SaveTaskAsync(task);
        await SaveCurrentTaskIdAsync();
        NotifyStateChanged();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        var sanitized = SanitizeTaskName(task.Name ?? string.Empty);
        
        // Validate that the task name is not empty after sanitization and within length limit
        if (string.IsNullOrEmpty(sanitized) || sanitized.Length > Constants.UI.MaxTaskNameLength)
        {
            return;
        }
        
        // Thread-safe task update via AppState method
        var updated = _appState.UpdateTask(task.Id, t => t.Name = sanitized);
        
        if (updated)
        {
            var updatedTask = _appState.FindTaskById(task.Id);
            if (updatedTask != null)
            {
                await SaveTaskAsync(updatedTask);
            }
            NotifyStateChanged();
        }
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var updated = _appState.UpdateTask(taskId, t =>
        {
            // Soft delete - mark as deleted but keep for history
            t.IsDeleted = true;
            t.DeletedAt = DateTime.UtcNow;
        });
        
        if (updated)
        {
            if (_appState.CurrentTaskId == taskId)
            {
                _appState.CurrentTaskId = null;
                await SaveCurrentTaskIdAsync();
            }
            
            var deletedTask = _appState.FindTaskById(taskId);
            if (deletedTask != null)
            {
                await SaveTaskAsync(deletedTask);
            }
            NotifyStateChanged();
        }
    }

    public async Task CompleteTaskAsync(Guid taskId)
    {
        var updated = _appState.UpdateTask(taskId, t => t.IsCompleted = true);
        
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

    public async Task UncompleteTaskAsync(Guid taskId)
    {
        var updated = _appState.UpdateTask(taskId, t => t.IsCompleted = false);
        
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

    /// <summary>
    /// Sanitizes task name by trimming and HTML-encoding to prevent XSS attacks.
    /// Blazor generally escapes content automatically, but this provides defense-in-depth.
    /// </summary>
    private static string SanitizeTaskName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        
        // Trim whitespace
        var trimmed = name.Trim();
        
        // HTML encode to prevent XSS (defense-in-depth, Blazor escapes automatically)
        return HttpUtility.HtmlEncode(trimmed);
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
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
