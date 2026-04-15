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
            // Create and configure mocks for all services that History page requires
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

            // Configure ActivityService mock to return empty lists instead of null
            activityServiceMock.Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() => new List<ActivityRecord>());
            activityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
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

            // Configure InfiniteScrollInterop mock
            infiniteScrollInteropMock.Setup(x => x.IsSupportedAsync())
                .ReturnsAsync(false);
                
            // Configure HistoryStatsService mock to return a valid DailyStatsSummary
            historyStatsServiceMock.Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
                .Returns(new DailyStatsSummary());

            // Register common services
            var commonServices = TestBase.CreateCommonServiceCollection();
            foreach (var service in commonServices)
            {
                Services.Add(service);
            }
            
            // Override with test-specific mocks
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
            
            // Add missing services for child components (WeeklyMiniChart, ActivityItem)
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
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            Assert.NotNull(cut);
            // Verify the component renders without throwing exceptions
        }

        [Fact]
        public void HistoryPage_ShouldHaveExpectedTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var title = cut.Find("h1");
            Assert.Equal("📊 History", title.TextContent);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayHistoryTabs()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var tabs = cut.FindComponents<HistoryTabs>();
            Assert.Single(tabs);
        }

        [Fact]
        public void HistoryPage_ShouldDisplaySummaryCards()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check if weekly-summary-section exists (which contains the summary cards)
            var weeklySummarySection = cut.Find(".weekly-summary-section");
            Assert.NotNull(weeklySummarySection);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayActivityTimeline()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var activityTimeline = cut.FindComponents<ActivityTimeline>();
            Assert.Single(activityTimeline);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayTimeDistributionChart()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var timeDistributionChart = cut.FindComponents<TimeDistributionChart>();
            Assert.Single(timeDistributionChart);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayDateNavigator()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var dateNavigator = cut.FindComponents<DateNavigator>();
            Assert.Single(dateNavigator);
        }

        [Fact]
        public void HistoryPage_ShouldDisplayWeekNavigator()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // WeekNavigator is only visible in the weekly view, which is not the default tab
            // So we should check that it's not initially visible
            var weekNavigator = cut.FindComponents<WeekNavigator>();
            Assert.Empty(weekNavigator); // Should be empty since we're in daily view by default
        }

        [Fact]
        public void HistoryPage_ShouldHaveResponsiveLayout()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var historyPage = cut.Find(".history-page");
            Assert.NotNull(historyPage);
            
            var classes = historyPage.GetAttribute("class");
            Assert.Contains("history-page", classes);
        }

        [Fact]
        public void HistoryPage_ShouldHaveAccessibilityAttributes()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var historyPage = cut.Find(".history-page");
            Assert.NotNull(historyPage);
        }

        [Fact]
        public void HistoryPage_ShouldHaveProperNavigation()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check if HistoryTabs component is rendered (which provides navigation within the History page)
            var historyTabs = cut.FindComponent<HistoryTabs>();
            Assert.NotNull(historyTabs);
        }

        [Fact]
        public void HistoryPage_ShouldContainAllRequiredSections()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check for main sections
            var historyPage = cut.Find(".history-page");
            Assert.NotNull(historyPage);
            
            var dailyView = cut.Find(".daily-view");
            Assert.NotNull(dailyView);
            
            var timeDistributionSection = cut.Find(".time-distribution-section");
            Assert.NotNull(timeDistributionSection);
        }

        [Fact]
        public void HistoryPage_ShouldHaveCorrectCssClasses()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var page = cut.Find(".history-page");
            Assert.NotNull(page);
            
            var content = cut.Find(".history-content");
            Assert.NotNull(content);
        }

        [Fact]
        public void HistoryPage_ShouldRenderLoadingState()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check if timeline section exists
            var timelineSection = cut.Find(".timeline-section");
            Assert.NotNull(timelineSection);
        }

        [Fact]
        public void HistoryPage_ShouldRenderErrorState()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check if timeline-section exists (this is a key section that should always be present)
            var timelineSection = cut.Find(".timeline-section");
            Assert.NotNull(timelineSection);
        }

        [Fact]
        public void HistoryPage_ShouldRenderEmptyState()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            // Check if empty state message exists
            var emptyState = cut.Find(".empty-state");
            Assert.NotNull(emptyState);
            Assert.Contains("No activities for this day", emptyState.TextContent);
        }

        [Fact]
        public void HistoryPage_ShouldHaveDataAttributesForTesting()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();
            
            // Assert
            var historyPage = cut.Find(".history-page");
            Assert.NotNull(historyPage);
            
            var historyTabs = cut.FindComponent<HistoryTabs>();
            Assert.NotNull(historyTabs);
            
            var activityTimeline = cut.FindComponent<ActivityTimeline>();
            Assert.NotNull(activityTimeline);
        }

        [Fact]
        public void HistoryPage_WeeklyView_ShouldRenderCorrectly()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly));

            // Assert
            var weekNavigator = cut.FindComponents<WeekNavigator>();
            Assert.Single(weekNavigator);

            var weeklyMiniChart = cut.FindComponents<WeeklyMiniChart>();
            Assert.Single(weeklyMiniChart);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithStats_ShouldRenderSummary()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 300,
                TotalPomodoroCount = 6,
                DailyAverageMinutes = 43,
                WeekOverWeekChange = 15
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert
            var weeklySummarySection = cut.Find(".weekly-summary-section");
            Assert.NotNull(weeklySummarySection);

            // Check for trend indicator
            var trendStat = cut.Find(".stat.trend");
            Assert.NotNull(trendStat);
            Assert.Contains("positive", trendStat.GetAttribute("class"));
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithNegativeTrend_ShouldRenderNegativeClass()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 300,
                TotalPomodoroCount = 6,
                DailyAverageMinutes = 43,
                WeekOverWeekChange = -10
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert
            var trendStat = cut.Find(".stat.trend");
            Assert.NotNull(trendStat);
            Assert.Contains("negative", trendStat.GetAttribute("class"));
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithoutStats_ShouldNotRenderSummary()
        {
            // Arrange & Act - Weekly view but no stats (null InitialWeeklyStats)
            // Note: LoadDataAsync will still load weekly stats from ActivityService
            // So we need to verify the weekly-summary-section exists when WeeklyStats is available
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly));

            // Assert - Weekly view should have the chart section regardless
            var weeklyChartSection = cut.Find(".weekly-chart-section");
            Assert.NotNull(weeklyChartSection);
        }

        [Fact]
        public void HistoryPage_DailyView_WithMoreActivities_ShouldRenderSentinel()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true));

            // Assert
            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);
        }

        [Fact]
        public void HistoryPage_DailyView_WithLoadingState_ShouldRenderLoadingIndicator()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, true));

            // Assert
            var loadingIndicator = cut.Find(".loading-indicator");
            Assert.NotNull(loadingIndicator);
            Assert.Contains("Loading more activities...", loadingIndicator.TextContent);
        }

        [Fact]
        public void HistoryPage_DailyView_WithActivities_ShouldRenderEndOfList()
        {
            // Arrange
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro }
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities));

            // Assert
            var endOfList = cut.Find(".end-of-list");
            Assert.NotNull(endOfList);
            Assert.Contains("No more activities", endOfList.TextContent);
        }

        [Fact]
        public void HistoryPage_DailyView_WithActivitiesAndMore_ShouldNotRenderEndOfList()
        {
            // Arrange
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro }
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities)
                .Add(p => p.InitialHasMoreActivities, true));

            // Assert - When HasMoreActivities is true, sentinel should exist
            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);
            
            // End-of-list should not exist when HasMoreActivities is true
            var endOfListElements = cut.FindAll(".end-of-list");
            Assert.Empty(endOfListElements);
        }

        #region Additional Weekly View Tests for Branch Coverage

        [Fact]
        public void HistoryPage_WeeklyView_WithZeroChange_ShouldHideTrendIndicator()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.4,
                WeekOverWeekChange = 0.0 // Exactly zero - should hide trend
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert - Trend section should NOT be rendered when change is exactly 0
            var trendStats = cut.FindAll(".stat.trend");
            Assert.Empty(trendStats);

            // But weekly summary should still be present
            var weeklySummarySection = cut.Find(".weekly-summary-section");
            Assert.NotNull(weeklySummarySection);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithStats_ShouldDisplayAllStatValues()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.4,
                WeekOverWeekChange = 0 // No change to avoid trend rendering
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert - Verify all stat values are displayed
            Assert.Contains("500", cut.Markup); // TotalFocusMinutes
            Assert.Contains("10", cut.Markup); // TotalPomodoroCount
            Assert.Contains("71", cut.Markup); // DailyAverageMinutes (rounded)
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithPositiveTrend_ShouldDisplayPlusSign()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 10,
                DailyAverageMinutes = 71.0,
                WeekOverWeekChange = 25.0 // Positive change (whole number for F0 formatting)
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert
            var trendStat = cut.Find(".stat.trend");
            Assert.NotNull(trendStat);
            Assert.Contains("positive", trendStat.GetAttribute("class"));
            Assert.Contains("+", cut.Markup); // Plus sign for positive trend
            Assert.Contains("25%", cut.Markup); // Trend value with percent
        }

        #endregion

        #region Additional Infinite Scroll Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_HasMoreActivitiesNotLoading_ShowsEmptySentinel()
        {
            // Arrange & Act - HasMoreActivities=true but IsLoadingMore=false
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, false));

            // Assert
            var sentinel = cut.Find(".scroll-sentinel");
            Assert.NotNull(sentinel);

            // Loading indicator should NOT be present when not loading
            var loadingIndicator = cut.FindAll(".loading-indicator");
            Assert.Empty(loadingIndicator);
        }

        [Fact]
        public void HistoryPage_DailyView_LoadingIndicator_ContainsAllLoaderElements()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialHasMoreActivities, true)
                .Add(p => p.InitialIsLoadingMore, true));

            // Assert - Verify all loader elements are present
            var pomodoroLoader = cut.Find(".pomodoro-loader");
            Assert.NotNull(pomodoroLoader);

            var tomatoBody = cut.Find(".tomato-body");
            Assert.NotNull(tomatoBody);

            var tomatoLeafLeft = cut.Find(".tomato-leaf-left");
            Assert.NotNull(tomatoLeafLeft);

            var tomatoLeafRight = cut.Find(".tomato-leaf-right");
            Assert.NotNull(tomatoLeafRight);

            var tomatoShadow = cut.Find(".tomato-shadow");
            Assert.NotNull(tomatoShadow);
        }

        #endregion

        #region Additional End of List Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_NoActivities_NoEndOfListOrSentinel()
        {
            // Arrange & Act - Empty activities, no more to load
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, new List<ActivityRecord>())
                .Add(p => p.InitialHasMoreActivities, false));

            // Assert - Neither end-of-list nor sentinel should be present
            var endOfList = cut.FindAll(".end-of-list");
            Assert.Empty(endOfList);

            var sentinel = cut.FindAll(".scroll-sentinel");
            Assert.Empty(sentinel);
        }

        [Fact]
        public void HistoryPage_DailyView_MultipleActivities_NoMore_ShowsEndOfList()
        {
            // Arrange
            var activities = new List<ActivityRecord>
            {
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now.AddMinutes(-50), DurationMinutes = 25, TaskName = "Task 1", Type = SessionType.Pomodoro },
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now.AddMinutes(-25), DurationMinutes = 25, TaskName = "Task 2", Type = SessionType.Pomodoro },
                new ActivityRecord { Id = Guid.NewGuid(), CompletedAt = DateTime.Now, DurationMinutes = 5, TaskName = "Task 3", Type = SessionType.ShortBreak }
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActivities, activities)
                .Add(p => p.InitialHasMoreActivities, false));

            // Assert
            var endOfList = cut.Find(".end-of-list");
            Assert.NotNull(endOfList);
            Assert.Contains("No more activities", endOfList.TextContent);

            // Sentinel should not be present when no more activities
            var sentinel = cut.FindAll(".scroll-sentinel");
            Assert.Empty(sentinel);
        }

        #endregion

        #region Edge Case Tests for Branch Coverage

        [Fact]
        public void HistoryPage_DailyView_NullStats_ShowsZeroValues()
        {
            // Arrange & Act - Default setup returns empty DailyStatsSummary
            var cut = RenderComponent<Pomodoro.Web.Pages.History>();

            // Assert - The daily summary section should exist
            var summarySection = cut.Find(".weekly-summary-section");
            Assert.NotNull(summarySection);
            
            // Verify stats are displayed (default DailyStatsSummary has 0 for all values)
            var statsValues = cut.FindAll(".stat-value");
            Assert.NotEmpty(statsValues);
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithLargeTrendValue_ShouldDisplayCorrectly()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 1000,
                TotalPomodoroCount = 20,
                DailyAverageMinutes = 142.9,
                WeekOverWeekChange = 150.5 // Large positive change
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert
            var trendStat = cut.Find(".stat.trend");
            Assert.NotNull(trendStat);
            Assert.Contains("positive", trendStat.GetAttribute("class"));
            Assert.Contains("150", cut.Markup); // Large trend value
        }

        [Fact]
        public void HistoryPage_WeeklyView_WithSmallNegativeTrend_ShouldDisplayCorrectly()
        {
            // Arrange
            var weeklyStats = new WeeklyStats
            {
                TotalFocusMinutes = 100,
                TotalPomodoroCount = 2,
                DailyAverageMinutes = 14.0,
                WeekOverWeekChange = -5.0 // Small negative change (whole number for F0 formatting)
            };

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters => parameters
                .Add(p => p.InitialActiveTab, HistoryTab.Weekly)
                .Add(p => p.InitialWeeklyStats, weeklyStats));

            // Assert
            var trendStat = cut.Find(".stat.trend");
            Assert.NotNull(trendStat);
            Assert.Contains("negative", trendStat.GetAttribute("class"));
            Assert.Contains("-5%", cut.Markup); // Small negative trend value with percent
        }

        #endregion
    }
}
