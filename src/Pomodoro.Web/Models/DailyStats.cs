namespace Pomodoro.Web.Models;

public class DailyStats
{
    public DateTime Date { get; set; }
    public int TotalFocusMinutes { get; set; }
    public int PomodoroCount { get; set; }
    public List<Guid> TaskIdsWorkedOn { get; set; } = new();
}
