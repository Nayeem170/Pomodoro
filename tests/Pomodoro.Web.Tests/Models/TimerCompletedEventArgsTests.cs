using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class TimerCompletedEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var taskId = Guid.NewGuid();
        var completedAt = new DateTime(2024, 6, 15, 14, 30, 0);
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            taskId,
            "Test Task",
            25,
            true,
            completedAt
        );

        Assert.Equal(SessionType.Pomodoro, args.SessionType);
        Assert.Equal(taskId, args.TaskId);
        Assert.Equal("Test Task", args.TaskName);
        Assert.Equal(25, args.DurationMinutes);
        Assert.True(args.WasCompleted);
        Assert.Equal(completedAt, args.CompletedAt);
    }

    [Fact]
    public void Constructor_WithNullTaskId_SetsCorrectly()
    {
        var args = new TimerCompletedEventArgs(
            SessionType.ShortBreak,
            null,
            null,
            5,
            true,
            DateTime.Now
        );

        Assert.Null(args.TaskId);
        Assert.Null(args.TaskName);
        Assert.Equal(SessionType.ShortBreak, args.SessionType);
    }

    [Fact]
    public void Constructor_WasCompletedFalse_SetsCorrectly()
    {
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            Guid.NewGuid(),
            "Task",
            10,
            false,
            DateTime.Now
        );

        Assert.False(args.WasCompleted);
        Assert.Equal(10, args.DurationMinutes);
    }

    [Fact]
    public void Record_Equality_SameValuesAreEqual()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var taskId = Guid.NewGuid();

        var args1 = new TimerCompletedEventArgs(SessionType.Pomodoro, taskId, "Task", 25, true, completedAt);
        var args2 = new TimerCompletedEventArgs(SessionType.Pomodoro, taskId, "Task", 25, true, completedAt);

        Assert.Equal(args1, args2);
    }

    [Fact]
    public void Record_Equality_DifferentSessionType_NotEqual()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var taskId = Guid.NewGuid();

        var args1 = new TimerCompletedEventArgs(SessionType.Pomodoro, taskId, "Task", 25, true, completedAt);
        var args2 = new TimerCompletedEventArgs(SessionType.ShortBreak, taskId, "Task", 25, true, completedAt);

        Assert.NotEqual(args1, args2);
    }

    [Fact]
    public void AllSessionTypes_CanBeUsed()
    {
        var completedAt = DateTime.Now;

        var pomodoro = new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, completedAt);
        var shortBreak = new TimerCompletedEventArgs(SessionType.ShortBreak, null, null, 5, true, completedAt);
        var longBreak = new TimerCompletedEventArgs(SessionType.LongBreak, null, null, 15, true, completedAt);

        Assert.Equal(SessionType.Pomodoro, pomodoro.SessionType);
        Assert.Equal(SessionType.ShortBreak, shortBreak.SessionType);
        Assert.Equal(SessionType.LongBreak, longBreak.SessionType);
    }
}

