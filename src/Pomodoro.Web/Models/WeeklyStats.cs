namespace Pomodoro.Web.Models;

public record WeeklyStats
{
    public int TotalFocusMinutes { get; init; }

    public int TotalPomodoroCount { get; init; }

    public int UniqueTasksWorkedOn { get; init; }

    public double DailyAverageMinutes { get; init; }

    public DayOfWeek MostProductiveDay { get; init; }

    public int PreviousWeekFocusMinutes { get; init; }

    public double WeekOverWeekChange { get; init; }
}
