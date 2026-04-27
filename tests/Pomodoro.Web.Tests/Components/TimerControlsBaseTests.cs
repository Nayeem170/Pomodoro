using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TimerControlsBaseTests
{
    private class TestableTimerControlsBase : TimerControlsBase
    {
        public new bool IsStartDisabled => base.IsStartDisabled;
    }

    [Fact]
    public void GetSessionLabel_Pomodoro_ReturnsPomodoro()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.Pomodoro };
        Assert.Equal("Pomodoro", sut.GetSessionLabel());
    }

    [Fact]
    public void GetSessionLabel_ShortBreak_ReturnsShortBreak()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.ShortBreak };
        Assert.Equal("Short Break", sut.GetSessionLabel());
    }

    [Fact]
    public void GetSessionLabel_LongBreak_ReturnsLongBreak()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.LongBreak };
        Assert.Equal("Long Break", sut.GetSessionLabel());
    }

    [Fact]
    public void GetSessionLabel_Default_ReturnsEmpty()
    {
        var sut = new TestableTimerControlsBase { SessionType = (SessionType)99 };
        Assert.Equal(string.Empty, sut.GetSessionLabel());
    }

    [Fact]
    public void GetSessionClass_Pomodoro_ReturnsPomodoroClass()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.Pomodoro };
        Assert.Equal("pomodoro", sut.GetSessionClass());
    }

    [Fact]
    public void GetSessionClass_ShortBreak_ReturnsShortBreakClass()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.ShortBreak };
        Assert.Equal("short-break", sut.GetSessionClass());
    }

    [Fact]
    public void GetSessionClass_LongBreak_ReturnsLongBreakClass()
    {
        var sut = new TestableTimerControlsBase { SessionType = SessionType.LongBreak };
        Assert.Equal("long-break", sut.GetSessionClass());
    }

    [Fact]
    public void GetSessionClass_Default_ReturnsPomodoroClass()
    {
        var sut = new TestableTimerControlsBase { SessionType = (SessionType)99 };
        Assert.Equal("pomodoro", sut.GetSessionClass());
    }

    [Fact]
    public void IsStartDisabled_WhenCanStartFalse_IsTrue()
    {
        var sut = new TestableTimerControlsBase { CanStart = false };
        Assert.True(sut.IsStartDisabled);
    }

    [Fact]
    public void IsStartDisabled_WhenCanStartTrue_IsFalse()
    {
        var sut = new TestableTimerControlsBase { CanStart = true };
        Assert.False(sut.IsStartDisabled);
    }
}
