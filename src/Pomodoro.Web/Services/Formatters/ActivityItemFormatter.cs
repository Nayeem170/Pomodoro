using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for formatting ActivityItem component data.
/// Extracts formatting logic from component to enable testable code with coverage tracking.
/// </summary>
public class ActivityItemFormatter
{
    /// <summary>
    /// Checks if the activity is valid.
    /// </summary>
    /// <param name="activity">The activity to check</param>
    /// <returns>True if activity is not null and has a valid ID</returns>
    public bool IsValidActivity(ActivityRecord? activity)
    {
        return activity != null && activity.Id != Guid.Empty;
    }

    /// <summary>
    /// Gets the formatted time for display.
    /// </summary>
    /// <param name="activity">The activity to format</param>
    /// <returns>Formatted time string (HH:mm) or "N/A" if null</returns>
    public string GetFormattedTime(ActivityRecord? activity)
    {
        if (activity == null) return "N/A";
        return activity.CompletedAt.ToString("HH:mm");
    }

    /// <summary>
    /// Gets the formatted duration for display.
    /// </summary>
    /// <param name="activity">The activity to format</param>
    /// <returns>Formatted duration string (e.g., "25m") or "0m" if null</returns>
    public string GetFormattedDuration(ActivityRecord? activity)
    {
        if (activity == null) return "0m";
        return $"{activity.DurationMinutes}m";
    }

    /// <summary>
    /// Gets the session type display name.
    /// </summary>
    /// <param name="activity">The activity to format</param>
    /// <returns>Session type name or "Unknown" if null</returns>
    public string GetSessionTypeDisplay(ActivityRecord? activity)
    {
        if (activity == null) return "Unknown";
        return activity.Type.ToString();
    }

    /// <summary>
    /// Checks if the activity has an associated task.
    /// </summary>
    /// <param name="activity">The activity to check</param>
    /// <returns>True if activity has a task name</returns>
    public bool HasTask(ActivityRecord? activity)
    {
        return activity != null && !string.IsNullOrWhiteSpace(activity.TaskName);
    }

    /// <summary>
    /// Gets the task name for display.
    /// </summary>
    /// <param name="activity">The activity to format</param>
    /// <returns>Task name or "No task" if null or empty</returns>
    public string GetTaskName(ActivityRecord? activity)
    {
        if (activity == null) return "No task";
        return string.IsNullOrWhiteSpace(activity.TaskName) ? "No task" : activity.TaskName;
    }
}
