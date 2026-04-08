namespace Pomodoro.Web.Models;

/// <summary>
/// Event arguments for timer completion events
/// </summary>
public record TimerCompletedEventArgs(
    SessionType SessionType,
    Guid? TaskId,
    string? TaskName,
    int DurationMinutes,
    bool WasCompleted,
    DateTime CompletedAt
);
