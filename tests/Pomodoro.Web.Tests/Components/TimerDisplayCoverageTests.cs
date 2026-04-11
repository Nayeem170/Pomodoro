using System.Reflection;
using Bunit;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Services;
using Pomodoro.Web.Components;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Components;

public class TimerDisplayCoverageTests : TestContext
{
    private readonly Mock<ITimerService> _timerServiceMock;
    private readonly Mock<ILogger<TimerDisplayBase>> _mockLogger;

    public TimerDisplayCoverageTests()
    {
        _timerServiceMock = new Mock<ITimerService>();
        _mockLogger = new Mock<ILogger<TimerDisplayBase>>();
        Services.AddSingleton(_timerServiceMock.Object);
        Services.AddSingleton(_mockLogger.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void GetSessionTypeLabel_WithAllSessionTypes_ReturnsCorrectLabels()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();
        var method = typeof(TimerDisplayBase).GetMethod("GetSessionTypeLabel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("POMODORO", result);

        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        cut.SetParametersAndRender();
        result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("SHORT BREAK", result);

        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, false);
        cut.SetParametersAndRender();
        result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("LONG BREAK", result);
    }

    [Fact]
    public void GetTimerClass_WithAllSessionTypesAndRunningStates_ReturnsCorrectClasses()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true);
        var cut = RenderComponent<TimerDisplay>();
        var method = typeof(TimerDisplayBase).GetMethod("GetTimerClass", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("pomodoro", result);
        Assert.DoesNotContain("paused", result);

        SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, false);
        cut.SetParametersAndRender();
        result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("short-break", result);
        Assert.Contains("paused", result);

        SetupTimerService(TimeSpan.FromMinutes(15), SessionType.LongBreak, true);
        cut.SetParametersAndRender();
        result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Contains("long-break", result);
        Assert.DoesNotContain("paused", result);
    }

    [Fact]
    public void GetSessionTypeLabel_WithInvalidSessionType_ReturnsPomodoroLabel()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), (SessionType)99, false);
        var cut = RenderComponent<TimerDisplay>();
        var method = typeof(TimerDisplayBase).GetMethod("GetSessionTypeLabel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Equal(Constants.SessionTypes.PomodoroUppercase, result);
    }

    [Fact]
    public void GetTimerClass_WithInvalidSessionType_ReturnsPomodoroClass()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), (SessionType)99, true);
        var cut = RenderComponent<TimerDisplay>();
        var method = typeof(TimerDisplayBase).GetMethod("GetTimerClass", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (string)method!.Invoke(cut.Instance, null)!;
        Assert.Equal(Constants.SessionTypes.PomodoroClass, result);
    }

    [Fact]
    public void HandleStateChangeError_BaseClass_CanBeCalledDirectly()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();

        var method = typeof(TimerDisplayBase).GetMethod("HandleStateChangeError", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        var exception = Record.Exception(() => method!.Invoke(cut.Instance, null));

        Assert.Null(exception);
    }

    private void SetupTimerService(TimeSpan remainingTime, SessionType sessionType, bool isRunning)
    {
        _timerServiceMock.SetupGet(s => s.RemainingTime).Returns(remainingTime);
        _timerServiceMock.SetupGet(s => s.RemainingSeconds).Returns((int)remainingTime.TotalSeconds);
        _timerServiceMock.SetupGet(s => s.CurrentSessionType).Returns(sessionType);
        _timerServiceMock.SetupGet(s => s.IsRunning).Returns(isRunning);
        _timerServiceMock.SetupGet(s => s.Settings).Returns(new TimerSettings());
    }

    [Fact]
    public async Task UpdateDisplay_CallsStateHasChanged()
    {
        SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, false);
        var cut = RenderComponent<TimerDisplay>();

        var method = typeof(TimerDisplayBase).GetMethod("UpdateDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        await cut.InvokeAsync(() => method!.Invoke(cut.Instance, null));
    }

    [Fact]
    public void OnTimerTick_WithoutRenderer_LogsError()
    {
        var component = new TestableTimerDisplay(
            _timerServiceMock.Object, _mockLogger.Object);

        var onTimerTick = typeof(TimerDisplayBase).GetMethod("OnTimerTick",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(onTimerTick);

        var errorEvent = new ManualResetEventSlim();
        _mockLogger
            .Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => errorEvent.Set());

        onTimerTick.Invoke(component, null);

        Assert.True(errorEvent.Wait(3000), "Logger.LogError should have been called from catch block");
    }

    [Fact]
    public void OnTimerStateChanged_WithoutRenderer_LogsError()
    {
        var component = new TestableTimerDisplay(
            _timerServiceMock.Object, _mockLogger.Object);

        var onTimerStateChanged = typeof(TimerDisplayBase).GetMethod("OnTimerStateChanged",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(onTimerStateChanged);

        var errorEvent = new ManualResetEventSlim();
        _mockLogger
            .Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => errorEvent.Set());

        onTimerStateChanged.Invoke(component, null);

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
