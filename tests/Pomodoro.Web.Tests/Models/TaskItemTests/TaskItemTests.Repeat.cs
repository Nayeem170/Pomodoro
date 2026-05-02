using FluentAssertions;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class TaskItemRepeatTests
{
    [Fact]
    public void RepeatRule_IsActive_True_WhenNotPausedAndNotNone()
    {
        var rule = new RepeatRule { Type = RepeatType.Daily, IsPaused = false };
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RepeatRule_IsActive_False_WhenPaused()
    {
        var rule = new RepeatRule { Type = RepeatType.Daily, IsPaused = true };
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RepeatRule_IsActive_False_WhenNone()
    {
        var rule = new RepeatRule { Type = RepeatType.None, IsPaused = false };
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RepeatRule_IsActive_False_WhenNoneAndPaused()
    {
        var rule = new RepeatRule { Type = RepeatType.None, IsPaused = true };
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsRecurring_True_WhenRepeatTypeIsNotNone()
    {
        var task = new TaskItem { Repeat = new RepeatRule { Type = RepeatType.Daily } };
        task.IsRecurring.Should().BeTrue();
    }

    [Fact]
    public void TaskItem_IsRecurring_False_WhenRepeatIsNull()
    {
        var task = new TaskItem { Repeat = null };
        task.IsRecurring.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsRecurring_False_WhenRepeatTypeIsNone()
    {
        var task = new TaskItem { Repeat = new RepeatRule { Type = RepeatType.None } };
        task.IsRecurring.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsScheduled_True_WhenScheduledDateSet()
    {
        var task = new TaskItem { ScheduledDate = DateTime.UtcNow.Date };
        task.IsScheduled.Should().BeTrue();
    }

    [Fact]
    public void TaskItem_IsScheduled_False_WhenScheduledDateNull()
    {
        var task = new TaskItem { ScheduledDate = null };
        task.IsScheduled.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsVisible_True_WhenNotDeleted()
    {
        var task = new TaskItem { IsDeleted = false };
        task.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void TaskItem_IsVisible_False_WhenDeleted()
    {
        var task = new TaskItem { IsDeleted = true };
        task.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsVisible_False_WhenScheduledForFuture()
    {
        var task = new TaskItem { ScheduledDate = DateTime.UtcNow.Date.AddDays(1) };
        task.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void TaskItem_IsVisible_True_WhenScheduledForPast()
    {
        var task = new TaskItem { ScheduledDate = DateTime.UtcNow.Date.AddDays(-1) };
        task.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void TaskItem_IsVisible_True_WhenScheduledForToday()
    {
        var task = new TaskItem { ScheduledDate = DateTime.UtcNow.Date };
        task.IsVisible.Should().BeTrue();
    }
}
