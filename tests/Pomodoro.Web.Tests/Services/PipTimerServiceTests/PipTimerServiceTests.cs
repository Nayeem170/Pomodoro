using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Base test class for PipTimerService.
/// Contains shared setup and helper methods.
/// </summary>
using Xunit;
[Trait("Category", "Service")]
public partial class PipTimerServiceTests
{
    protected readonly Mock<IJSRuntime> MockJsRuntime;
    protected readonly Mock<ITimerService> MockTimerService;
    protected readonly Mock<ITaskService> MockTaskService;
    protected readonly Mock<ILogger<PipTimerService>> MockLogger;
    protected readonly AppState AppState;

    public PipTimerServiceTests()
    {
        MockJsRuntime = new Mock<IJSRuntime>();
        MockTimerService = new Mock<ITimerService>();
        MockTaskService = new Mock<ITaskService>();
        MockLogger = new Mock<ILogger<PipTimerService>>();
        AppState = new AppState();
    }

    /// <summary>
    /// Creates a PipTimerService instance with mocked dependencies.
    /// </summary>
    protected PipTimerService CreateService()
    {
        return new PipTimerService(
            MockJsRuntime.Object,
            MockTimerService.Object,
            MockTaskService.Object,
            AppState,
            MockLogger.Object
        );
    }

    /// <summary>
    /// Sets up the timer service mock with specific state values.
    /// </summary>
    protected void SetupTimerState(bool isRunning, bool isStarted, int remainingSeconds = 300, SessionType sessionType = SessionType.Pomodoro)
    {
        // IsPaused is true when timer was started but is not running
        var isPaused = isStarted && !isRunning;

        MockTimerService.SetupGet(x => x.IsRunning).Returns(isRunning);
        MockTimerService.SetupGet(x => x.IsStarted).Returns(isStarted);
        MockTimerService.SetupGet(x => x.IsPaused).Returns(isPaused);
        MockTimerService.SetupGet(x => x.RemainingSeconds).Returns(remainingSeconds);
        MockTimerService.SetupGet(x => x.CurrentSessionType).Returns(sessionType);
    }

    /// <summary>
    /// Sets up the current session in AppState.
    /// </summary>
    protected void SetupCurrentSession(bool isRunning, bool wasStarted, int remainingSeconds = 300, SessionType sessionType = SessionType.Pomodoro)
    {
        TestBase.SetupCurrentSession(AppState, isRunning, wasStarted, remainingSeconds, sessionType);
    }
}

