using System.Reflection;
using System.Threading;
using Bunit;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

public partial class HistoryBaseTests
{
    public class OnParametersSetCoverageTests : HistoryBaseTests
    {
        [Fact]
        public void OnParametersSet_SetsCurrentStats_WhenProvided()
        {
            var stats = new DailyStatsSummary { PomodoroCount = 5, FocusMinutes = 125 };
            var cut = RenderComponent<TestableHistoryBase>(parameters =>
                parameters.Add(p => p.InitialCurrentStats, stats));

            Assert.Equal(5, cut.Instance.CurrentStats.PomodoroCount);
            Assert.Equal(125, cut.Instance.CurrentStats.FocusMinutes);
        }

        [Fact]
        public void OnParametersSet_SetsSelectedDate_WhenProvided()
        {
            var date = new DateTime(2024, 6, 15);
            var cut = RenderComponent<TestableHistoryBase>(parameters =>
                parameters.Add(p => p.InitialSelectedDate, date));

            Assert.Equal(date, cut.Instance.SelectedDate);
        }

        [Fact]
        public void OnParametersSet_SetsSelectedWeekStart_WhenProvided()
        {
            var weekStart = new DateTime(2024, 6, 10);
            var cut = RenderComponent<TestableHistoryBase>(parameters =>
                parameters.Add(p => p.InitialSelectedWeekStart, weekStart));

            Assert.Equal(weekStart, cut.Instance.SelectedWeekStart);
        }

        [Fact]
        public void OnParametersSet_WithNonEmptyActivities_SetsCurrentActivities()
        {
            var activities = new List<ActivityRecord> { new() { Id = Guid.NewGuid() } };
            var cut = RenderComponent<TestableHistoryBase>(parameters =>
                parameters.Add(p => p.InitialActivities, activities));

            Assert.Single(cut.Instance.CurrentActivities);
        }

        [Fact]
        public void OnParametersSet_WithNullActivities_DoesNotOverrideCurrentActivities()
        {
            var cut = RenderComponent<TestableHistoryBase>(parameters =>
                parameters.Add(p => p.InitialActivities, null!));

            Assert.NotNull(cut.Instance.CurrentActivities);
            Assert.Empty(cut.Instance.CurrentActivities);
        }
    }

    public class ObserverRetryCoverageTests : HistoryBaseTests
    {
        [Fact]
        public async Task SetupInfiniteScrollObserver_WhenMaxRetriesExceeded_LogsWarning()
        {
            _mockInfiniteScrollInterop.Setup(x => x.IsSupportedAsync())
                .ReturnsAsync(true);
            _mockInfiniteScrollInterop.Setup(x => x.CreateObserverAsync(
                    It.IsAny<string>(),
                    It.IsAny<DotNetObjectReference<object>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(false);

            var cut = RenderComponent<TestableHistoryBase>();
            cut.Instance.HasMoreActivities = true;

            var method = typeof(HistoryBase).GetMethod("SetupInfiniteScrollObserverAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(cut.Instance, new object[] { 2 })!;

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteRetryAsync_WhenLockContention_LogsDebugAndReturns()
        {
            var cut = RenderComponent<TestableHistoryBase>();
            cut.Instance.HasMoreActivities = true;

            var lockField = typeof(HistoryBase).GetField("_observerSetupLock",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var semaphore = (SemaphoreSlim)lockField!.GetValue(cut.Instance)!;
            await semaphore.WaitAsync();

            try
            {
                var method = typeof(HistoryBase).GetMethod("ExecuteRetryAsync",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                await (Task)method!.Invoke(cut.Instance, new object[] { 1, 100 })!;

                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception?>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    public class SentinelIntersectingCoverageTests : HistoryBaseTests
    {
        [Fact]
        public async Task OnSentinelIntersecting_WhenLoadMoreThrows_CatchesAndDelays()
        {
            var callCount = 0;
            _mockActivityService.Setup(x => x.GetActivitiesPagedAsync(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount > 1)
                        throw new InvalidOperationException("Test error");
                    return new List<ActivityRecord>();
                });

            var cut = RenderComponent<TestableHistoryBase>();
            cut.Instance.HasMoreActivities = true;
            cut.Instance.IsLoadingMore = false;
            cut.Instance.CurrentActivities.Clear();

            await cut.InvokeAsync(async () => await cut.Instance.OnSentinelIntersecting());

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            var field = typeof(HistoryBase).GetField("_isCallbackInProgress",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.False((bool)field!.GetValue(cut.Instance)!);
        }
    }

    public class ActivityChangedCoverageTests : HistoryBaseTests
    {
        [Fact]
        public async Task OnActivityChanged_ReloadsData()
        {
            var cut = RenderComponent<TestableHistoryBase>();
            _mockActivityService.Invocations.Clear();

            _mockActivityService.Raise(x => x.OnActivityChanged += null);
            await cut.InvokeAsync(() => { });

            _mockActivityService.Verify(x => x.GetActivitiesPagedAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.AtLeastOnce);
        }
    }

    public class DateChangedCoverageTests : HistoryBaseTests
    {
        [Fact]
        public async Task HandleDateChanged_WhenObserverInitialized_DestroyObserver()
        {
            _mockInfiniteScrollInterop.Setup(x => x.IsSupportedAsync())
                .ReturnsAsync(true);
            _mockInfiniteScrollInterop.Setup(x => x.CreateObserverAsync(
                    It.IsAny<string>(),
                    It.IsAny<DotNetObjectReference<object>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);

            var cut = RenderComponent<TestableHistoryBase>();
            cut.Instance.HasMoreActivities = true;
            await cut.InvokeAsync(async () => await cut.Instance.OnAfterRenderAsyncProtected(true));

            _mockInfiniteScrollInterop.Invocations.Clear();

            await cut.InvokeAsync(async () => await cut.Instance.HandleDateChangedProtected(DateTime.Now.AddDays(1)));

            _mockInfiniteScrollInterop.Verify(
                x => x.DestroyObserverAsync(It.IsAny<string>()),
                Times.Once);
        }
    }

    public class DisposeNullLockTests : HistoryBaseTests
    {
        [Fact]
        public async Task DisposeAsync_WhenObserverSetupLockIsNull_DoesNotThrow()
        {
            var cut = RenderComponent<TestableHistoryBase>();

            var field = typeof(HistoryBase).GetField("_observerSetupLock",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field!.SetValue(cut.Instance, null);

            await cut.Instance.DisposeAsync();
            Assert.True(true);
        }
    }
}
