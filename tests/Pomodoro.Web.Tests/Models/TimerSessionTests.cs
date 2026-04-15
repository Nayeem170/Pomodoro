using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class TimerSessionTests
{
    [Fact]
    public void Constructor_DefaultValues_AllPropertiesInitialized()
    {
        var session = new TimerSession();

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Null(session.TaskId);
        Assert.Equal(SessionType.Pomodoro, session.Type);
        Assert.Equal(default, session.StartedAt);
        Assert.Equal(0, session.DurationSeconds);
        Assert.Equal(0, session.RemainingSeconds);
        Assert.False(session.IsRunning);
        Assert.False(session.IsCompleted);
        Assert.False(session.WasStarted);
    }

    [Fact]
    public void Id_GeneratesUniqueValues()
    {
        var session1 = new TimerSession();
        var session2 = new TimerSession();

        Assert.NotEqual(session1.Id, session2.Id);
    }

    [Fact]
    public void Properties_CanBeSetAndRead()
    {
        var session = new TimerSession
        {
            TaskId = Guid.NewGuid(),
            Type = SessionType.ShortBreak,
            StartedAt = new DateTime(2024, 1, 1, 10, 0, 0),
            DurationSeconds = 300,
            RemainingSeconds = 150,
            IsRunning = true,
            IsCompleted = false,
            WasStarted = true
        };

        Assert.NotNull(session.TaskId);
        Assert.Equal(SessionType.ShortBreak, session.Type);
        Assert.Equal(new DateTime(2024, 1, 1, 10, 0, 0), session.StartedAt);
        Assert.Equal(300, session.DurationSeconds);
        Assert.Equal(150, session.RemainingSeconds);
        Assert.True(session.IsRunning);
        Assert.False(session.IsCompleted);
        Assert.True(session.WasStarted);
    }

    [Fact]
    public void RemainingSeconds_CanReachZero()
    {
        var session = new TimerSession
        {
            DurationSeconds = 25,
            RemainingSeconds = 0
        };

        Assert.Equal(0, session.RemainingSeconds);
    }

    [Fact]
    public void TaskId_CanBeNull()
    {
        var session = new TimerSession { TaskId = null };
        Assert.Null(session.TaskId);
    }

    [Fact]
    public void TaskId_CanBeSet()
    {
        var taskId = Guid.NewGuid();
        var session = new TimerSession { TaskId = taskId };
        Assert.Equal(taskId, session.TaskId);
    }

    [Fact]
    public void AllSessionTypes_CanBeAssigned()
    {
        var pomodoro = new TimerSession { Type = SessionType.Pomodoro };
        var shortBreak = new TimerSession { Type = SessionType.ShortBreak };
        var longBreak = new TimerSession { Type = SessionType.LongBreak };

        Assert.Equal(SessionType.Pomodoro, pomodoro.Type);
        Assert.Equal(SessionType.ShortBreak, shortBreak.Type);
        Assert.Equal(SessionType.LongBreak, longBreak.Type);
    }

    [Fact]
    public void IsCompleted_CanBeSetAfterRunning()
    {
        var session = new TimerSession
        {
            IsRunning = true,
            WasStarted = true
        };

        session.IsRunning = false;
        session.IsCompleted = true;

        Assert.False(session.IsRunning);
        Assert.True(session.IsCompleted);
        Assert.True(session.WasStarted);
    }
}

