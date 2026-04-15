using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    [Trait("Category", "Service")]
    public class CoverageTests : TimerServiceTests
    {
        #region Property getters with null session (lines 38, 41, 43, 47, 48)

        [Fact]
        public void Properties_WhenSessionIsNull_ReturnDefaults()
        {
            var service = CreateService();

            Assert.Equal(0, service.RemainingSeconds);
            Assert.Null(service.CurrentSession);
            Assert.False(service.IsRunning);
            Assert.False(service.IsPaused);
            Assert.False(service.IsStarted);
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
        }

        #endregion

        #region OnTimerTickJs with null session (line 147 null branch)

        [Fact]
        public void OnTimerTickJs_WhenSessionIsNull_ReturnsImmediately()
        {
            var service = CreateService();
            var exception = Record.Exception(() => service.OnTimerTickJs());
            Assert.Null(exception);
        }

        #endregion

        #region OnTimerTickJs with sync context (lines 168, 169-171)

        [Fact]
        public async Task OnTimerTickJs_WithSyncContext_PostsTickViaSyncContext()
        {
            var tickRaised = false;
            var originalSyncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());
            try
            {
                MockIndexedDb
                    .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync((DailyStats?)null);
                MockIndexedDb
                    .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.CompletedTask);

                var service = CreateService();
                await service.InitializeAsync();
                await service.StartPomodoroAsync();

                service.OnTick += () => tickRaised = true;
                AppState.CurrentSession!.RemainingSeconds = 100;

                service.OnTimerTickJs();

                Assert.True(tickRaised);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalSyncContext);
            }
        }

        #endregion

        #region InitializeAsync with null TaskIdsWorkedOn (line 89)

        [Fact]
        public async Task InitializeAsync_WithNullTaskIdsWorkedOn_InitializesEmptyList()
        {
            var todayKey = AppState.GetCurrentDayKey();
            var dailyStats = new DailyStats
            {
                Date = todayKey,
                TotalFocusMinutes = 10,
                PomodoroCount = 1,
                TaskIdsWorkedOn = null
            };

            MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync((TimerSettings?)null);
            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(dailyStats);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.InitializeAsync();

            Assert.NotNull(AppState.TodayTaskIdsWorkedOn);
            Assert.Empty(AppState.TodayTaskIdsWorkedOn);
        }

        #endregion

        #region StartJsTimerAsync error paths (lines 341-345, 351-367)

        [Fact]
        public async Task StartPomodoroAsync_WhenAudioUnlockFails_StillStartsTimer()
        {
            var jsRuntime = new TestJsRuntime();
            jsRuntime.OnInvoke(Constants.NotificationJsFunctions.UnlockAudio, () => throw new JSException("Audio unlock failed"));

            var service = new TimerService(
                MockIndexedDb.Object, MockSettingsRepository.Object, new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object), AppState, jsRuntime, MockLogger.Object);

            var exception = await Record.ExceptionAsync(() => service.StartPomodoroAsync());

            Assert.Null(exception);
            MockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task StartPomodoroAsync_WhenTimerStartFails_RetriesSuccessfully()
        {
            var callCount = 0;
            var jsRuntime = new TestJsRuntime();
            jsRuntime.OnInvoke(Constants.JsFunctions.TimerStart, () =>
            {
                callCount++;
                if (callCount == 1)
                    throw new JSException("Timer start failed");
                return default;
            });

            var service = new TimerService(
                MockIndexedDb.Object, MockSettingsRepository.Object, new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object), AppState, jsRuntime, MockLogger.Object);

            var exception = await Record.ExceptionAsync(() => service.StartPomodoroAsync());

            Assert.Null(exception);
            Assert.Equal(2, callCount);
            MockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task StartPomodoroAsync_WhenTimerStartAndRetryBothFail_LogsError()
        {
            var jsRuntime = new TestJsRuntime();
            jsRuntime.OnInvoke(Constants.JsFunctions.TimerStart, () => throw new JSException("Timer start failed"));

            var service = new TimerService(
                MockIndexedDb.Object, MockSettingsRepository.Object, new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object), AppState, jsRuntime, MockLogger.Object);

            var exception = await Record.ExceptionAsync(() => service.StartPomodoroAsync());

            Assert.Null(exception);
            MockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region StopJsTimer error path (lines 376-379)

        [Fact]
        public async Task PauseAsync_WhenStopTimerFails_DoesNotThrow()
        {
            var jsRuntime = new TestJsRuntime();
            jsRuntime.OnInvoke(Constants.JsFunctions.TimerStop, () => throw new JSException("Timer stop failed"));

            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DailyStats?)null);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = new TimerService(
                MockIndexedDb.Object, MockSettingsRepository.Object, new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object), AppState, jsRuntime, MockLogger.Object);
            await service.InitializeAsync();
            await service.StartPomodoroAsync();

            var exception = await Record.ExceptionAsync(() => service.PauseAsync());

            Assert.Null(exception);
            MockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region SwitchSessionTypeAsync with null session (line 250)

        [Fact]
        public async Task SwitchSessionTypeAsync_WhenSessionIsNull_CreatesNewSessionWithNullTaskId()
        {
            var service = CreateService();

            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            Assert.NotNull(AppState.CurrentSession);
            Assert.Null(AppState.CurrentSession.TaskId);
            Assert.Equal(SessionType.ShortBreak, AppState.CurrentSession.Type);
            Assert.False(AppState.CurrentSession.IsRunning);
        }

        #endregion

        #region HandleTimerCompleteAsync with null session (line 387)

        [Fact]
        public async Task OnTimerTickJs_WhenSessionClearedBeforeAsyncCompletion_DoesNotThrow()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DailyStats?)null);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();
            AppState.CurrentSession!.RemainingSeconds = 1;

            service.OnTimerTickJs();
            AppState.CurrentSession = null;

            await Task.Delay(500);

            Assert.Null(AppState.CurrentSession);
        }

        #endregion

        #region HandleTimerCompleteSafeAsync semaphore contention (lines 452-454)

        [Fact]
        public async Task OnTimerTickJs_WhenCalledRapidly_SecondCompletionSkipsDueToLock()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DailyStats?)null);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            MockIndexedDb
                .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<DailyStats>()))
                .Returns(async () => { await Task.Delay(300); return true; });

            var service = CreateService();
            await service.InitializeAsync();

            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            service.OnTimerTickJs();
            service.OnTimerTickJs();

            await Task.Delay(1000);

            Assert.Equal(1, AppState.TodayPomodoroCount);
        }

        #endregion

        #region HandleTimerCompleteSafeAsync exception handling (lines 462-465)

        [Fact]
        public async Task OnTimerTickJs_WhenHandleCompleteThrows_LogsError()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DailyStats?)null);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            MockIndexedDb
                .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<DailyStats>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var service = CreateService();
            await service.InitializeAsync();

            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            service.OnTimerTickJs();
            await Task.Delay(3000);

            MockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task HandleTimerCompleteSafeAsync_WhenDisposed_SkipsCompletion()
        {
            var jsRuntime = new TestJsRuntime();
            MockIndexedDb
                .Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DailyStats?)null);
            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = new TimerService(
                MockIndexedDb.Object, MockSettingsRepository.Object, new DailyStatsService(MockIndexedDb.Object, AppState, new Mock<ILogger<DailyStatsService>>().Object), AppState, jsRuntime, MockLogger.Object);
            await service.InitializeAsync();

            await service.DisposeAsync();

            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            service.OnTimerTickJs();
            await Task.Delay(500);

            Assert.Equal(0, AppState.TodayPomodoroCount);
        }

        #endregion

        #region OnTick event with subscriber (line 486 non-null branch)

        [Fact]
        public void OnTimerTickJs_WithOnTickSubscriber_InvokesHandler()
        {
            var service = CreateService();
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 100);

            var tickCount = 0;
            service.OnTick += () => tickCount++;

            service.OnTimerTickJs();

            Assert.Equal(1, tickCount);
        }

        #endregion

        #region OnTimerStateChanged event (line 512 non-null branch)

        [Fact]
        public async Task OnTimerStateChanged_WhenSubscriberExists_InvokesHandler()
        {
            var service = CreateService();
            var stateChangedRaised = false;
            service.OnStateChanged += () => stateChangedRaised = true;

            await service.StartPomodoroAsync();

            Assert.True(stateChangedRaised);
        }

        #endregion

        private class ImmediateSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object? state) => d(state);
            public override void Send(SendOrPostCallback d, object? state) => d(state);
        }

        private class TestJsRuntime : IJSRuntime
        {
            private readonly Dictionary<string, Func<ValueTask<object?>>> _configs = new();

            public void OnInvoke(string identifier, Func<ValueTask<object?>> behavior)
            {
                _configs[identifier] = behavior;
            }

            private ValueTask<object?> HandleInvoke(string identifier)
            {
                if (_configs.TryGetValue(identifier, out var behavior))
                    return behavior();
                return default;
            }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default)
            {
                var vt = HandleInvoke(identifier);
                vt.GetAwaiter().GetResult();
                return default;
            }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            {
                var vt = HandleInvoke(identifier);
                vt.GetAwaiter().GetResult();
                return default;
            }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            {
                var vt = HandleInvoke(identifier);
                vt.GetAwaiter().GetResult();
                return default;
            }
        }
    }
}

