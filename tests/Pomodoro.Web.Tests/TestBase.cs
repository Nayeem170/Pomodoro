using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Models;
using Pomodoro.Web.Components.Shared;

namespace Pomodoro.Web.Tests;

/// <summary>
/// Base class for tests that provides common mock setup and utilities.
/// Reduces test setup time by providing pre-configured mocks.
/// </summary>
public abstract class TestBase
{
    protected static Mock<ITimerService> CreateMockTimerService()
    {
        var mock = new Mock<ITimerService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<IIndexedDbService> CreateMockIndexedDbService()
    {
        var mock = new Mock<IIndexedDbService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<IActivityService> CreateMockActivityService()
    {
        var mock = new Mock<IActivityService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<ITaskService> CreateMockTaskService()
    {
        var mock = new Mock<ITaskService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<INotificationService> CreateMockNotificationService()
    {
        var mock = new Mock<INotificationService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<IExportService> CreateMockExportService()
    {
        var mock = new Mock<IExportService>();
        return mock;
    }

    protected static Mock<IConsentService> CreateMockConsentService()
    {
        var mock = new Mock<IConsentService>();
        mock.Setup(s => s.Initialize());
        return mock;
    }

    protected static Mock<IPipTimerService> CreateMockPipTimerService()
    {
        var mock = new Mock<IPipTimerService>();
        return mock;
    }

    protected static Mock<IKeyboardShortcutService> CreateMockKeyboardShortcutService()
    {
        var mock = new Mock<IKeyboardShortcutService>();
        mock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<ILocalDateTimeService> CreateMockLocalDateTimeService()
    {
        var mock = new Mock<ILocalDateTimeService>();
        // Use current date (without time) to ensure tests work correctly
        var currentDate = DateTime.Now.Date;
        mock.Setup(s => s.GetLocalDateAsync()).ReturnsAsync(currentDate);
        mock.Setup(s => s.GetLocalDateTimeAsync()).ReturnsAsync(currentDate);
        mock.Setup(s => s.GetLocalDateTimeOffsetAsync()).ReturnsAsync(currentDate);
        mock.Setup(s => s.GetTimezoneOffsetAsync()).ReturnsAsync(0);
        return mock;
    }

    protected static Mock<IInfiniteScrollInterop> CreateMockInfiniteScrollInterop()
    {
        var mock = new Mock<IInfiniteScrollInterop>();
        // Setup basic mock methods
        mock.Setup(s => s.IsSupportedAsync()).ReturnsAsync(true);
        mock.Setup(s => s.CreateObserverAsync(
            It.IsAny<string>(),
            It.IsAny<DotNetObjectReference<object>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>())).ReturnsAsync(true);
        mock.Setup(s => s.DestroyObserverAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(s => s.DestroyAllObserversAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    protected static Mock<TimerThemeFormatter> CreateMockTimerThemeFormatter()
    {
        return new Mock<TimerThemeFormatter>();
    }

    protected static Mock<IJSRuntime> CreateMockJSRuntime()
    {
        return new Mock<IJSRuntime>();
    }

    protected static Mock<LayoutPresenterService> CreateMockLayoutPresenterService()
    {
        var mock = new Mock<LayoutPresenterService>();
        mock.Setup(x => x.GetNavMenuCssClass()).Returns("collapse");
        mock.Setup(x => x.GetNavigationLinks()).Returns(new List<Pomodoro.Web.Services.NavLinkData>());
        mock.Setup(x => x.ToggleNavMenu());
        return mock;
    }

    protected static Mock<IndexPagePresenterService> CreateMockIndexPagePresenterService()
    {
        var loggerMock = new Mock<ILogger<IndexPagePresenterService>>();
        return new Mock<IndexPagePresenterService>(loggerMock.Object);
    }

    protected static Mock<AppState> CreateMockAppState()
    {
        var mock = new Mock<AppState>();
        // Note: AppState properties are not overridable, so we can't mock them with SetupProperty
        // Tests that need AppState should create their own instances or use the real AppState
        return mock;
    }

    protected static TodayStatsService CreateTodayStatsService(IActivityService activityService)
    {
        return new TodayStatsService(activityService);
    }

    /// <summary>
    /// Creates a collection of common service mocks for Blazor component testing.
    /// This reduces setup time for component tests.
    /// </summary>
    public static ServiceCollection CreateCommonServiceCollection()
    {
        var services = new ServiceCollection();

        // Register common mocks
        services.AddSingleton(CreateMockLayoutPresenterService().Object);
        services.AddSingleton(CreateMockTaskService().Object);
        services.AddSingleton(CreateMockActivityService().Object);
        services.AddSingleton(CreateMockTimerService().Object);
        services.AddSingleton(CreateMockNotificationService().Object);
        services.AddSingleton(CreateMockExportService().Object);
        services.AddSingleton(CreateMockIndexedDbService().Object);
        services.AddSingleton(CreateMockConsentService().Object);
        services.AddSingleton(CreateMockPipTimerService().Object);
        services.AddSingleton(CreateMockKeyboardShortcutService().Object);
        services.AddSingleton(CreateMockIndexPagePresenterService().Object);
        services.AddSingleton(CreateMockTimerThemeFormatter().Object);
        services.AddSingleton(CreateMockJSRuntime().Object);
        services.AddSingleton<IJSInteropService, JSInteropService>();
        services.AddSingleton(CreateMockInfiniteScrollInterop().Object);
        var mockLocalDateTimeService = CreateMockLocalDateTimeService();
        services.AddSingleton<ILocalDateTimeService>(mockLocalDateTimeService.Object);
        // Register the mock as the concrete implementation as well
        services.AddSingleton<LocalDateTimeService>(sp =>
        {
            // Create a mock of the concrete class
            var jsRuntime = sp.GetRequiredService<IJSRuntime>();
            var mock = new Mock<LocalDateTimeService>(jsRuntime) { CallBase = false };

            // Set up the mock to return the same values as the interface mock
            var currentDate = DateTime.Now.Date;
            mock.Setup(s => s.GetLocalDateAsync()).ReturnsAsync(currentDate);
            mock.Setup(s => s.GetLocalDateTimeAsync()).ReturnsAsync(currentDate);
            mock.Setup(s => s.GetTimezoneOffsetAsync()).ReturnsAsync(0);

            return mock.Object;
        });
        // Note: AppState is not included here as it's not overridable
        // Tests that need AppState should create their own instances

        // Register logger mocks
        services.AddSingleton(new Mock<ILogger<App>>().Object);
        services.AddSingleton(new Mock<ILogger<LayoutPresenterService>>().Object);
        services.AddSingleton(new Mock<ILogger<ErrorDisplay>>().Object);

        // Register concrete service
        services.AddSingleton<ITodayStatsService>(sp =>
            CreateTodayStatsService(sp.GetRequiredService<IActivityService>()));

        return services;
    }

    public static void SetupTimerServiceMock(Mock<ITimerService> mock, TimeSpan remainingTime, SessionType sessionType, bool isRunning)
    {
        mock.SetupGet(s => s.RemainingTime).Returns(remainingTime);
        mock.SetupGet(s => s.RemainingSeconds).Returns((int)remainingTime.TotalSeconds);
        mock.SetupGet(s => s.CurrentSessionType).Returns(sessionType);
        mock.SetupGet(s => s.IsRunning).Returns(isRunning);
        mock.SetupGet(s => s.Settings).Returns(new TimerSettings());
    }

    public static void SetupCurrentSession(AppState appState, bool isRunning, bool wasStarted, int remainingSeconds = 300, SessionType sessionType = SessionType.Pomodoro)
    {
        appState.CurrentSession = new TimerSession
        {
            Type = sessionType,
            DurationSeconds = 1500,
            RemainingSeconds = remainingSeconds,
            IsRunning = isRunning,
            WasStarted = wasStarted
        };
    }

    public static void SetupJsInvokeAsync<T>(Mock<IJSRuntime> mockJsRuntime, string methodName, T result)
    {
        mockJsRuntime
            .Setup(js => js.InvokeAsync<T>(methodName, It.IsAny<object?[]?>()))
            .ReturnsAsync(result);
    }

    public static void SetupJsInvokeVoidAsync(Mock<IJSRuntime> mockJsRuntime, string methodName)
    {
        mockJsRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
    }

    public static void SetupJsInvokeAsyncException<T>(Mock<IJSRuntime> mockJsRuntime, string methodName, Exception exception)
    {
        mockJsRuntime
            .Setup(js => js.InvokeAsync<T>(methodName, It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
    }

    public static void SetupJsInvokeVoidAsyncException(Mock<IJSRuntime> mockJsRuntime, string methodName, Exception exception)
    {
        mockJsRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
    }

    public static void VerifyJsInvokeVoidAsync(Mock<IJSRuntime> mockJsRuntime, string methodName, Times times)
    {
        mockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()), times);
    }
}
