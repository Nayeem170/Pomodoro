using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService timer completion handling.
/// </summary>
public partial class TimerServiceTests
{
    public class CompletionTests : TimerServiceTests
    {
        [Fact]
        public async Task TimerCompletion_WithTask_UpdatesDailyStats()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Equal(1, AppState.TodayPomodoroCount);
            Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, AppState.TodayTotalFocusMinutes);
            Assert.Contains(taskId, AppState.TodayTaskIdsWorkedOn);
        }

        [Fact]
        public async Task TimerCompletion_WithoutTask_DoesNotUpdateTaskStats()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            await service.StartPomodoroAsync(); // No task
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - pomodoro count should not increment without task
            Assert.Equal(0, AppState.TodayPomodoroCount);
            Assert.Equal(0, AppState.TodayTotalFocusMinutes);
            Assert.Empty(AppState.TodayTaskIdsWorkedOn);
        }

        [Fact]
        public async Task TimerCompletion_SavesDailyStats()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            MockIndexedDb.Verify(db => db.PutAsync(Constants.Storage.DailyStatsStore, It.IsAny<DailyStats>()), Times.Once);
        }

        [Fact]
        public async Task TimerCompletion_MarksSessionCompleted()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.True(AppState.CurrentSession.IsCompleted);
            Assert.False(AppState.CurrentSession.IsRunning);
        }

        [Fact]
        public async Task TimerCompletion_ResetsRemainingSeconds()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - remaining seconds should be reset to duration
            Assert.Equal(AppState.CurrentSession.DurationSeconds, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task TimerCompletion_RaisesOnTimerCompletedEvent()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            TimerCompletedEventArgs? receivedArgs = null;
            service.OnTimerCompleted += args =>
            {
                receivedArgs = args;
                return Task.CompletedTask;
            };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.NotNull(receivedArgs);
            Assert.Equal(SessionType.Pomodoro, receivedArgs.SessionType);
            Assert.Equal(taskId, receivedArgs.TaskId);
            Assert.Equal("Test Task", receivedArgs.TaskName);
            Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, receivedArgs.DurationMinutes);
            Assert.True(receivedArgs.WasCompleted);
        }

        [Fact]
        public async Task TimerCompletion_RaisesBackwardCompatibleOnTimerComplete()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            SessionType? completedSessionType = null;
            service.OnTimerComplete += (sessionType) => completedSessionType = sessionType;

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Equal(SessionType.Pomodoro, completedSessionType);
        }

        [Fact]
        public async Task TimerCompletion_WithShortBreak_DoesNotUpdatePomodoroCount()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            await service.StartShortBreakAsync();
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Equal(0, AppState.TodayPomodoroCount);
            Assert.Equal(0, AppState.TodayTotalFocusMinutes);
        }

        [Fact]
        public async Task TimerCompletion_WithLongBreak_DoesNotUpdatePomodoroCount()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            await service.StartLongBreakAsync();
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Equal(0, AppState.TodayPomodoroCount);
            Assert.Equal(0, AppState.TodayTotalFocusMinutes);
        }

        [Fact]
        public async Task TimerCompletion_MultiplePomodoros_AccumulatesStats()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();

            // First pomodoro
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Second pomodoro
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Equal(2, AppState.TodayPomodoroCount);
            Assert.Equal(Constants.Timer.DefaultPomodoroMinutes * 2, AppState.TodayTotalFocusMinutes); // 25 * 2
        }

        [Fact]
        public async Task TimerCompletion_WithMultipleTasks_TracksAllTaskIds()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var task1Id = Guid.NewGuid();
            var task2Id = Guid.NewGuid();

            // First pomodoro with task 1
            await service.StartPomodoroAsync(task1Id);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Second pomodoro with task 2
            await service.StartPomodoroAsync(task2Id);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.Contains(task1Id, AppState.TodayTaskIdsWorkedOn);
            Assert.Contains(task2Id, AppState.TodayTaskIdsWorkedOn);
            Assert.Equal(2, AppState.TodayTaskIdsWorkedOn.Count);
        }

        [Fact]
        public async Task TimerCompletion_SameTaskMultipleTimes_OnlyTracksOnce()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();

            // First pomodoro
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Second pomodoro with same task
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - task ID should only appear once
            Assert.Single(AppState.TodayTaskIdsWorkedOn);
            Assert.Contains(taskId, AppState.TodayTaskIdsWorkedOn);
        }

        [Fact]
        public async Task TimerCompletion_WhenDisposed_DoesNotProcess()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act - dispose before completion
            await service.DisposeAsync();
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - stats should not be updated
            Assert.Equal(0, AppState.TodayPomodoroCount);
        }

        [Fact]
        public async Task TimerCompletion_RaisesStateChangedEvent()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            var stateChangedRaised = false;
            service.OnStateChanged += () => stateChangedRaised = true;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            Assert.True(stateChangedRaised);
        }

        [Fact]
        public async Task TimerCompletion_WithEventHandlerException_ContinuesProcessing()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };
            
            var secondHandlerCalled = false;
            
            // First handler throws
            service.OnTimerCompleted += args => throw new InvalidOperationException("Test exception");
            // Second handler should still be called
            service.OnTimerCompleted += args =>
            {
                secondHandlerCalled = true;
                return Task.CompletedTask;
            };
            
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;
            
            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();
            
            // Assert - second handler should still be called despite first handler exception
            Assert.True(secondHandlerCalled);
        }

        [Fact]
        public async Task TimerCompletion_WhenNotRunning_DoesNothing()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            // No session started - verify default session exists
            var initialSession = AppState.CurrentSession;
            var initialIsRunning = initialSession?.IsRunning ?? false;
            var initialWasStarted = initialSession?.WasStarted ?? false;
            var initialIsCompleted = initialSession?.IsCompleted ?? false;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - Default session should exist and be in non-running state
            Assert.NotNull(AppState.CurrentSession);
            Assert.False(AppState.CurrentSession.IsRunning);
            Assert.False(AppState.CurrentSession.WasStarted);
            Assert.False(AppState.CurrentSession.IsCompleted);
            Assert.Equal(initialIsRunning, AppState.CurrentSession.IsRunning);
            Assert.Equal(initialWasStarted, AppState.CurrentSession.WasStarted);
            Assert.Equal(initialIsCompleted, AppState.CurrentSession.IsCompleted);
        }

        [Fact]
        public async Task TimerCompletion_SavesDailyStatsWithCurrentDate()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            var expectedDate = AppState.GetCurrentDayKey();

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            MockIndexedDb.Verify(
                db => db.PutAsync(
                        Constants.Storage.DailyStatsStore,
                        It.Is<DailyStats>(stats => stats.Date == expectedDate)),
                    Times.Once);
        }

        [Fact]
        public async Task TimerCompletion_SavesDailyStatsWithCorrectFocusMinutes()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            MockIndexedDb.Verify(
                db => db.PutAsync(
                        Constants.Storage.DailyStatsStore,
                        It.Is<DailyStats>(stats => stats.TotalFocusMinutes == Constants.Timer.DefaultPomodoroMinutes)),
                    Times.Once);
        }

        [Fact]
        public async Task TimerCompletion_SavesDailyStatsWithCorrectPomodoroCount()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            MockIndexedDb.Verify(
                db => db.PutAsync(
                        Constants.Storage.DailyStatsStore,
                        It.Is<DailyStats>(stats => stats.PomodoroCount == 1)),
                    Times.Once);
        }

        [Fact]
        public async Task TimerCompletion_SavesDailyStatsWithCorrectTaskIds()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert
            MockIndexedDb.Verify(
                db => db.PutAsync(
                        Constants.Storage.DailyStatsStore,
                        It.Is<DailyStats>(stats => stats.TaskIdsWorkedOn.Contains(taskId))),
                    Times.Once);
        }

        [Fact]
        public async Task TimerCompletion_WhenNoSubscribers_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act - No subscribers registered, should not throw
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - Should complete without errors
            Assert.True(AppState.CurrentSession.IsCompleted);
        }

        [Fact]
        public async Task TimerCompletion_WhenSessionIsNull_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();

            AppState.CurrentSession = null;

            // Act - Trigger tick with no session
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - Should not throw, no stats updated
            Assert.Equal(0, AppState.TodayPomodoroCount);
        }

        [Fact]
        public async Task TimerCompletion_WhenDisposedDuringLock_DoesNotProcess()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();

            var taskId = Guid.NewGuid();
            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act - Dispose while timer is running
            await service.DisposeAsync();
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - Should not update stats
            Assert.Equal(0, AppState.TodayPomodoroCount);
        }

        [Fact]
        public async Task TimerCompletion_WhenMultipleSubscribers_AllAreNotified()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Name = "Test Task" };
            AppState.Tasks = new List<TaskItem> { task };

            var handler1Called = false;
            var handler2Called = false;
            var handler3Called = false;

            service.OnTimerCompleted += args =>
            {
                handler1Called = true;
                return Task.CompletedTask;
            };
            service.OnTimerCompleted += args =>
            {
                handler2Called = true;
                return Task.CompletedTask;
            };
            service.OnTimerCompleted += args =>
            {
                handler3Called = true;
                return Task.CompletedTask;
            };

            await service.StartPomodoroAsync(taskId);
            AppState.CurrentSession!.RemainingSeconds = 1;

            // Act
            service.OnTimerTickJs();
            await WaitForCompletionAsync();

            // Assert - All handlers should be called
            Assert.True(handler1Called);
            Assert.True(handler2Called);
            Assert.True(handler3Called);
        }

        private static async Task WaitForCompletionAsync()
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(100);
            }
        }
    }
}