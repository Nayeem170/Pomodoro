namespace Pomodoro.Web.Models;

/// <summary>
/// Weekly statistics summary for productivity tracking
/// </summary>
public record WeeklyStats
{
    /// <summary>
    /// Total focus minutes accumulated during the week
    /// </summary>
    public int TotalFocusMinutes { get; init; }
    
    /// <summary>
    /// Total number of pomodoro sessions completed during the week
    /// </summary>
    public int TotalPomodoroCount { get; init; }
    
    /// <summary>
    /// Number of unique tasks worked on during the week
    /// </summary>
    public int UniqueTasksWorkedOn { get; init; }
    
    /// <summary>
    /// Average focus minutes per day during the week
    /// </summary>
    public double DailyAverageMinutes { get; init; }
    
    /// <summary>
    /// The day of the week with the most focus time
    /// </summary>
    public DayOfWeek MostProductiveDay { get; init; }
    
    /// <summary>
    /// Total focus minutes from the previous week
    /// </summary>
    public int PreviousWeekFocusMinutes { get; init; }
    
    /// <summary>
    /// Percentage change compared to previous week (positive = increase, negative = decrease)
    /// </summary>
    public double WeekOverWeekChange { get; init; }
}
