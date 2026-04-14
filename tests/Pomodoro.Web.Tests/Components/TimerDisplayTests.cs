using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

/// <summary>
/// Unit tests for TimerDisplay component
/// </summary>
public partial class TimerDisplayTests : TestContext
{
    private readonly Mock<ITimerService> _timerServiceMock;

    public TimerDisplayTests()
    {
        _timerServiceMock = new Mock<ITimerService>();
        Services.AddSingleton(_timerServiceMock.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void TimerDisplay_RendersWithCorrectStructure()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.NotNull(cut.Find(".timer-display"));
        Assert.NotNull(cut.Find(".timer-time"));
        Assert.NotNull(cut.Find(".timer-type"));
    }

    [Fact]
    public void TimerDisplay_ShowsFormattedTime()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("25:00", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_ShowsSingleDigitMinutes()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("05:00", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_ShowsSecondsCorrectly()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromSeconds(90), SessionType.Pomodoro, false); // 1:30

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("01:30", cut.Markup);
    }

    #endregion

    #region Session Type Label Tests

    [Fact]
    public void TimerDisplay_ShowsPomodoroLabel()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("POMODORO", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_ShowsShortBreakLabel()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("SHORT BREAK", cut.Markup);
    }

    [Fact]
    public void TimerDisplay_ShowsLongBreakLabel()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        Assert.Contains("LONG BREAK", cut.Markup);
    }

    #endregion

    #region CSS Class Tests

    [Fact]
    public void TimerDisplay_HasPomodoroClass_WhenSessionIsPomodoro()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("pomodoro", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_HasShortBreakClass_WhenSessionIsShortBreak()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, true);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("short-break", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_HasLongBreakClass_WhenSessionIsLongBreak()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, true);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("long-break", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_HasPausedClass_WhenNotRunning()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("paused", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_DoesNotHavePausedClass_WhenRunning()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.DoesNotContain("paused", timerDisplay.ClassList);
    }

    [Fact]
    public void TimerDisplay_HasBothSessionAndPausedClass_WhenPaused()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        var timerDisplay = cut.Find(".timer-display");
        Assert.Contains("short-break", timerDisplay.ClassList);
        Assert.Contains("paused", timerDisplay.ClassList);
    }

    #endregion

    #region Event Subscription Tests

    [Fact]
    public void TimerDisplay_SubscribesToOnTickEvent()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        _timerServiceMock.VerifyAdd(s => s.OnTick += It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public void TimerDisplay_SubscribesToOnStateChangedEvent()
    {
        // Arrange
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);

        // Act
        var cut = RenderComponent<TimerDisplay>();

        // Assert
        _timerServiceMock.VerifyAdd(s => s.OnStateChanged += It.IsAny<Action>(), Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupTimerService(TimeSpan remainingTime, SessionType sessionType, bool isRunning)
    {
        TestBase.SetupTimerServiceMock(_timerServiceMock, remainingTime, sessionType, isRunning);
    }

    #endregion
}
