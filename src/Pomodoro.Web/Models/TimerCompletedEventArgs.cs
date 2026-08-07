namespace Pomodoro.Web.Models;

public record TimerCompletedEventArgs(
    SessionType SessionType,
    Guid? TaskId,
    string? TaskName,
    int DurationMinutes,
    bool WasCompleted,
    DateTime CompletedAt
);
