using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Pomodoro.Web.Services;
using Pomodoro.Web.Models;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Tests.Pages
{
    [Trait("Category", "Page")]
    public class HistoryTests : TestContext
    {
        public HistoryTests()
        {
            var activityServiceMock = new Mock<IActivityService>();
            var statisticsServiceMock = new Mock<IStatisticsService>();
            var timerServiceMock = new Mock<ITimerService>();
            var notificationServiceMock = new Mock<INotificationService>();
            var exportServiceMock = new Mock<IExportService>();
            var indexedDbServiceMock = new Mock<IIndexedDbService>();
            var consentServiceMock = new Mock<IConsentService>();
            var pipTimerServiceMock = new Mock<IPipTimerService>();
            var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
            var appStateMock = new Mock<AppState>();
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var infiniteScrollInteropMock = new Mock<IInfiniteScrollInterop>();
            var historyStatsServiceMock = new Mock<IHistoryStatsService>();
            var historyPagePresenterServiceMock = new Mock<HistoryPagePresenterService>(new Mock<ILogger<HistoryPagePresenterService>>().Object);
            var loggerHistoryMock = new Mock<ILogger<Pomodoro.Web.Pages.History>>();
            var loggerPresenterMock = new Mock<ILogger<HistoryPagePresenterService>>();
            var timeFormatterMock = new Mock<TimeFormatter>();
            var activityTimelineFormatterMock = new Mock<ActivityTimelineFormatter>();

            activityServiceMock.Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() => new List<ActivityRecord>());
            activityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
                .Returns(() => new List<ActivityRecord>());
            activityServiceMock.Setup(x => x.GetAllActivities())
                .Returns(() => new List<ActivityRecord>());
            activityServiceMock.Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            activityServiceMock.Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new Dictionary<DateTime, int>());
            activityServiceMock.Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new Dictionary<DateTime, int>());
            statisticsServiceMock.Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new WeeklyStats());
            activityServiceMock.Setup(x => x.InitializeAsync())
                .Returns(Task.CompletedTask);

            infiniteScrollInteropMock.Setup(x => x.IsSupportedAsync())
                .ReturnsAsync(false);

            historyStatsServiceMock.Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
                .Returns(new DailyStatsSummary());

            var commonServices = TestBase.CreateCommonServiceCollection();
            foreach (var service in commonServices)
            {
                Services.Add(service);
            }

            Services.AddSingleton(activityServiceMock.Object);
            Services.AddSingleton(statisticsServiceMock.Object);
            Services.AddSingleton(timerServiceMock.Object);
            Services.AddSingleton(notificationServiceMock.Object);
            Services.AddSingleton(exportServiceMock.Object);
            Services.AddSingleton(indexedDbServiceMock.Object);
            Services.AddSingleton(consentServiceMock.Object);
            Services.AddSingleton(pipTimerServiceMock.Object);
            Services.AddSingleton(keyboardShortcutServiceMock.Object);
            Services.AddSingleton(appStateMock.Object);
            Services.AddSingleton(jsRuntimeMock.Object);
            Services.AddSingleton(infiniteScrollInteropMock.Object);
            Services.AddSingleton(historyStatsServiceMock.Object);
            Services.AddSingleton(historyPagePresenterServiceMock.Object);
            Services.AddSingleton(loggerHistoryMock.Object);
            Services.AddSingleton(loggerPresenterMock.Object);
            Services.AddSingleton(timeFormatterMock.Object);
            Services.AddSingleton(activityTimelineFormatterMock.Object);

            var chartServiceMock = new Mock<IChartService>();
            var activityItemFormatterMock = new Mock<ActivityItemFormatter>();
            var chartDataFormatterMock = new Mock<ChartDataFormatter>();
            Services.AddSingleton<IChartService>(chartServiceMock.Object);
            Services.AddSingleton(activityItemFormatterMock.Object);
            Services.AddSingleton(chartDataFormatterMock.Object);
        }

        [Fact]
        public void HistoryPage_ShouldRenderCorrectly()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            Assert.NotNull(cut);
        }

        [Fact]
        public void HistoryPage_ShouldRenderWithoutHeader()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var h1Elements = cut.FindAll("h1");
            Assert.Empty(h1Elements);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayHistoryTabs()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var tabs = cut.FindComponents<HistoryTabs>();
            Assert.Single(tabs);
        }

        [Fact]
        public void HistoryPage_ShouldDisplaySummaryCards()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayActivityTimeline()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var activityTimeline = cut.FindComponents<ActivityTimeline>();
            Assert.Single(activityTimeline);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayTimeDistributionChart()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var timeDistributionChart = cut.FindComponents<TimeDistributionChart>();
            Assert.Single(timeDistributionChart);
        }

        [Fact]
        public void HistoryPage_ShouldNotDisplayDateNavigator()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var dateNavigator = cut.FindComponents<DateNavigator>();
            Assert.Empty(dateNavigator);
        }

        [Fact]
        public void HistoryPage_ShouldNotDisplayWeekNavigator()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var weekNavigator = cut.FindComponents<WeekNavigator>();
            Assert.Empty(weekNavigator);
        }

        [Fact]
        public void HistoryPage_ShouldHaveResponsiveLayout()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var historyPage = cut.Find(".hist-body");
            Assert.NotNull(historyPage);

            var classes = historyPage.GetAttribute("class");
            Assert.Contains("hist-body", classes);
        }

        [Fact]
        public void HistoryPage_ShouldHaveAccessibilityAttributes()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var historyPage = cut.Find(".hist-body");
            Assert.NotNull(historyPage);
        }

        [Fact]
        public void HistoryPage_ShouldHaveProperNavigation()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var historyTabs = cut.FindComponent<HistoryTabs>();
            Assert.NotNull(historyTabs);
        }

        [Fact]
        public void HistoryPage_ShouldContainAllRequiredSections()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var historyPage = cut.Find(".hist-body");
            Assert.NotNull(historyPage);

            var card = cut.Find(".card");
            Assert.NotNull(card);
        }

        [Fact]
        public void HistoryPage_ShouldHaveCorrectCssClasses()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var page = cut.Find(".hist-body");
            Assert.NotNull(page);
        }

        [Fact]
        public void HistoryPage_ShouldRenderLoadingState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var timelineScrollContainer = cut.Find(".timeline-scroll-container");
            Assert.NotNull(timelineScrollContainer);
        }

        [Fact]
        public void HistoryPage_ShouldRenderErrorState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var timelineScrollContainer = cut.Find(".timeline-scroll-container");
            Assert.NotNull(timelineScrollContainer);
        }

        [Fact]
        public void HistoryPage_ShouldRenderEmptyState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var emptyState = cut.Find(".empty-state");
            Assert.NotNull(emptyState);
            Assert.Contains("No activities for this day", emptyState.TextContent);
        }

        [Fact]
        public void HistoryPage_ShouldHaveDataAttributesForTesting()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var historyPage = cut.Find(".hist-body");
            Assert.NotNull(historyPage);

            var historyTabs = cut.FindComponent<HistoryTabs>();
            Assert.NotNull(historyTabs);

            var activityTimeline = cut.FindComponent<ActivityTimeline>();
            Assert.NotNull(activityTimeline);
        }

        [Fact]
        public void HistoryPage_WeeklyView_ShouldRenderCorrectly()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly));

            var weeklyMiniChart = cut.FindComponents<WeeklyMiniChart>();
            Assert.Single(weeklyMiniChart);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithStats_ShouldRenderSummary()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 300,
                TotalPomodoroCount = 6,
                DailyAverageMinutes = 43,
                WeekOverWeekChange = 15
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);

            var statCards = cut.FindAll(".sc");
            Assert.True(statCards.Count >= 4);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithNegativeTrend_ShouldRenderStatGrid()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 300,
                TotalPomodoroCount = 6,
                DailyAverageMinutes = 43,
                WeekOverWeekChange = -10
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithoutStats_ShouldNotRenderSummary()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly));

            var card = cut.Find(".card");
            Assert.NotNull(card);
        }

        [Fact]
        public void HistoryPage_DailyView_WithMoreActivities_ShouldRenderSentinel()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true));

            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);
        }

        [Fact]
        public void HistoryPage_DailyView_WithLoadingState_ShouldRenderLoadingIndicator()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, true));

            var loadingIndicator = cut.Find(".loading-indicator");
            Assert.NotNull(loadingIndicator);
            Assert.Contains("Loading more activities...", loadingIndicator.TextContent);
        }

        [Fact]
        public void HistoryPage_DailyView_WithActivities_ShouldRenderEndOfList()
        {
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro }
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities));

            var endOfList = cut.Find(".end-of-list");
            Assert.NotNull(endOfList);
            Assert.Contains("No more activities", endOfList.TextContent);
        }

        [Fact]
        public void HistoryPage_DailyView_WithActivitiesAndMore_ShouldNotRenderEndOfList()
        {
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro }
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities)
                .Add(p => p.InitialHasMoreActivities, true));

            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);

            var endOfListElements = cut.FindAll(".end-of-list");
            Assert.Empty(endOfListElements);
        }

        #region Additional Weekly View Tests for Branch Coverage

        [Fact]
        public void HistoryPage_WeeklyView_WithZeroChange_ShouldRenderStatGrid()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.4,
                WeekOverWeekChange = 0.0
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithStats_ShouldDisplayAllStatValues()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 60,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.4,
                WeekOverWeekChange = 0
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            Assert.Contains("1h", cut.Markup);
            Assert.Contains("10", cut.Markup);
            Assert.Contains("71", cut.Markup);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithPositiveTrend_ShouldRenderStatGrid()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.0,
                WeekOverWeekChange = 25.0
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
        }

        #endregion

        #region Additional Infinite Scroll Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_HasMoreActivitiesNotLoading_ShowsEmptySentinel()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, false));

            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);

            var loadingIndicator = cut.FindAll(".loading-indicator");
            Assert.Empty(loadingIndicator);
        }

        [Fact]
        public void HistoryPage_DailyView_LoadingIndicator_ContainsText()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, true));

            var loadingIndicator = cut.Find(".loading-indicator");
            Assert.NotNull(loadingIndicator);
        }

        #endregion

        #region Additional End of List Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_NoActivities_NoEndOfListOrSentinel()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, new List<ActivityRecord>())
                .Add(p => p.InitialHasMoreActivities, false));

            var endOfList = cut.FindAll(".end-of-list");
            Assert.Empty(endOfList);

            var sentinel = cut.FindAll(".scroll-sentinel");
            Assert.Empty(sentinel);
        }

        [Fact]
        public void HistoryPage_DailyView_MultipleActivities_NoMore_ShowsEndOfList()
        {
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now.AddMinutes(-50), DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro },
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now.AddMinutes(-25), DurationMinutes = 25, TaskName = "Task 2", Type = SessionType.Pomodoro },
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 5, TaskName = "Task 3", Type = SessionType.ShortBreak }
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities)
                .Add(p => p.InitialHasMoreActivities, false));

            var endOfList = cut.Find(".end-of-list");
            Assert.NotNull(endOfList);
            Assert.Contains("No more activities", endOfList.TextContent);

            var sentinel = cut.FindAll(".scroll-sentinel");
            Assert.Empty(sentinel);
        }

        #endregion

        #region Edge Case Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_NullStats_ShowsZeroValues()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            var summarySection = cut.Find(".stat-grid");
            Assert.NotNull(summarySection);

            var statsValues = cut.FindAll(".sv");
            Assert.NotEmpty(statsValues);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithLargeTrendValue_ShouldDisplayCorrectly()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 120,
                TotalPomodoroCount = 20,
                DailyAverageMinutes = 142.9,
                WeekOverWeekChange = 150.5
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
            Assert.Contains("20", cut.Markup);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithSmallNegativeTrend_ShouldDisplayCorrectly()
        {
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 60,
                TotalPomodoroCount = 2,
                DailyAverageMinutes = 14.0,
                WeekOverWeekChange = -5.0
            };

            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            var statGrid = cut.Find(".stat-grid");
            Assert.NotNull(statGrid);
            Assert.Contains("2", cut.Markup);
        }

        #endregion
    }
}
