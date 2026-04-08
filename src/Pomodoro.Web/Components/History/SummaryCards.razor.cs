using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for SummaryCards component
/// Displays a row of summary stat cards
/// </summary>
public class SummaryCardsBase : ComponentBase
{
    #region Parameters

    [Parameter]
    public int PomodoroCount { get; set; }

    [Parameter]
    public int FocusMinutes { get; set; }

    [Parameter]
    public int TasksWorkedOn { get; set; }

    #endregion

    #region Services

    [Inject]
    protected SummaryCardsFormatter Formatter { get; set; } = default!;

    #endregion

    #region Helper Methods

    protected string FormatTime(int minutes) => Formatter.FormatTime(minutes);

    protected string FormatCount(int count) => Formatter.FormatCount(count);

    #endregion
}
