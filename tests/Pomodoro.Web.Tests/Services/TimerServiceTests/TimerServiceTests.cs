using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Base test class for TimerService.
/// Contains shared setup and helper methods.
/// </summary>
public partial class TimerServiceTests
{
    protected readonly Mock<IIndexedDbService> MockIndexedDb;
    protected readonly Mock<ISettingsRepository> MockSettingsRepository;
    protected readonly Mock<IJSRuntime> MockJsRuntime;
    protected readonly Mock<ILogger<TimerService>> MockLogger;
    protected readonly AppState AppState;

    public TimerServiceTests()
    {
        MockIndexedDb = new Mock<IIndexedDbService>();
        MockSettingsRepository = new Mock<ISettingsRepository>();
        MockJsRuntime = new Mock<IJSRuntime>();
        MockLogger = new Mock<ILogger<TimerService>>();
        AppState = new AppState();
    }

    /// <summary>
    /// Creates a TimerService instance with mocked dependencies.
    /// </summary>
    protected TimerService CreateService()
    {
        return new TimerService(
            MockIndexedDb.Object,
            MockSettingsRepository.Object,
            AppState,
            MockJsRuntime.Object,
            MockLogger.Object
        );
    }

    /// <summary>
    /// Sets up the current session in AppState with specific state values.
    /// </summary>
    protected void SetupCurrentSession(bool isRunning, bool wasStarted, int remainingSeconds = 300, SessionType sessionType = SessionType.Pomodoro)
    {
        AppState.CurrentSession = new TimerSession
        {
            Type = sessionType,
            DurationSeconds = 1500,
            RemainingSeconds = remainingSeconds,
            IsRunning = isRunning,
            WasStarted = wasStarted
        };
    }

    /// <summary>
    /// Clears the current session from AppState.
    /// </summary>
    protected void ClearCurrentSession()
    {
        AppState.CurrentSession = null;
    }
}
