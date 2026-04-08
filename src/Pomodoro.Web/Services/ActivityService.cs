using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for managing activity history records using IndexedDB
/// Stores unlimited history in IndexedDB but keeps a sliding window cache in memory
/// Implements ITimerEventSubscriber to handle timer completion events
/// </summary>
public partial class ActivityService : IActivityService, ITimerEventSubscriber
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<ActivityService> _logger;
    private readonly object _cacheLock = new();
    private List<ActivityRecord> _cachedActivities = new();
    private bool _isCacheLoaded;
    
    /// <summary>
    /// Date-based cache for activities grouped by local date
    /// </summary>
    private Dictionary<DateTime, List<ActivityRecord>> _activitiesByDate = new();
    
    /// <summary>
    /// Cache for daily statistics (pomodoro count, focus minutes, break minutes)
    /// </summary>
    private readonly struct DailyStatsCache
    {
        public int PomodoroCount { get; init; }
        public int FocusMinutes { get; init; }
        public int BreakMinutes { get; init; }
    }
    private Dictionary<DateTime, DailyStatsCache> _dailyStatsCache = new();
    
    /// <summary>
    /// Cache for time distribution data by date
    /// </summary>
    private Dictionary<DateTime, Dictionary<string, int>> _timeDistributionCache = new();
    
    public event Action? OnActivityChanged;
    
    public ActivityService(IActivityRepository activityRepository, ILogger<ActivityService> logger)
    {
        _activityRepository = activityRepository;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        await LoadCacheAsync();
    }
    
    /// <summary>
    /// Reloads all activity data from storage, clearing and rebuilding caches.
    /// Called after import operations to refresh in-memory data.
    /// </summary>
    public async Task ReloadAsync()
    {
        var activities = await _activityRepository.GetAllAsync();
        
        lock (_cacheLock)
        {
            _cachedActivities = activities.Take(Constants.Cache.MaxActivityCacheSize).ToList();
            _isCacheLoaded = true;
            ClearAllDerivedCaches();
        }
        
        _logger.LogInformation("Reloaded {Count} activities from storage", _cachedActivities.Count);
        OnActivityChanged?.Invoke();
    }
    
    private async Task LoadCacheAsync()
    {
        if (_isCacheLoaded) return;
        
        var activities = await _activityRepository.GetAllAsync();
        
        // Apply cache size limit - only keep most recent activities in memory
        lock (_cacheLock)
        {
            _cachedActivities = (activities ?? Enumerable.Empty<ActivityRecord>()).Take(Constants.Cache.MaxActivityCacheSize).ToList();
            _isCacheLoaded = true;
        }
        
        _logger.LogDebug(Constants.Messages.LogActivitiesLoadedFormat, _cachedActivities.Count, Constants.Cache.MaxActivityCacheSize);
        foreach (var a in _cachedActivities.Take(5))
        {
            _logger.LogDebug(Constants.Messages.LogActivityDebugFormat, a.Type, a.CompletedAt, a.TaskName);
        }
    }
    
    public List<ActivityRecord> GetTodayActivities()
    {
        // Use local time for consistent date comparison (matches GetActivitiesForDate behavior)
        var todayLocal = DateTime.Now.Date;
        return GetActivitiesForDate(todayLocal);
    }
    
    /// <summary>
    /// Gets paged activities for a date range
    /// </summary>
    public async Task<List<ActivityRecord>> GetActivitiesPagedAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 20)
    {
        return await _activityRepository.GetPagedAsync(startDate, endDate, skip, take);
    }
    
    /// <summary>
    /// Gets the total count of activities for a date range
    /// </summary>
    public async Task<int> GetActivityCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _activityRepository.GetCountAsync(startDate, endDate);
    }
    
    public List<ActivityRecord> GetAllActivities()
    {
        lock (_cacheLock)
        {
            return _cachedActivities.ToList();
        }
    }
    
    /// <summary>
    /// Gets all activities for a specific date (in local time)
    /// Uses date-based cache for O(1) lookup on repeated access
    /// </summary>
    public List<ActivityRecord> GetActivitiesForDate(DateTime date)
    {
        var targetDate = date.Date;
        
        lock (_cacheLock)
        {
            // Check cache first
            if (_activitiesByDate.TryGetValue(targetDate, out var cached))
            {
                _logger.LogDebug("Cache hit for GetActivitiesForDate: {Date}", targetDate);
                return cached;
            }
            
            // Compute and cache
            var result = _cachedActivities
                .Where(a => a.CompletedAt.ToLocalTime().Date == targetDate)
                .OrderByDescending(a => a.CompletedAt)
                .ToList();
            
            _activitiesByDate[targetDate] = result;
            _logger.LogDebug("Cache miss for GetActivitiesForDate: {Date}, cached {Count} activities", targetDate, result.Count);
            return result;
        }
    }
    
    /// <summary>
    /// Gets daily break minutes for a date range (in local time)
    /// Returns total minutes of both short and long breaks per day
    /// Uses date-based cache for improved performance
    /// </summary>

    /// This is more efficient than computing each date separately when querying a range.
    /// </summary>
    /// <param name="dates">The dates to compute stats for</param>
    private void ComputeDailyStatsForRange(List<DateTime> dates)
    {
        // Initialize stats for all dates
        var statsByDate = new Dictionary<DateTime, (int PomodoroCount, int FocusMinutes, int BreakMinutes)>();
        foreach (var date in dates)
        {
            statsByDate[date.Date] = (0, 0, 0);
        }
        
        // Single pass through all activities
        foreach (var a in _cachedActivities)
        {
            var activityDate = a.CompletedAt.ToLocalTime().Date;
            
            // Only process if this date is in our target list
            if (!statsByDate.ContainsKey(activityDate)) continue;
            
            var stats = statsByDate[activityDate];
            if (a.Type == SessionType.Pomodoro)
            {
                statsByDate[activityDate] = (stats.PomodoroCount + 1, stats.FocusMinutes + a.DurationMinutes, stats.BreakMinutes);
            }
            else if (a.Type == SessionType.ShortBreak || a.Type == SessionType.LongBreak)
            {
                statsByDate[activityDate] = (stats.PomodoroCount, stats.FocusMinutes, stats.BreakMinutes + a.DurationMinutes);
            }
        }
        
        // Store in cache
        foreach (var kvp in statsByDate)
        {
            _dailyStatsCache[kvp.Key] = new DailyStatsCache
            {
                PomodoroCount = kvp.Value.PomodoroCount,
                FocusMinutes = kvp.Value.FocusMinutes,
                BreakMinutes = kvp.Value.BreakMinutes
            };
        }
        
        _logger.LogDebug("Computed stats for {Count} dates in single pass", dates.Count);
    }
    
    /// <summary>
    /// Computes daily statistics for a single date using single-pass iteration
    /// </summary>
    private DailyStatsCache ComputeDailyStats(DateTime date)
    {
        var targetDate = date.Date;
        var pomodoroCount = 0;
        var focusMinutes = 0;
        var breakMinutes = 0;
        
        // Single-pass iteration for better performance
        foreach (var a in _cachedActivities)
        {
            if (a.CompletedAt.ToLocalTime().Date != targetDate) continue;
            
            if (a.Type == SessionType.Pomodoro)
            {
                pomodoroCount++;
                focusMinutes += a.DurationMinutes;
            }
            else if (a.Type == SessionType.ShortBreak || a.Type == SessionType.LongBreak)
            {
                breakMinutes += a.DurationMinutes;
            }
        }
        
        return new DailyStatsCache
        {
            PomodoroCount = pomodoroCount,
            FocusMinutes = focusMinutes,
            BreakMinutes = breakMinutes
        };
    }
    
    /// <summary>
    /// Gets task pomodoro counts for a date range (in local time)
    /// </summary>
    public Dictionary<string, int> GetTaskPomodoroCounts(DateTime from, DateTime to)
    {
        var fromDate = from.Date;
        var toDate = to.Date;
        
        lock (_cacheLock)
        {
            return _cachedActivities
                .Where(a => a.Type == SessionType.Pomodoro &&
                            a.CompletedAt.ToLocalTime().Date >= fromDate &&
                            a.CompletedAt.ToLocalTime().Date <= toDate &&
                            !string.IsNullOrWhiteSpace(a.TaskName))
                .GroupBy(a => a.TaskName!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }
    }
    
    /// <summary>
    /// Gets time distribution data for a specific date (in local time)
    /// Returns a dictionary with labels (task names or break types) as keys and minutes as values
    /// Uses date-based cache for O(1) lookup on repeated access
    /// </summary>
    public Dictionary<string, int> GetTimeDistribution(DateTime date)
    {
        var targetDate = date.Date;
        
        lock (_cacheLock)
        {
            // Check cache first
            if (_timeDistributionCache.TryGetValue(targetDate, out var cached))
            {
                _logger.LogDebug("Cache hit for GetTimeDistribution: {Date}", targetDate);
                return cached;
            }
            
            // Compute and cache
            var dayActivities = _cachedActivities
                .Where(a => a.CompletedAt.ToLocalTime().Date == targetDate)
                .ToList();
            
            var result = new Dictionary<string, int>();
            
            // Group pomodoro sessions by task name
            var pomodoroByTask = dayActivities
                .Where(a => a.Type == SessionType.Pomodoro)
                .GroupBy(a => a.TaskName ?? Constants.Activity.FocusTimeLabel)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.DurationMinutes)
                );
            
            // Add task times to result
            foreach (var kvp in pomodoroByTask)
            {
                result[kvp.Key] = kvp.Value;
            }
            
            // Aggregate short breaks
            var shortBreakMinutes = dayActivities
                .Where(a => a.Type == SessionType.ShortBreak)
                .Sum(a => a.DurationMinutes);
            
            if (shortBreakMinutes > 0)
            {
                result[Constants.Activity.ShortBreaksLabel] = shortBreakMinutes;
            }
            
            // Aggregate long breaks
            var longBreakMinutes = dayActivities
                .Where(a => a.Type == SessionType.LongBreak)
                .Sum(a => a.DurationMinutes);
            
            if (longBreakMinutes > 0)
            {
                result[Constants.Activity.LongBreaksLabel] = longBreakMinutes;
            }
            
            _timeDistributionCache[targetDate] = result;
            _logger.LogDebug("Cache miss for GetTimeDistribution: {Date}, cached {Count} entries", targetDate, result.Count);
            return result;
        }
    }
    
    /// <summary>
    /// Gets weekly statistics for a given week start date
    /// </summary>
    public async Task<WeeklyStats> GetWeeklyStatsAsync(DateTime weekStartDate)
    {
        try
        {
            // Week starts on Saturday (matches History page calculation)
            // DayOfWeek: Sun=0, Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6
            // Using AddDays(7) because IDBKeyRange.bound() is inclusive, so end at Saturday midnight includes all Friday
            var weekStart = weekStartDate.Date;
            var weekEnd = weekStart.AddDays(7); // Saturday to Saturday midnight (includes Saturday through Friday)
            var previousWeekStart = weekStart.AddDays(-7);
            
            // Get activities for current week from repository (to handle large datasets)
            var currentWeekActivities = await _activityRepository.GetByDateRangeAsync(weekStart, weekEnd);
            var previousWeekActivities = await _activityRepository.GetByDateRangeAsync(previousWeekStart, weekStart);
        
        // Return zero stats if no activities for the week
        if (!currentWeekActivities.Any())
        {
            return new WeeklyStats
            {
                TotalFocusMinutes = 0,
                TotalPomodoroCount = 0,
                UniqueTasksWorkedOn = 0,
                DailyAverageMinutes = 0,
                MostProductiveDay = DayOfWeek.Monday,
                PreviousWeekFocusMinutes = 0,
                WeekOverWeekChange = 0
            };
        }
        
        // Calculate current week statistics
        var pomodoroActivities = currentWeekActivities.Where(a => a.Type == SessionType.Pomodoro).ToList();
        var totalFocusMinutes = pomodoroActivities.Sum(a => a.DurationMinutes);
        var totalPomodoroCount = pomodoroActivities.Count;
        var uniqueTasks = pomodoroActivities
            .Where(a => !string.IsNullOrEmpty(a.TaskName))
            .Select(a => a.TaskName)
            .Distinct()
            .Count();
        
        // Calculate daily average (divide by 7 for full week, or actual days with activity)
        var daysWithActivity = currentWeekActivities
            .Select(a => a.CompletedAt.ToLocalTime().Date)
            .Distinct()
            .Count();
        var dailyAverageMinutes = (double)totalFocusMinutes / daysWithActivity;
        
        // Find most productive day
        var dayTotals = pomodoroActivities
            .GroupBy(a => a.CompletedAt.ToLocalTime().DayOfWeek)
            .Select(g => new { Day = g.Key, Minutes = g.Sum(a => a.DurationMinutes) })
            .OrderByDescending(g => g.Minutes)
            .FirstOrDefault();
        var mostProductiveDay = dayTotals?.Day ?? DayOfWeek.Monday;
        
        // Calculate previous week focus minutes
        var previousWeekFocusMinutes = previousWeekActivities
            .Where(a => a.Type == SessionType.Pomodoro)
            .Sum(a => a.DurationMinutes);
        
        // Calculate week-over-week change percentage
        double weekOverWeekChange = 0;
        if (previousWeekFocusMinutes > 0)
        {
            weekOverWeekChange = ((double)(totalFocusMinutes - previousWeekFocusMinutes) / previousWeekFocusMinutes) * 100;
        }
        else if (totalFocusMinutes > 0)
        {
            // Previous week had no data, this week has data - treat as 100% increase
            weekOverWeekChange = 100;
        }
        
        return new WeeklyStats
        {
            TotalFocusMinutes = totalFocusMinutes,
            TotalPomodoroCount = totalPomodoroCount,
            UniqueTasksWorkedOn = uniqueTasks,
            DailyAverageMinutes = dailyAverageMinutes,
            MostProductiveDay = mostProductiveDay,
            PreviousWeekFocusMinutes = previousWeekFocusMinutes,
            WeekOverWeekChange = weekOverWeekChange
        };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating weekly stats for week starting {WeekStartDate}", weekStartDate);
            return new WeeklyStats
            {
                TotalFocusMinutes = 0,
                TotalPomodoroCount = 0,
                UniqueTasksWorkedOn = 0,
                DailyAverageMinutes = 0,
                MostProductiveDay = DayOfWeek.Monday,
                PreviousWeekFocusMinutes = 0,
                WeekOverWeekChange = 0
            };
        }
    }
    
    public async Task AddActivityAsync(ActivityRecord activity)
    {
        // Add to cache with thread safety and trim if needed
        lock (_cacheLock)
        {
            _cachedActivities.Insert(0, activity);
            
            // Trim cache if it exceeds max size (remove oldest entries from end)
            // Track dates of removed activities to invalidate their caches
            var datesToInvalidate = new HashSet<DateTime>();
            
            while (_cachedActivities.Count > Constants.Cache.MaxActivityCacheSize)
            {
                var removed = _cachedActivities[^1];
                datesToInvalidate.Add(removed.CompletedAt.ToLocalTime().Date);
                _cachedActivities.RemoveAt(_cachedActivities.Count - 1);
            }
            
            // Invalidate caches for all affected dates (removed activities + new activity)
            foreach (var date in datesToInvalidate)
            {
                InvalidateDateCache(date);
            }
            
            // Also invalidate the new activity's date
            InvalidateDateCache(activity.CompletedAt.ToLocalTime().Date);
        }
        
        _logger.LogDebug(Constants.Messages.LogAddedActivityFormat, activity.Type, activity.CompletedAt, _cachedActivities.Count);
        
        // Save to repository
        await _activityRepository.SaveAsync(activity);
        
        OnActivityChanged?.Invoke();
    }
    
    public async Task ClearAllActivitiesAsync()
    {
        lock (_cacheLock)
        {
            _cachedActivities.Clear();
            ClearAllDerivedCaches();
        }
        
        await _activityRepository.ClearAllAsync();
        OnActivityChanged?.Invoke();
    }
    
    /// <summary>
    /// Gets activities for a date range directly from IndexedDB
    /// Useful for large datasets where caching everything isn't practical
    /// </summary>
    public async Task<List<ActivityRecord>> GetActivitiesByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _activityRepository.GetByDateRangeAsync(from, to);
    }

    /// <summary>
    /// Handles timer completion events from ITimerEventSubscriber
    /// Creates an activity record for the completed session
    /// </summary>
    public async Task HandleTimerCompletedAsync(TimerCompletedEventArgs args)
    {
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = args.SessionType,
            TaskId = args.TaskId,
            TaskName = args.TaskName,
            CompletedAt = args.CompletedAt,
            DurationMinutes = args.DurationMinutes,
            WasCompleted = args.WasCompleted
        };
        
        await AddActivityAsync(activity);
    }
    
    #region Cache Management
    
    /// <summary>
    /// Invalidates all cached data for a specific date
    /// </summary>
    private void InvalidateDateCache(DateTime date)
    {
        var localDate = date.Date;
        _activitiesByDate.Remove(localDate);
        _dailyStatsCache.Remove(localDate);
        _timeDistributionCache.Remove(localDate);
        _logger.LogDebug("Invalidated cache for date: {Date}", localDate);
    }
    
    /// <summary>
    /// Clears all derived caches (called when primary cache is cleared)
    /// </summary>
    private void ClearAllDerivedCaches()
    {
        _activitiesByDate.Clear();
        _dailyStatsCache.Clear();
        _timeDistributionCache.Clear();
        _logger.LogDebug("Cleared all derived caches");
    }
    
    /// <summary>
    /// Gets cache statistics for debugging/monitoring
    /// </summary>
    public (int ActivityCount, int DatesCached, int StatsCached, int DistributionCached) GetCacheStatistics()
    {
        lock (_cacheLock)
        {
            return (
                _cachedActivities.Count,
                _activitiesByDate.Count,
                _dailyStatsCache.Count,
                _timeDistributionCache.Count
            );
        }
    }
    
    /// <summary>
    /// Invalidates cached data for a specific activity by its date.
    /// Useful for future extensibility when activities can be modified or deleted individually.
    /// </summary>
    /// <param name="activityId">The ID of the activity that was modified/deleted</param>
    public void InvalidateCacheForActivity(Guid activityId)
    {
        lock (_cacheLock)
        {
            // Find the activity in cache to get its date
            var activity = _cachedActivities.FirstOrDefault(a => a.Id == activityId);
            if (activity != null)
            {
                InvalidateDateCache(activity.CompletedAt.ToLocalTime().Date);
                _logger.LogDebug("Invalidated cache for activity {ActivityId} on date {Date}", activityId, activity.CompletedAt.ToLocalTime().Date);
            }
        }
    }
    
    /// <summary>
    /// Invalidates cached data for a date range.
    /// Useful for bulk operations that affect multiple days.
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    public void InvalidateCacheForDateRange(DateTime from, DateTime to)
    {
        lock (_cacheLock)
        {
            var count = 0;
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                _activitiesByDate.Remove(date);
                _dailyStatsCache.Remove(date);
                _timeDistributionCache.Remove(date);
                count++;
            }
            _logger.LogDebug("Invalidated cache for {Count} dates from {From} to {To}", count, from.Date, to.Date);
        }
    }
    
    #endregion
}
