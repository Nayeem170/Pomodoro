using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService PauseAsync method.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    [Trait("Category", "Service")]
    public class PauseAsyncTests : TimerServiceTests
    {
        [Fact]
        public async Task PauseAsync_WhenRunning_SetsIsRunningToFalse()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act
            await service.PauseAsync();

            // Assert
            Assert.False(AppState.CurrentSession?.IsRunning);
            Assert.False(service.IsRunning);
        }

        [Fact]
        public async Task PauseAsync_WhenRunning_PreservesWasStarted()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act
            await service.PauseAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.WasStarted);
            Assert.True(service.IsStarted);
        }

        [Fact]
        public async Task PauseAsync_WhenRunning_FiresOnStateChangedEvent()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.PauseAsync();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task PauseAsync_WhenNotRunning_DoesNothing()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.PauseAsync();

            // Assert
            Assert.False(eventFired);
        }

        [Fact]
        public async Task PauseAsync_WhenNoSession_DoesNothing()
        {
            // Arrange
            ClearCurrentSession();
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.PauseAsync();

            // Assert
            Assert.False(eventFired);
        }

        [Fact]
        public async Task PauseAsync_PreservesRemainingSeconds()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 750);
            var service = CreateService();

            // Act
            await service.PauseAsync();

            // Assert
            Assert.Equal(750, AppState.CurrentSession?.RemainingSeconds);
        }

        [Fact]
        public async Task PauseAsync_PreservesSessionType()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true, sessionType: SessionType.ShortBreak);
            var service = CreateService();

            // Act
            await service.PauseAsync();

            // Assert
            Assert.Equal(SessionType.ShortBreak, AppState.CurrentSession?.Type);
        }

        [Fact]
        public async Task PauseAsync_SetsIsPausedToTrue()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act
            await service.PauseAsync();

            // Assert
            Assert.True(service.IsPaused);
        }
    }
}

