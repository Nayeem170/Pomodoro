using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository interface for activity record persistence operations
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// Gets all activities
    /// </summary>
    Task<List<ActivityRecord>> GetAllAsync();
    
    /// <summary>
    /// Gets activities for a date range
    /// </summary>
    Task<List<ActivityRecord>> GetByDateRangeAsync(DateTime start, DateTime end);
    
    /// <summary>
    /// Gets activities with pagination
    /// </summary>
    Task<List<ActivityRecord>> GetPagedAsync(DateTime start, DateTime end, int skip, int take);
    
    /// <summary>
    /// Gets an activity by ID
    /// </summary>
    Task<ActivityRecord?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Saves an activity (insert or update)
    /// </summary>
    Task<bool> SaveAsync(ActivityRecord activity);
    
    /// <summary>
    /// Gets count of activities in a date range
    /// </summary>
    Task<int> GetCountAsync(DateTime? start = null, DateTime? end = null);
    
    /// <summary>
    /// Clears all activities
    /// </summary>
    Task<bool> ClearAllAsync();
}
