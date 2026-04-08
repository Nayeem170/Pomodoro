using Microsoft.AspNetCore.Components;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for WeekNavigator component
/// Provides week navigation controls (Saturday to Friday week)
/// </summary>
public class WeekNavigatorBase : ComponentBase
{
    #region Parameters
    
    [Parameter]
    public DateTime SelectedWeekStart { get; set; }
    
    [Parameter]
    public EventCallback<DateTime> OnWeekChanged { get; set; }
    
    #endregion

    #region Properties
    
    /// <summary>
    /// Check if the selected week is the current week
    /// </summary>
    protected bool IsThisWeek
    {
        get
        {
            var thisWeekStart = GetWeekStart(DateTime.Now.Date);
            return SelectedWeekStart.Date == thisWeekStart.Date;
        }
    }
    
    #endregion

    #region Actions
    
    /// <summary>
    /// Navigate to the previous week (Saturday to Friday)
    /// </summary>
    protected async Task GoToPreviousWeek()
    {
        var newWeekStart = SelectedWeekStart.AddDays(-Constants.Charts.DaysPerWeek);
        await OnWeekChanged.InvokeAsync(newWeekStart);
    }
    
    /// <summary>
    /// Navigate to the next week (Saturday to Friday)
    /// </summary>
    protected async Task GoToNextWeek()
    {
        var newWeekStart = SelectedWeekStart.AddDays(Constants.Charts.DaysPerWeek);
        await OnWeekChanged.InvokeAsync(newWeekStart);
    }
    
    /// <summary>
    /// Navigate to the current week
    /// </summary>
    protected async Task GoToThisWeek()
    {
        var thisWeekStart = GetWeekStart(DateTime.Now.Date);
        await OnWeekChanged.InvokeAsync(thisWeekStart);
    }
    
    #endregion

    #region Helper Methods
    
    /// <summary>
    /// Get the Saturday start of the week containing the given date
    /// DayOfWeek: Sun=0, Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6
    /// </summary>
    public static DateTime GetWeekStart(DateTime date)
    {
        int dayOfWeek = (int)date.DayOfWeek;
        int daysSinceSaturday = (dayOfWeek + Constants.TimeConversion.SaturdayBasedWeekOffset) % Constants.TimeConversion.DaysInWeek; // Sat=0, Sun=1, ..., Fri=6
        return date.AddDays(-daysSinceSaturday).Date;
    }
    
    /// <summary>
    /// Format the week range for display (e.g., "Jan 20 - Jan 26, 2026")
    /// </summary>
    protected string FormatWeekRange(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(Constants.TimeConversion.WeeklyLookbackDays);
        return string.Format(Constants.History.WeekRangeFormat, weekStart, weekEnd);
    }
    
    #endregion
}
