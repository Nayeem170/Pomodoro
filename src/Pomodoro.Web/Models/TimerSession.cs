namespace Pomodoro.Web.Models;

/// <summary>
/// Represents an active or completed timer session
/// </summary>
public class TimerSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? TaskId { get; set; }
    public SessionType Type { get; set; }
    public DateTime StartedAt { get; set; }
    public int DurationSeconds { get; set; }
    public int RemainingSeconds { get; set; }
    public bool IsRunning { get; set; }
    public bool IsCompleted { get; set; }
    public bool WasStarted { get; set; }
}
