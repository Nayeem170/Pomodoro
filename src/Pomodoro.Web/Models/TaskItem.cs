using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pomodoro.Web.Models;

public class RepeatRule
{
    public RepeatType Type { get; set; } = RepeatType.None;
    public int CustomDays { get; set; }
    public DayOfWeek[] Weekdays { get; set; } = [];
    public int? MonthlyDay { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? LastCompletedDate { get; set; }

    [JsonIgnore]
    public DateTime? NextOccurrence { get; set; }

    public bool IsActive => Type != RepeatType.None && !IsPaused;
}

public enum RepeatType
{
    None,
    Daily,
    Weekly,
    Custom,
    Monthly
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = Constants.Validation.TaskNameRequiredMessage)]
    [StringLength(Constants.Validation.MaxTaskNameLength, ErrorMessage = Constants.Validation.TaskNameMaxLengthMessage)]
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
    public int TotalFocusMinutes { get; set; }
    public int PomodoroCount { get; set; }
    public DateTime? LastWorkedOn { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public RepeatRule? Repeat { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public bool IsScheduled => ScheduledDate.HasValue;

    public bool IsRecurring => Repeat is { Type: not RepeatType.None };

    public bool IsVisible => !IsDeleted && (!IsScheduled || ScheduledDate <= DateTime.UtcNow.Date);

    public bool IsGoogleTask => !string.IsNullOrEmpty(GoogleTaskId);

    public string? GoogleTaskId { get; set; }
    public string? GoogleListId { get; set; }
    public string? ETag { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }

    public TaskItem WithUpdates(Action<TaskItem>? mutate = null)
    {
        var copy = new TaskItem
        {
            Id = Id,
            Name = Name,
            CreatedAt = CreatedAt,
            IsCompleted = IsCompleted,
            TotalFocusMinutes = TotalFocusMinutes,
            PomodoroCount = PomodoroCount,
            LastWorkedOn = LastWorkedOn,
            IsDeleted = IsDeleted,
            DeletedAt = DeletedAt,
            Repeat = Repeat,
            ScheduledDate = ScheduledDate,
            GoogleTaskId = GoogleTaskId,
            GoogleListId = GoogleListId,
            ETag = ETag,
            UpdatedAt = UpdatedAt,
            Notes = Notes,
            DueDate = DueDate,
            Priority = Priority
        };
        mutate?.Invoke(copy);
        return copy;
    }
}
