using FluentAssertions;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class TaskItemTests
{
    [Fact]
    public void GoogleTasksSettings_DefaultProperties()
    {
        var settings = new GoogleTasksSettings([]);
        settings.Lists.Should().BeEmpty();
    }

    [Fact]
    public void ListSetting_DefaultProperties()
    {
        var setting = new ListSetting(true, "#4285F4", null);
        setting.IsVisible.Should().BeTrue();
        setting.Color.Should().Be("#4285F4");
        setting.LastSync.Should().BeNull();
    }
    private static TaskItem CreateFullyPopulatedTask()
    {
        return new TaskItem
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Task",
            CreatedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            IsCompleted = true,
            TotalFocusMinutes = 50,
            PomodoroCount = 2,
            LastWorkedOn = new DateTime(2025, 6, 20, 14, 30, 0, DateTimeKind.Utc),
            IsDeleted = false,
            DeletedAt = null,
            Repeat = new RepeatRule { Type = RepeatType.Daily, CustomDays = 1 },
            ScheduledDate = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            GoogleTaskId = "gtask-123",
            GoogleListId = "glist-456",
            ETag = "etag-abc",
            UpdatedAt = new DateTime(2025, 6, 19, 8, 0, 0, DateTimeKind.Utc),
            Notes = "Some notes",
            DueDate = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            Priority = Priority.High
        };
    }

    [Fact]
    public void WithUpdates_CopiesAllFieldsIncludingGoogleFields()
    {
        var original = CreateFullyPopulatedTask();

        var result = original.WithUpdates(t => t.Name = "Mutated Name");

        result.Should().NotBeSameAs(original);
        result.Id.Should().Be(original.Id);
        result.Name.Should().Be("Mutated Name");
        result.CreatedAt.Should().Be(original.CreatedAt);
        result.IsCompleted.Should().Be(original.IsCompleted);
        result.TotalFocusMinutes.Should().Be(original.TotalFocusMinutes);
        result.PomodoroCount.Should().Be(original.PomodoroCount);
        result.LastWorkedOn.Should().Be(original.LastWorkedOn);
        result.IsDeleted.Should().Be(original.IsDeleted);
        result.DeletedAt.Should().Be(original.DeletedAt);
        result.Repeat.Should().BeSameAs(original.Repeat);
        result.ScheduledDate.Should().Be(original.ScheduledDate);
        result.GoogleTaskId.Should().Be(original.GoogleTaskId);
        result.GoogleListId.Should().Be(original.GoogleListId);
        result.ETag.Should().Be(original.ETag);
        result.UpdatedAt.Should().Be(original.UpdatedAt);
        result.Notes.Should().Be(original.Notes);
        result.DueDate.Should().Be(original.DueDate);
        result.Priority.Should().Be(original.Priority);
    }

    [Fact]
    public void WithUpdates_WithNoMutation_ReturnsExactCopy()
    {
        var original = CreateFullyPopulatedTask();

        var result = original.WithUpdates();

        result.Should().NotBeSameAs(original);
        result.Id.Should().Be(original.Id);
        result.Name.Should().Be(original.Name);
        result.CreatedAt.Should().Be(original.CreatedAt);
        result.IsCompleted.Should().Be(original.IsCompleted);
        result.TotalFocusMinutes.Should().Be(original.TotalFocusMinutes);
        result.PomodoroCount.Should().Be(original.PomodoroCount);
        result.LastWorkedOn.Should().Be(original.LastWorkedOn);
        result.IsDeleted.Should().Be(original.IsDeleted);
        result.DeletedAt.Should().Be(original.DeletedAt);
        result.Repeat.Should().BeSameAs(original.Repeat);
        result.ScheduledDate.Should().Be(original.ScheduledDate);
        result.GoogleTaskId.Should().Be(original.GoogleTaskId);
        result.GoogleListId.Should().Be(original.GoogleListId);
        result.ETag.Should().Be(original.ETag);
        result.UpdatedAt.Should().Be(original.UpdatedAt);
        result.Notes.Should().Be(original.Notes);
        result.DueDate.Should().Be(original.DueDate);
        result.Priority.Should().Be(original.Priority);
    }

    [Fact]
    public void IsGoogleTask_WhenGoogleTaskIdSet_ReturnsTrue()
    {
        var task = new TaskItem { GoogleTaskId = "gtask-123" };

        task.IsGoogleTask.Should().BeTrue();
    }

    [Fact]
    public void IsGoogleTask_WhenGoogleTaskIdNull_ReturnsFalse()
    {
        var task = new TaskItem { GoogleTaskId = null };

        task.IsGoogleTask.Should().BeFalse();
    }

    [Fact]
    public void IsGoogleTask_WhenGoogleTaskIdEmpty_ReturnsFalse()
    {
        var task = new TaskItem { GoogleTaskId = string.Empty };

        task.IsGoogleTask.Should().BeFalse();
    }
}
