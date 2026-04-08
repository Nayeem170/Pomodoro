using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for ActivityTimeline component
/// Displays a list of activity records
/// </summary>
public class ActivityTimelineBase : ComponentBase
{
    [Inject]
    private ActivityTimelineFormatter Formatter { get; set; } = null!;

    #region Parameters

    [Parameter]
    public List<ActivityRecord> Activities { get; set; } = new();

    #endregion

    #region Methods for Testing

    /// <summary>
    /// Gets count of activities
    /// </summary>
    public int GetActivityCount()
    {
        return Formatter.GetActivityCount(Activities);
    }

    /// <summary>
    /// Checks if there are any activities
    /// </summary>
    public bool HasActivities()
    {
        return Formatter.HasActivities(Activities);
    }

    /// <summary>
    /// Gets total duration of all activities in minutes
    /// </summary>
    public int GetTotalDuration()
    {
        return Formatter.GetTotalDuration(Activities);
    }

    /// <summary>
    /// Gets count of pomodoro sessions
    /// </summary>
    public int GetPomodoroCount()
    {
        return Formatter.GetPomodoroCount(Activities);
    }

    /// <summary>
    /// Gets count of completed sessions
    /// </summary>
    public int GetCompletedCount()
    {
        return Formatter.GetCompletedCount(Activities);
    }

    /// <summary>
    /// Checks if timeline is empty
    /// </summary>
    public bool IsEmpty()
    {
        return Formatter.IsEmpty(Activities);
    }

    #endregion
}
