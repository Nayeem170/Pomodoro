namespace Pomodoro.Web.Models;

/// <summary>
/// Represents daily statistics for persistence
/// </summary>
public class DailyStats
{
    public DateTime Date { get; set; }
    public int TotalFocusMinutes { get; set; }
    public int PomodoroCount { get; set; }
    public List<Guid> TaskIdsWorkedOn { get; set; } = new();
}
