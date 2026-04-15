using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests;

/// <summary>
/// Base test class that provides common service mocks for component testing.
/// All component tests should inherit from this class to have consistent mocking setup.
/// </summary>
public abstract class TestHelper : TestContext
{
    protected Mock<ITimerService> TimerServiceMock { get; private set; }
    protected Mock<ITaskService> TaskServiceMock { get; private set; }
    protected Mock<IActivityService> ActivityServiceMock { get; private set; }
    protected Mock<IStatisticsService> StatisticsServiceMock { get; private set; }
    protected Mock<INotificationService> NotificationServiceMock { get; private set; }
    protected Mock<IExportService> ExportServiceMock { get; private set; }
    protected Mock<IImportService> ImportServiceMock { get; private set; }
    protected Mock<IIndexedDbService> IndexedDbServiceMock { get; private set; }
    protected Mock<IConsentService> ConsentServiceMock { get; private set; }
    protected Mock<IPipTimerService> PipTimerServiceMock { get; private set; }
    protected Mock<IKeyboardShortcutService> KeyboardShortcutServiceMock { get; private set; }
    protected Mock<IJSInteropService> JSInteropServiceMock { get; private set; }
    protected Mock<IJSRuntime> JSRuntimeMock { get; private set; }
    protected Mock<IInfiniteScrollInterop> InfiniteScrollInteropMock { get; private set; }
    protected Mock<ITodayStatsService> TodayStatsServiceMock { get; private set; }
    protected Mock<HistoryStatsService> HistoryStatsServiceMock { get; private set; }
    protected IndexPagePresenterService IndexPagePresenterService { get; private set; }
    protected HistoryPagePresenterService HistoryPagePresenterService { get; private set; }
    protected SettingsPresenterService SettingsPresenterService { get; private set; }
    protected TimerThemeFormatter TimerThemeFormatter { get; private set; }
    protected TimeFormatter TimeFormatter { get; private set; }
    protected ChartDataFormatter ChartDataFormatter { get; private set; }
    protected SummaryCardsFormatter SummaryCardsFormatter { get; private set; }
    protected StatCardFormatter StatCardFormatter { get; private set; }
    protected ActivityItemFormatter ActivityItemFormatter { get; private set; }
    protected ActivityTimelineFormatter ActivityTimelineFormatter { get; private set; }
    protected AppState AppState { get; private set; }
    protected IChartService ChartService { get; private set; }
    
    protected TestHelper()
    {
        // Create mocks for all services
        TimerServiceMock = new Mock<ITimerService>();
        TaskServiceMock = new Mock<ITaskService>();
        ActivityServiceMock = new Mock<IActivityService>();
        StatisticsServiceMock = new Mock<IStatisticsService>();
        NotificationServiceMock = new Mock<INotificationService>();
        ExportServiceMock = new Mock<IExportService>();
        ImportServiceMock = new Mock<IImportService>();
        IndexedDbServiceMock = new Mock<IIndexedDbService>();
        ConsentServiceMock = new Mock<IConsentService>();
        PipTimerServiceMock = new Mock<IPipTimerService>();
        KeyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        JSInteropServiceMock = new Mock<IJSInteropService>();
        JSRuntimeMock = new Mock<IJSRuntime>();
        InfiniteScrollInteropMock = new Mock<IInfiniteScrollInterop>();
        TodayStatsServiceMock = new Mock<ITodayStatsService>();
        
        // Create mock loggers
        var indexPageLoggerMock = new Mock<ILogger<IndexPagePresenterService>>();
        var historyPageLoggerMock = new Mock<ILogger<HistoryPagePresenterService>>();
        var settingsPageLoggerMock = new Mock<ILogger<SettingsPresenterService>>();
        
        // Create concrete service instances (order matters for dependencies)
        AppState = new AppState();
        HistoryStatsServiceMock = new Mock<HistoryStatsService>();
        // Set up the mock to call the base method by default
        HistoryStatsServiceMock.Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>())).CallBase();
        ChartService = new ChartService(JSRuntimeMock.Object);
        TimerThemeFormatter = new TimerThemeFormatter();
        TimeFormatter = new TimeFormatter();
        ChartDataFormatter = new ChartDataFormatter();
        StatCardFormatter = new StatCardFormatter();
        ActivityItemFormatter = new ActivityItemFormatter();
        ActivityTimelineFormatter = new ActivityTimelineFormatter();
        SummaryCardsFormatter = new SummaryCardsFormatter(TimeFormatter);
        IndexPagePresenterService = new IndexPagePresenterService(indexPageLoggerMock.Object);
        HistoryPagePresenterService = new HistoryPagePresenterService(historyPageLoggerMock.Object);
        SettingsPresenterService = new SettingsPresenterService(settingsPageLoggerMock.Object);
        
        // Register all mocks as singletons in test context
        Services.AddSingleton(TimerServiceMock.Object);
        Services.AddSingleton(TaskServiceMock.Object);
        Services.AddSingleton(ActivityServiceMock.Object);
        Services.AddSingleton(StatisticsServiceMock.Object);
        Services.AddSingleton(NotificationServiceMock.Object);
        Services.AddSingleton(ExportServiceMock.Object);
        Services.AddSingleton(ImportServiceMock.Object);
        Services.AddSingleton(IndexedDbServiceMock.Object);
        Services.AddSingleton(ConsentServiceMock.Object);
        Services.AddSingleton(PipTimerServiceMock.Object);
        Services.AddSingleton(KeyboardShortcutServiceMock.Object);
        Services.AddSingleton(JSInteropServiceMock.Object);
        Services.AddSingleton(JSRuntimeMock.Object);
        Services.AddSingleton(InfiniteScrollInteropMock.Object);
        Services.AddSingleton(TodayStatsServiceMock.Object);
        Services.AddSingleton(AppState);
        Services.AddSingleton<IHistoryStatsService>(HistoryStatsServiceMock.Object);
        Services.AddSingleton<IChartService>(ChartService);
        Services.AddSingleton(TimerThemeFormatter);
        Services.AddSingleton(TimeFormatter);
        Services.AddSingleton(ChartDataFormatter);
        Services.AddSingleton(SummaryCardsFormatter);
        Services.AddSingleton(StatCardFormatter);
        Services.AddSingleton(ActivityItemFormatter);
        Services.AddSingleton(ActivityTimelineFormatter);
        Services.AddSingleton(IndexPagePresenterService);
        Services.AddSingleton(HistoryPagePresenterService);
        Services.AddSingleton(SettingsPresenterService);
        
        // Register LocalDateTimeService
        var mockLocalDateTimeService = new Mock<ILocalDateTimeService>();
        mockLocalDateTimeService.Setup(x => x.GetLocalDateAsync()).ReturnsAsync(new DateTime(2023, 1, 2));
        mockLocalDateTimeService.Setup(x => x.GetLocalDateTimeAsync()).ReturnsAsync(new DateTime(2023, 1, 2, 12, 0, 0));
        mockLocalDateTimeService.Setup(x => x.GetTimezoneOffsetAsync()).ReturnsAsync(0);
        Services.AddSingleton<ILocalDateTimeService>(mockLocalDateTimeService.Object);
        Services.AddSingleton<LocalDateTimeService>(sp => new LocalDateTimeService(sp.GetRequiredService<IJSRuntime>()));
    }

    /// <summary>
    /// Creates test activities for testing purposes
    /// </summary>
    public static List<ActivityRecord> CreateTestActivities(int count, int startIndex = 0)
    {
        var activities = new List<ActivityRecord>();
        for (int i = 0; i < count; i++)
        {
            activities.Add(new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = Pomodoro.Web.Models.SessionType.Pomodoro,
                CompletedAt = DateTime.Now.AddMinutes(-(startIndex + i) * 30),
                DurationMinutes = 25,
                TaskId = i % 2 == 0 ? Guid.NewGuid() : null
            });
        }
        return activities;
    }
}
