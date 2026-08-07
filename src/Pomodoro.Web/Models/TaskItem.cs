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
    public DateTime? PausedDate { get; set; }
    public DateTime? LastCompletedDate { get; set; }

    [JsonIgnore]
    public DateTime? NextOccurrence { get; set; }

    public bool IsActive => Type != RepeatType.None && !IsPaused;

    public bool OccursOn(TaskItem task, DateTime date)
    {
        var anchor = (task.ScheduledDate ?? StartDate ?? task.CreatedAt).Date;
        if (date < anchor) return false;
        if (EndDate.HasValue && date > EndDate.Value.Date) return false;
        if (IsPaused && PausedDate.HasValue
            && PausedDate.Value.Date >= anchor
            && date >= PausedDate.Value.Date) return false;

        return Type switch
        {
            RepeatType.Daily => true,
            RepeatType.Weekly => Weekdays.Length == 0
                ? date.DayOfWeek == anchor.DayOfWeek
                : Weekdays.Contains(date.DayOfWeek),
            RepeatType.Custom => CustomDays > 0 && (date - anchor).Days % CustomDays == 0,
            RepeatType.Monthly => date.Day == (MonthlyDay ?? anchor.Day),
            _ => false
        };
    }
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

    public DateTime? OccurrenceDate { get; set; }

    public bool IsScheduled => ScheduledDate.HasValue;

    public bool IsRecurring => Repeat is { Type: not RepeatType.None };

    public bool IsRepeatInstance => RepeatSeriesId.HasValue;

    public bool IsVisible => !IsDeleted && (!IsScheduled || ScheduledDate <= DateTime.Now.Date);

    public bool OccursOn(DateTime date)
    {
        if (OccurrenceDate?.Date == date) return true;
        if (Repeat is { Type: not RepeatType.None } rule) return rule.OccursOn(this, date);
        return ScheduledDate?.Date == date || DueDate?.Date == date;
    }

    public bool OccursToday
    {
        get
        {
            if (Repeat is not { Type: not RepeatType.None }) return true;
            return OccursOn(DateTime.Now.Date);
        }
    }

    public bool IsGoogleTask => !string.IsNullOrEmpty(GoogleTaskId);

    public bool IsSubtask => ParentTaskId.HasValue || !string.IsNullOrEmpty(GoogleParentTaskId);

    public string? GoogleTaskId { get; set; }
    public string? GoogleListId { get; set; }
    public string? ETag { get; set; }
    public string? GoogleParentTaskId { get; set; }
    public string? GooglePosition { get; set; }
    public Guid? RepeatSeriesId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public bool IsLocalDirty { get; set; }

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
            OccurrenceDate = OccurrenceDate,
            GoogleTaskId = GoogleTaskId,
            GoogleListId = GoogleListId,
            ETag = ETag,
            GoogleParentTaskId = GoogleParentTaskId,
            GooglePosition = GooglePosition,
            RepeatSeriesId = RepeatSeriesId,
            ParentTaskId = ParentTaskId,
            UpdatedAt = UpdatedAt,
            Notes = Notes,
            DueDate = DueDate,
            Priority = Priority,
            IsLocalDirty = IsLocalDirty
        };
        mutate?.Invoke(copy);
        return copy;
    }
}
