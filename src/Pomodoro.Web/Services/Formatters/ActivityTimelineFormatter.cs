using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for formatting ActivityTimeline component data.
/// Extracts formatting logic from component to enable testable code with coverage tracking.
/// </summary>
public class ActivityTimelineFormatter
{
    /// <summary>
    /// Gets the count of activities.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>Count of activities or 0 if null</returns>
    public int GetActivityCount(List<ActivityRecord>? activities)
    {
        return activities?.Count ?? 0;
    }

    /// <summary>
    /// Checks if there are any activities.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>True if there are activities</returns>
    public bool HasActivities(List<ActivityRecord>? activities)
    {
        return activities != null && activities.Count > 0;
    }

    /// <summary>
    /// Gets the total duration of all activities.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>Total duration in minutes or 0 if null</returns>
    public int GetTotalDuration(List<ActivityRecord>? activities)
    {
        if (activities == null) return 0;
        return activities.Sum(a => a.DurationMinutes);
    }

    /// <summary>
    /// Gets the count of Pomodoro sessions.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>Count of Pomodoro sessions or 0 if null</returns>
    public int GetPomodoroCount(List<ActivityRecord>? activities)
    {
        if (activities == null) return 0;
        return activities.Count(a => a.Type == SessionType.Pomodoro);
    }

    /// <summary>
    /// Gets the count of completed activities.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>Count of completed activities or 0 if null</returns>
    public int GetCompletedCount(List<ActivityRecord>? activities)
    {
        if (activities == null) return 0;
        return activities.Count(a => a.WasCompleted);
    }

    /// <summary>
    /// Checks if the activity list is empty.
    /// </summary>
    /// <param name="activities">The list of activities</param>
    /// <returns>True if the list is null or empty</returns>
    public bool IsEmpty(List<ActivityRecord>? activities)
    {
        return activities == null || activities.Count == 0;
    }
}
