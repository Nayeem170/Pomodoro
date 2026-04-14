using System.Text.Json.Serialization;

namespace Pomodoro.Web.Models;

/// <summary>
/// Summary statistics for a day's activities
/// </summary>
public class DailyStatsSummary
{
    public int PomodoroCount { get; set; }
    public int FocusMinutes { get; set; }
    public int TasksWorkedOn { get; set; }
    

}
