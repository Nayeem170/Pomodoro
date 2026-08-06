namespace Pomodoro.Web.Models;

public sealed record NewTaskRequest(
    string Name,
    RepeatType RepeatType = RepeatType.None,
    DateTime? ScheduledDate = null,
    DayOfWeek[]? Weekdays = null,
    int CustomDays = 0,
    int MonthlyDay = 0,
    bool IsPaused = false,
    DateTime? PausedDate = null,
    string? ListId = null);
