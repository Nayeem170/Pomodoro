using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

public class ActivityItemBase : ComponentBase
{
    [Inject]
    private ActivityItemFormatter Formatter { get; set; } = null!;

    #region Parameters

    [Parameter]
    public ActivityRecord Activity { get; set; } = default!;

    #endregion

    #region Methods for Testing

    public bool IsValidActivity()
    {
        return Formatter.IsValidActivity(Activity);
    }

    public string GetFormattedTime()
    {
        return Formatter.GetFormattedTime(Activity);
    }

    public string GetFormattedDuration()
    {
        return Formatter.GetFormattedDuration(Activity);
    }

    public string GetSessionTypeDisplay()
    {
        return Formatter.GetSessionTypeDisplay(Activity);
    }

    public bool HasTask()
    {
        return Formatter.HasTask(Activity);
    }

    public string GetTaskName()
    {
        return Formatter.GetTaskName(Activity);
    }

    #endregion
}
