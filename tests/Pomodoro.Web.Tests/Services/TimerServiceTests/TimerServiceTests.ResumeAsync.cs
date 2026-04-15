using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService ResumeAsync method.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    [Trait("Category", "Service")]
    public class ResumeAsyncTests : TimerServiceTests
    {
        [Fact]
        public async Task ResumeAsync_WhenPaused_SetsIsRunningToTrue()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.IsRunning);
            Assert.True(service.IsRunning);
        }

        [Fact]
        public async Task ResumeAsync_WhenPaused_PreservesWasStarted()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.True(AppState.CurrentSession?.WasStarted);
        }

        [Fact]
        public async Task ResumeAsync_WhenPaused_FiresOnStateChangedEvent()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task ResumeAsync_WhenRunning_DoesNothing()
        {
            // Arrange
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();
            var eventCount = 0;
            service.OnStateChanged += () => eventCount++;

            // Act
            await service.ResumeAsync();

            // Assert - Event should not fire again
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public async Task ResumeAsync_WhenNoSession_DoesNothing()
        {
            // Arrange
            ClearCurrentSession();
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.False(eventFired);
        }

        [Fact]
        public async Task ResumeAsync_PreservesRemainingSeconds()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 500);
            var service = CreateService();

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.Equal(500, AppState.CurrentSession?.RemainingSeconds);
        }

        [Fact]
        public async Task ResumeAsync_PreservesSessionType()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true, sessionType: SessionType.LongBreak);
            var service = CreateService();

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.Equal(SessionType.LongBreak, AppState.CurrentSession?.Type);
        }

        [Fact]
        public async Task ResumeAsync_SetsIsPausedToFalse()
        {
            // Arrange
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();
            Assert.True(service.IsPaused); // Verify initial state

            // Act
            await service.ResumeAsync();

            // Assert
            Assert.False(service.IsPaused);
        }

        [Fact]
        public async Task ResumeAsync_WhenNotStartedButSessionExists_StillResumes()
        {
            // Arrange - Edge case: session exists but WasStarted is false
            // This could happen in unusual scenarios
            SetupCurrentSession(isRunning: false, wasStarted: false);
            var service = CreateService();

            // Act
            await service.ResumeAsync();

            // Assert - ResumeAsync doesn't check WasStarted, only IsRunning
            Assert.True(AppState.CurrentSession?.IsRunning);
        }
    }
}

