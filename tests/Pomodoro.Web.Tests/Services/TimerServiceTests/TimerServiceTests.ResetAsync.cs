using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService ResetAsync method.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    public class ResetAsyncTests : TimerServiceTests
    {
        [Fact]
        public async Task ResetAsync_SetsIsRunningToFalse()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.False(AppState.CurrentSession?.IsRunning);
            Assert.False(service.IsRunning);
        }

        [Fact]
        public async Task ResetAsync_SetsWasStartedToFalse()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.False(AppState.CurrentSession?.WasStarted);
            Assert.False(service.IsStarted);
        }

        [Fact]
        public async Task ResetAsync_ResetsRemainingSecondsToDuration()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 500);
            var service = CreateService();
            var expectedDuration = AppState.CurrentSession!.DurationSeconds;

            // Act
            await service.ResetAsync();

            // Assert
            Assert.Equal(expectedDuration, AppState.CurrentSession?.RemainingSeconds);
        }

        [Fact]
        public async Task ResetAsync_FiresOnStateChangedEvent()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();
            var eventFired = false;
            service.OnTimerStateChanged += () => eventFired = true;

            // Act
            await service.ResetAsync();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task ResetAsync_WhenNoSession_DoesNothing()
        {
            // Arrange
            ClearCurrentSession();
            var service = CreateService();
            var eventFired = false;
            service.OnTimerStateChanged += () => eventFired = true;

            // Act
            await service.ResetAsync();

            // Assert - Event should still fire even without session
            Assert.True(eventFired);
        }

        [Fact]
        public async Task ResetAsync_PreservesSessionType()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true, sessionType: SessionType.ShortBreak);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.Equal(SessionType.ShortBreak, AppState.CurrentSession?.Type);
        }

        [Fact]
        public async Task ResetAsync_PreservesTaskId()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            SetupCurrentSession(isRunning: true, wasStarted: true);
            AppState.CurrentSession!.TaskId = taskId;
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.Equal(taskId, AppState.CurrentSession?.TaskId);
        }

        [Fact]
        public async Task ResetAsync_SetsIsPausedToFalse()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();
            Assert.True(service.IsPaused); // Verify initial state

            // Act
            await service.ResetAsync();

            // Assert - After reset, IsPaused should be false because WasStarted is false
            Assert.False(service.IsPaused);
        }

        [Fact]
        public async Task ResetAsync_ResetsTickCount()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Simulate some ticks
            service.OnTimerTickJs();
            service.OnTimerTickJs();
            service.OnTimerTickJs();
            Assert.Equal(3, service.TickCount);

            // Act
            await service.ResetAsync();

            // Assert
            Assert.Equal(0, service.TickCount);
        }

        [Fact]
        public async Task ResetAsync_WhenPaused_ResetsCorrectly()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 300);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.False(AppState.CurrentSession?.IsRunning);
            Assert.False(AppState.CurrentSession?.WasStarted);
            Assert.Equal(1500, AppState.CurrentSession?.RemainingSeconds); // Default pomodoro duration
        }

        [Fact]
        public async Task ResetAsync_WhenAlreadyReset_DoesNothingHarmful()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: false);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert - Should not throw and state should remain consistent
            Assert.False(AppState.CurrentSession?.IsRunning);
            Assert.False(AppState.CurrentSession?.WasStarted);
        }

        [Theory]
        [InlineData(SessionType.Pomodoro, 1500)]
        [InlineData(SessionType.ShortBreak, 300)]
        [InlineData(SessionType.LongBreak, 900)]
        public async Task ResetAsync_ResetsToCorrectDurationForSessionType(SessionType sessionType, int expectedDurationSeconds)
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true, sessionType: sessionType, remainingSeconds: 10);
            var service = CreateService();

            // Act
            await service.ResetAsync();

            // Assert
            Assert.Equal(expectedDurationSeconds, AppState.CurrentSession?.RemainingSeconds);
        }
    }
}

