using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for ActivityItem component
/// Displays a single activity record
/// </summary>
public class ActivityItemBase : ComponentBase
{
    [Inject]
    private ActivityItemFormatter Formatter { get; set; } = null!;

    #region Parameters

    [Parameter]
    public ActivityRecord Activity { get; set; } = default!;

    #endregion

    #region Methods for Testing

    /// <summary>
    /// Checks if the activity is valid
    /// </summary>
    public bool IsValidActivity()
    {
        return Formatter.IsValidActivity(Activity);
    }

    /// <summary>
    /// Gets the formatted time for display
    /// </summary>
    public string GetFormattedTime()
    {
        return Formatter.GetFormattedTime(Activity);
    }

    /// <summary>
    /// Gets the formatted duration for display
    /// </summary>
    public string GetFormattedDuration()
    {
        return Formatter.GetFormattedDuration(Activity);
    }

    /// <summary>
    /// Gets the session type display name
    /// </summary>
    public string GetSessionTypeDisplay()
    {
        return Formatter.GetSessionTypeDisplay(Activity);
    }

    /// <summary>
    /// Checks if the activity has an associated task
    /// </summary>
    public bool HasTask()
    {
        return Formatter.HasTask(Activity);
    }

    /// <summary>
    /// Gets the task name or default value
    /// </summary>
    public string GetTaskName()
    {
        return Formatter.GetTaskName(Activity);
    }

    #endregion
}
