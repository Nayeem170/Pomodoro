using System.Text.Json.Serialization;

namespace Pomodoro.Web.Models;

public class DailyStatsSummary
{
    public int PomodoroCount { get; set; }
    public int FocusMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int TasksWorkedOn { get; set; }


}
