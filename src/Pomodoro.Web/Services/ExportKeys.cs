using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Composite key for activity duplicate detection
/// </summary>
public readonly record struct ActivityKey
{
    public SessionType Type { get; }
    public DateTime CompletedAt { get; }
    public int DurationMinutes { get; }
    public string? TaskName { get; }

    public ActivityKey(SessionType type, DateTime completedAt, int durationMinutes, string? taskName)
    {
        Type = type;
        CompletedAt = completedAt;
        DurationMinutes = durationMinutes;
        TaskName = taskName;
    }
}

/// <summary>
/// Composite key for task duplicate detection
/// </summary>
public readonly record struct TaskKey
{
    public string Name { get; }
    public DateTime CreatedAt { get; }

    public TaskKey(string name, DateTime createdAt)
    {
        Name = name;
        CreatedAt = createdAt;
    }
}
