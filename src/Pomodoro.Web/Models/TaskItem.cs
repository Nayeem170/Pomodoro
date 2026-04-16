using System.ComponentModel.DataAnnotations;

namespace Pomodoro.Web.Models;

/// <summary>
/// Represents a task that can be worked on during pomodoro sessions
/// </summary>
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

    /// <summary>
    /// Soft delete flag - when true, task is hidden from active lists but preserved for history
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the task was soft-deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
