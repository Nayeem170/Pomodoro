namespace Pomodoro.Web.Models;

public sealed class ScheduleDay
{
    public DateTime Date { get; init; }
    public string DayLabel { get; init; } = string.Empty;
    public IReadOnlyList<ScheduleItem> Items { get; init; } = [];
}

public sealed class ScheduleItem
{
    public Guid? TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsRepeat { get; init; }
    public string? RepeatLabel { get; init; }
    public bool IsGoogle { get; init; }
    public bool IsCompleted { get; init; }
    public TaskItem? Task { get; init; }
}
