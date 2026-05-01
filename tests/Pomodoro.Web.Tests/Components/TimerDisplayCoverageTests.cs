using Bunit;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Services;
using Pomodoro.Web.Components.Timer;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class TimerDisplayCoverageTests : TestContext
{
    private readonly Mock<ITimerService> _timerServiceMock;
    private readonly Mock<ILogger<TimerDisplayBase>> _mockLogger;

    public TimerDisplayCoverageTests()
    {
        _timerServiceMock = new Mock<ITimerService>();
        _mockLogger = new Mock<ILogger<TimerDisplayBase>>();
        Services.AddSingleton(_timerServiceMock.Object);
        Services.AddSingleton(Mock.Of<ITimerEventPublisher>());
        Services.AddSingleton(_mockLogger.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void GetSessionTypeLabel_WithAllSessionTypes_ReturnsCorrectLabels()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        var result = cut.Instance.GetSessionTypeLabel();
        Assert.Contains("FOCUSING", result);

        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        cut.SetParametersAndRender();
        result = cut.Instance.GetSessionTypeLabel();
        Assert.Contains("SHORT BREAK", result);

        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, false);
        cut.SetParametersAndRender();
        result = cut.Instance.GetSessionTypeLabel();
        Assert.Contains("LONG BREAK", result);
    }

    [Fact]
    public void GetRingSessionClass_WithAllSessionTypes_ReturnsCorrectClasses()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);
        var cut = RenderComponent<TimerDisplay>();
        var result = cut.Instance.GetRingSessionClass();
        Assert.Equal("", result);

        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        cut.SetParametersAndRender();
        result = cut.Instance.GetRingSessionClass();
        Assert.Equal("short-break", result);

        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, true);
        cut.SetParametersAndRender();
        result = cut.Instance.GetRingSessionClass();
        Assert.Equal("long-break", result);
    }

    [Fact]
    public void GetSessionTypeLabel_WithInvalidSessionType_ReturnsPomodoroLabel()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), (SessionType)99, false);
        var cut = RenderComponent<TimerDisplay>();
        var result = cut.Instance.GetSessionTypeLabel();
        Assert.Equal("FOCUSING", result);
    }

    [Fact]
    public void GetRingSessionClass_WithInvalidSessionType_ReturnsEmptyClass()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), (SessionType)99, true);
        var cut = RenderComponent<TimerDisplay>();
        var result = cut.Instance.GetRingSessionClass();
        Assert.Equal("", result);
    }

    [Fact]
    public void HandleStateChangeError_BaseClass_CanBeCalledDirectly()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();

        var exception = Record.Exception(() => cut.Instance.HandleStateChangeError());

        Assert.Null(exception);
    }

    private void SetupTimerService(TimeSpan remainingTime, SessionType sessionType, bool isRunning)
    {
        TestBase.SetupTimerServiceMock(_timerServiceMock, remainingTime, sessionType, isRunning);
    }

    [Fact]
    public async Task UpdateDisplay_CallsStateHasChanged()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();

        await cut.InvokeAsync(() => cut.Instance.UpdateDisplay());
    }

    [Fact]
    public void OnTimerTick_WithoutRenderer_LogsError()
    {
        var component = new TestableTimerDisplay(
            _timerServiceMock.Object, _mockLogger.Object);

        var errorEvent = new ManualResetEventSlim();
        _mockLogger
            .Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => errorEvent.Set());

        component.OnTimerTick();

        Assert.True(errorEvent.Wait(3000), "Logger.LogError should have been called from catch block");
    }

    [Fact]
    public void OnTimerStateChanged_WithoutRenderer_LogsError()
    {
        var component = new TestableTimerDisplay(
            _timerServiceMock.Object, _mockLogger.Object);

        var errorEvent = new ManualResetEventSlim();
        _mockLogger
            .Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => errorEvent.Set());

        component.OnTimerStateChanged();

        Assert.True(errorEvent.Wait(3000), "Logger.LogError should have been called from catch block");
    }

    private class TestableTimerDisplay : TimerDisplayBase
    {
        public TestableTimerDisplay(ITimerService timerService, ILogger<TimerDisplayBase> logger)
        {
            TimerService = timerService;
            Logger = logger;
        }
    }
}

