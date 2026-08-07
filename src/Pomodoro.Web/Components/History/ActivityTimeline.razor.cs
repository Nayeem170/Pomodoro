using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

public class ActivityTimelineBase : ComponentBase
{
    [Inject]
    private ActivityTimelineFormatter Formatter { get; set; } = null!;

    #region Parameters

    [Parameter]
    public List<ActivityRecord> Activities { get; set; } = new();

    #endregion

    #region Methods for Testing

    public int GetActivityCount()
    {
        return Formatter.GetActivityCount(Activities);
    }

    public bool HasActivities()
    {
        return Formatter.HasActivities(Activities);
    }

    public int GetTotalDuration()
    {
        return Formatter.GetTotalDuration(Activities);
    }

    public int GetPomodoroCount()
    {
        return Formatter.GetPomodoroCount(Activities);
    }

    public int GetCompletedCount()
    {
        return Formatter.GetCompletedCount(Activities);
    }

    public bool IsEmpty()
    {
        return Formatter.IsEmpty(Activities);
    }

    #endregion
}
