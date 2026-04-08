using static Pomodoro.Web.Constants;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for preparing chart data from activity dictionaries
/// </summary>
public class ChartDataFormatter
{
    /// <summary>
    /// Prepares chart data arrays from daily activity dictionaries for a week
    /// </summary>
    /// <param name="dailyFocusMinutes">Dictionary mapping dates to focus minutes</param>
    /// <param name="breakDailyMinutes">Dictionary mapping dates to break minutes</param>
    /// <param name="weekStartDate">Start date of the week</param>
    /// <returns>Tuple containing labels array, focus data array, and break data array</returns>
    public (string[] labels, int[] focusData, int[] breakData) PrepareWeeklyChartData(
        Dictionary<DateTime, int> dailyFocusMinutes,
        Dictionary<DateTime, int> breakDailyMinutes,
        DateTime weekStartDate)
    {
        var weekStart = weekStartDate.Date;
        
        var labels = new string[Constants.Charts.DaysPerWeek];
        var focusData = new int[Constants.Charts.DaysPerWeek];
        var breakData = new int[Constants.Charts.DaysPerWeek];
        
        for (int i = 0; i < Constants.Charts.DaysPerWeek; i++)
        {
            var date = weekStart.AddDays(i);
            labels[i] = date.ToString(Constants.DateFormats.DayOfWeekShortFormat);
            focusData[i] = dailyFocusMinutes.TryGetValue(date, out var minutes) ? minutes : 0;
            breakData[i] = breakDailyMinutes.TryGetValue(date, out var breakMinutes) ? breakMinutes : 0;
        }
        
        return (labels, focusData, breakData);
    }
}
