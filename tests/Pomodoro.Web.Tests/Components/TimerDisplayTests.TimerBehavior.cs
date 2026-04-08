using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pomodoro.Web.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

/// <summary>
/// Tests for TimerDisplay timer behavior and event handling
/// </summary>
public partial class TimerDisplayTests
{
    #region Timer Tick Tests

    [Fact]
    public void TimerDisplay_UpdatesDisplay_WhenTimerTickEventFires()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        
        // Act
        _timerServiceMock.Raise(s => s.OnTick += null);
        
        // Assert - Component should re-render when tick event fires
        Assert.NotNull(cut.Find(".timer-display"));
    }

    [Fact]
    public void TimerDisplay_UpdatesTime_WhenRemainingTimeChanges()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        Assert.Contains("25:00", cut.Markup);
        
        // Act - Change remaining time
        SetupTimerService(TimeSpan.FromMinutes(24), SessionType.Pomodoro, false);
        _timerServiceMock.Raise(s => s.OnTick += null);
        
        // Assert
        Assert.Contains("24:00", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_UpdatesSessionLabel_WhenSessionTypeChanges()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        Assert.Contains("POMODORO", cut.Markup);
        
        // Act - Change session type
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        // Assert
        Assert.Contains("SHORT BREAK", cut.Markup);
    }

    #endregion

    #region State Change Tests

    [Fact]
    public void TimerDisplay_UpdatesRunningState_WhenStateChanges()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("paused", timerDisplay.ClassList);
        
        // Act - Change to running state
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        // Assert
        timerDisplay = cut.Find(".timer-display");
        Assert.DoesNotContain("paused", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_UpdatesSessionClass_WhenSessionTypeChanges()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);
        var cut = RenderComponent<TimerDisplay>();
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("pomodoro", timerDisplay.ClassList);
        
        // Act - Change to short break
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, true);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        // Assert
        timerDisplay = cut.Find(".timer-display");
        Assert.Contains("short-break", timerDisplay.ClassList);
        Assert.DoesNotContain("pomodoro", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_HandlesMultipleStateChanges()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        
        // Act - Multiple state changes
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, true);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        // Assert - Final state should be long break running
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("long-break", timerDisplay.ClassList);
        Assert.DoesNotContain("paused", timerDisplay.ClassList);
        Assert.Contains("LONG BREAK", cut.Markup);
    }

    #endregion

    #region Combined Event Tests

    [Fact]
    public void TimerDisplay_HandlesTickAndStateChangeEventsTogether()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        
        // Act - Fire both events
        SetupTimerService(TimeSpan.FromMinutes(24), SessionType.Pomodoro, true);
        _timerServiceMock.Raise(s => s.OnTick += null);
        _timerServiceMock.Raise(s => s.OnStateChanged += null);
        
        // Assert
        Assert.Contains("24:00", cut.Markup);
        var timerDisplay = cut.Find(".timer-display");
        Assert.DoesNotContain("paused", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_SubscribesToBothEvents_OnInitialization()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        
        // Act
        var cut = RenderComponent<TimerDisplay>();
        
        // Assert - Verify both events were subscribed
        _timerServiceMock.VerifyAdd(s => s.OnTick += It.IsAny<Action>(), Times.Once);
        _timerServiceMock.VerifyAdd(s => s.OnStateChanged += It.IsAny<Action>(), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TimerDisplay_HandlesZeroTime()
    {
        // Arrange
        SetupTimerService(TimeSpan.Zero, SessionType.Pomodoro, false);
        
        // Act
        var cut = RenderComponent<TimerDisplay>();
        
        // Assert
        Assert.Contains("00:00", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_HandlesLargeTime()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromHours(1), SessionType.LongBreak, false);
        
        // Act
        var cut = RenderComponent<TimerDisplay>();
        
        // Assert
        Assert.Contains("60:00", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_HandlesSingleSecond()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromSeconds(1), SessionType.Pomodoro, false);
        
        // Act
        var cut = RenderComponent<TimerDisplay>();
        
        // Assert
        Assert.Contains("00:01", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_HandlesFiftyNineSeconds()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromSeconds(59), SessionType.ShortBreak, false);
        
        // Act
        var cut = RenderComponent<TimerDisplay>();
        
        // Assert
        Assert.Contains("00:59", cut.Markup);
    }

    #endregion
}
