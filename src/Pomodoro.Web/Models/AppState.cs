namespace Pomodoro.Web.Models;

/// <summary>
/// Central application state container
/// Thread-safe implementation for access from multiple services and timer callbacks
/// </summary>
public class AppState
{
    private readonly object _tasksLock = new();
    private List<TaskItem> _tasks = new();
    
    /// <summary>
    /// Event raised when state properties change
    /// </summary>
    public event Action? OnStateChanged;
    
    /// <summary>
    /// Thread-safe access to tasks list
    /// Returns a copy to prevent external modifications
    /// </summary>
    public IReadOnlyList<TaskItem> Tasks
    {
        get
        {
            lock (_tasksLock)
            {
                return _tasks.ToList();
            }
        }
        set
        {
            lock (_tasksLock)
            {
                _tasks = (value as List<TaskItem>) ?? value?.ToList() ?? new List<TaskItem>();
            }
            NotifyStateChanged();
        }
    }
    
    /// <summary>
    /// Inserts a task at the specified position (thread-safe)
    /// </summary>
    public void InsertTask(TaskItem task, int position = 0)
    {
        lock (_tasksLock)
        {
            _tasks.Insert(position, task);
        }
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Finds a task by ID and performs an update action (thread-safe)
    /// Returns true if the task was found and updated
    /// </summary>
    public bool UpdateTask(Guid taskId, Action<TaskItem> updateAction)
    {
        lock (_tasksLock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                updateAction(task);
                return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Finds a task by ID (thread-safe, returns a copy)
    /// </summary>
    public TaskItem? FindTaskById(Guid taskId)
    {
        lock (_tasksLock)
        {
            return _tasks.FirstOrDefault(t => t.Id == taskId);
        }
    }
    
    private Guid? _currentTaskId;
    public Guid? CurrentTaskId
    {
        get => _currentTaskId;
        set
        {
            if (_currentTaskId != value)
            {
                _currentTaskId = value;
                NotifyStateChanged();
            }
        }
    }
    
    private TimerSettings _settings = new();
    public TimerSettings Settings
    {
        get => _settings;
        set
        {
            if (_settings != value)
            {
                _settings = value ?? new TimerSettings();
                NotifyStateChanged();
            }
        }
    }
    
    private TimerSession? _currentSession;
    public TimerSession? CurrentSession
    {
        get => _currentSession;
        set
        {
            if (_currentSession != value)
            {
                _currentSession = value;
                NotifyStateChanged();
            }
        }
    }
    
    // Today's statistics
    private int _todayTotalFocusMinutes;
    public int TodayTotalFocusMinutes
    {
        get => _todayTotalFocusMinutes;
        set
        {
            if (_todayTotalFocusMinutes != value)
            {
                _todayTotalFocusMinutes = value;
                NotifyStateChanged();
            }
        }
    }
    
    private int _todayPomodoroCount;
    public int TodayPomodoroCount
    {
        get => _todayPomodoroCount;
        set
        {
            if (_todayPomodoroCount != value)
            {
                _todayPomodoroCount = value;
                NotifyStateChanged();
            }
        }
    }
    
    private readonly object _todayTaskIdsLock = new();
    private List<Guid> _todayTaskIdsWorkedOn = new();
    
    /// <summary>
    /// Thread-safe access to today's task IDs
    /// </summary>
    public List<Guid> TodayTaskIdsWorkedOn
    {
        get
        {
            lock (_todayTaskIdsLock)
            {
                return _todayTaskIdsWorkedOn.ToList();
            }
        }
        set
        {
            lock (_todayTaskIdsLock)
            {
                _todayTaskIdsWorkedOn = value ?? new List<Guid>();
            }
            NotifyStateChanged();
        }
    }
    
    private DateTime? _lastResetDate;
    public DateTime? LastResetDate
    {
        get => _lastResetDate;
        set
        {
            if (_lastResetDate != value)
            {
                _lastResetDate = value;
                NotifyStateChanged();
            }
        }
    }
    
    /// <summary>
    /// Gets the currently selected task (thread-safe)
    /// </summary>
    public TaskItem? CurrentTask
    {
        get
        {
            lock (_tasksLock)
            {
                return CurrentTaskId.HasValue
                    ? _tasks.FirstOrDefault(t => t.Id == CurrentTaskId.Value)
                    : null;
            }
        }
    }
    
    /// <summary>
    /// Gets the count of unique tasks worked on today (thread-safe)
    /// </summary>
    public int TodayTasksWorkedOn
    {
        get
        {
            lock (_todayTaskIdsLock)
            {
                return _todayTaskIdsWorkedOn.Distinct().Count();
            }
        }
    }
    
    /// <summary>
    /// Adds a task ID to today's worked-on list (thread-safe)
    /// </summary>
    public void AddTodayTaskId(Guid taskId)
    {
        lock (_todayTaskIdsLock)
        {
            if (!_todayTaskIdsWorkedOn.Contains(taskId))
            {
                _todayTaskIdsWorkedOn.Add(taskId);
            }
        }
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Gets the current "day" key for daily stats.
    /// Uses local midnight for daily reset (e.g., midnight in the user's timezone).
    /// Note: For users in Bangladesh (UTC+6), this means stats reset at 6 PM UTC.
    /// </summary>
    public static DateTime GetCurrentDayKey()
    {
        // Use local midnight for daily reset
        return DateTime.Now.Date;
    }
    
    /// <summary>
    /// Checks if today's stats need to be reset.
    /// Returns true when the local date has changed (crossed local midnight).
    /// </summary>
    public bool NeedsDailyReset()
    {
        if (LastResetDate == null)
            return true;
        
        // Reset when the local date changes (at local midnight)
        return LastResetDate.Value < GetCurrentDayKey();
    }
    
    /// <summary>
    /// Resets daily statistics (thread-safe)
    /// </summary>
    public void ResetDailyStats()
    {
        TodayTotalFocusMinutes = 0;
        TodayPomodoroCount = 0;
        lock (_todayTaskIdsLock)
        {
            _todayTaskIdsWorkedOn = new List<Guid>();
        }
        LastResetDate = GetCurrentDayKey();
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Raises the OnStateChanged event
    /// </summary>
    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
