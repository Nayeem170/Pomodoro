using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <remarks>
/// <para>
/// This interface provides methods for tracking and querying completed Pomodoro sessions,
/// breaks, and other focus activities. It supports both real-time queries from the in-memory
/// cache and persisted storage via IndexedDB.
/// </para>
/// <para>
/// Activities are cached in memory for fast access, with a maximum cache size defined
/// by <see cref="Constants.Cache.MaxActivityCacheSize"/>.
/// </para>
/// </remarks>
public interface IActivityService
{
    /// <remarks>
    /// Subscribe to this event to refresh UI components that display activity history.
    /// </remarks>
    event Action? OnActivityChanged;

    /// <returns>A list of <see cref="ActivityRecord"/> instances for today.</returns>
    /// <remarks>
    /// This method uses the in-memory cache for fast access and filters by the current date
    /// in the user's local time zone.
    /// </remarks>
    List<ActivityRecord> GetTodayActivities();

    /// <returns>A list of all cached <see cref="ActivityRecord"/> instances.</returns>
    /// <remarks>
    /// This returns activities from the in-memory cache, which may not include
    /// older activities that have been evicted due to cache size limits.
    /// </remarks>
    List<ActivityRecord> GetAllActivities();

    /// <param name="date">The date to filter activities by (in local time).</param>
    /// <returns>A list of <see cref="ActivityRecord"/> instances for the specified date.</returns>
    List<ActivityRecord> GetActivitiesForDate(DateTime date);

    /// <param name="startDate">The start date of the range (inclusive).</param>
    /// <param name="endDate">The end date of the range (inclusive).</param>
    /// <param name="skip">Number of records to skip for pagination.</param>
    /// <param name="take">Maximum number of records to return.</param>
    /// <returns>A task that resolves to a list of <see cref="ActivityRecord"/> instances.</returns>
    /// <remarks>
    /// This method queries IndexedDB directly for efficient pagination of large datasets.
    /// Results are ordered by completion date in descending order (newest first).
    /// </remarks>
    Task<List<ActivityRecord>> GetActivitiesPagedAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 20);

    /// <param name="startDate">Optional start date (inclusive). If null, counts from the beginning.</param>
    /// <param name="endDate">Optional end date (inclusive). If null, counts to the end.</param>
    /// <returns>A task that resolves to the count of matching activities.</returns>
    Task<int> GetActivityCountAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <param name="from">Start date of the range.</param>
    /// <param name="to">End date of the range.</param>
    /// <returns>A dictionary mapping dates to Pomodoro counts.</returns>
    /// <remarks>
    /// Only counts completed Pomodoro sessions, not breaks or other activities.
    /// </remarks>
    Dictionary<DateTime, int> GetDailyPomodoroCounts(DateTime from, DateTime to);

    /// <param name="from">Start date of the range.</param>
    /// <param name="to">End date of the range.</param>
    /// <returns>A dictionary mapping dates to total focus minutes.</returns>
    /// <remarks>
    /// Focus minutes represent actual work time from completed Pomodoro sessions.
    /// </remarks>
    Dictionary<DateTime, int> GetDailyFocusMinutes(DateTime from, DateTime to);

    /// <param name="from">Start date of the range.</param>
    /// <param name="to">End date of the range.</param>
    /// <returns>A dictionary mapping task names to Pomodoro counts.</returns>
    Dictionary<string, int> GetTaskPomodoroCounts(DateTime from, DateTime to);

    /// <param name="date">The date to analyze.</param>
    /// <returns>A dictionary with activity labels as keys and minutes as values.</returns>
    /// <remarks>
    /// Returns a breakdown of time spent on Pomodoros, short breaks, and long breaks.
    /// Useful for displaying pie charts or time breakdown visualizations.
    /// </remarks>
    Dictionary<string, int> GetTimeDistribution(DateTime date);

    /// <param name="from">Start date of the range.</param>
    /// <param name="to">End date of the range.</param>
    /// <returns>A dictionary mapping dates to total break minutes.</returns>
    /// <remarks>
    /// Returns total minutes of both short and long breaks per day.
    /// </remarks>
    Dictionary<DateTime, int> GetDailyBreakMinutes(DateTime from, DateTime to);

    /// <param name="activity">The activity record to add.</param>
    /// <returns>A task that completes when the activity is saved.</returns>
    /// <remarks>
    /// The activity is added to both the in-memory cache and persisted to IndexedDB.
    /// Triggers the <see cref="OnActivityChanged"/> event.
    /// </remarks>
    Task AddActivityAsync(ActivityRecord activity);

    /// <returns>A task that completes when all activities are cleared.</returns>
    /// <remarks>
    /// This operation is irreversible. Use with caution.
    /// Triggers the <see cref="OnActivityChanged"/> event.
    /// </remarks>
    Task ClearAllActivitiesAsync();

    /// <returns>A task that completes when initialization is finished.</returns>
    /// <remarks>
    /// This method should be called during application startup to populate
    /// the in-memory cache from IndexedDB.
    /// </remarks>
    Task InitializeAsync();

    /// <returns>A task that completes when the reload is finished.</returns>
    /// <remarks>
    /// Call this method after importing data or when external changes to the database
    /// need to be reflected in the application. Triggers <see cref="OnActivityChanged"/> event.
    /// </remarks>
    Task ReloadAsync();
}
