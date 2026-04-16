using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService Start methods.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    public class StartAsyncTests : TimerServiceTests
    {
        [Fact]
        public async Task StartPomodoroAsync_WithTaskId_SetsTaskId()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync(taskId);

            // Assert
            Assert.NotNull(AppState.CurrentSession);
            Assert.Equal(taskId, AppState.CurrentSession.TaskId);
            Assert.Equal(SessionType.Pomodoro, AppState.CurrentSession.Type);
        }

        [Fact]
        public async Task StartPomodoroAsync_WithoutTaskId_SetsTaskIdToNull()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.NotNull(AppState.CurrentSession);
            Assert.Null(AppState.CurrentSession.TaskId);
            Assert.Equal(SessionType.Pomodoro, AppState.CurrentSession.Type);
        }

        [Fact]
        public async Task StartPomodoroAsync_SetsIsRunningToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.IsRunning);
            Assert.True(service.IsRunning);
        }

        [Fact]
        public async Task StartPomodoroAsync_SetsWasStartedToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.WasStarted);
            Assert.True(service.IsStarted);
        }

        [Fact]
        public async Task StartPomodoroAsync_SetsCorrectDuration()
        {
            // Arrange
            var expectedDuration = AppState.Settings.PomodoroMinutes * 60;
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.Equal(expectedDuration, AppState.CurrentSession?.DurationSeconds);
            Assert.Equal(expectedDuration, AppState.CurrentSession?.RemainingSeconds);
        }

        [Fact]
        public async Task StartPomodoroAsync_FiresOnStateChangedEvent()
        {
            // Arrange
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task StartShortBreakAsync_SetsCorrectSessionType()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.Equal(SessionType.ShortBreak, AppState.CurrentSession?.Type);
        }

        [Fact]
        public async Task StartShortBreakAsync_SetsIsRunningToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.IsRunning);
        }

        [Fact]
        public async Task StartShortBreakAsync_SetsWasStartedToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.WasStarted);
        }

        [Fact]
        public async Task StartShortBreakAsync_SetsCorrectDuration()
        {
            // Arrange
            var expectedDuration = AppState.Settings.ShortBreakMinutes * 60;
            var service = CreateService();

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.Equal(expectedDuration, AppState.CurrentSession?.DurationSeconds);
        }

        [Fact]
        public async Task StartShortBreakAsync_TaskIdIsNull()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.Null(AppState.CurrentSession?.TaskId);
        }

        [Fact]
        public async Task StartLongBreakAsync_SetsCorrectSessionType()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartLongBreakAsync();

            // Assert
            Assert.Equal(SessionType.LongBreak, AppState.CurrentSession?.Type);
        }

        [Fact]
        public async Task StartLongBreakAsync_SetsIsRunningToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartLongBreakAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.IsRunning);
        }

        [Fact]
        public async Task StartLongBreakAsync_SetsWasStartedToTrue()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartLongBreakAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.WasStarted);
        }

        [Fact]
        public async Task StartLongBreakAsync_SetsCorrectDuration()
        {
            // Arrange
            var expectedDuration = AppState.Settings.LongBreakMinutes * 60;
            var service = CreateService();

            // Act
            await service.StartLongBreakAsync();

            // Assert
            Assert.Equal(expectedDuration, AppState.CurrentSession?.DurationSeconds);
        }

        [Fact]
        public async Task StartLongBreakAsync_TaskIdIsNull()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartLongBreakAsync();

            // Assert
            Assert.Null(AppState.CurrentSession?.TaskId);
        }

        [Fact]
        public async Task StartPomodoroAsync_GeneratesNewSessionId()
        {
            // Arrange
            var service = CreateService();
            var originalSessionId = Guid.NewGuid();
            AppState.CurrentSession = new TimerSession
            {
                Id = originalSessionId,
                Type = SessionType.Pomodoro
            };

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.NotEqual(originalSessionId, AppState.CurrentSession.Id);
        }

        [Fact]
        public async Task StartPomodoroAsync_SetsIsCompletedToFalse()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.False(AppState.CurrentSession?.IsCompleted);
        }
    }
}

