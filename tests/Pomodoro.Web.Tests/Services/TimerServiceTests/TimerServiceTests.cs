using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
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
using Xunit;
[Trait("Category", "Service")]
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

    protected TimerService CreateService()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dailyStatsService = new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object);
        var jsTimerInterop = new JsTimerInterop(MockJsRuntime.Object, new Mock<ILogger<JsTimerInterop>>().Object);
        return new TimerService(
            MockIndexedDb.Object,
            MockSettingsRepository.Object,
            dailyStatsService,
            jsTimerInterop,
            AppState,
            MockLogger.Object,
            serviceProvider
        );
    }

    /// <summary>
    /// Sets up the current session in AppState with specific state values.
    /// </summary>
    protected void SetupCurrentSession(bool isRunning, bool wasStarted, int remainingSeconds = 300, SessionType sessionType = SessionType.Pomodoro)
    {
        TestBase.SetupCurrentSession(AppState, isRunning, wasStarted, remainingSeconds, sessionType);
    }

    /// <summary>
    /// Clears the current session from AppState.
    /// </summary>
    protected void ClearCurrentSession()
    {
        AppState.CurrentSession = null;
    }
}

