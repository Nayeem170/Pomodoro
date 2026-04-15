using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService.IsPaused property.
/// IsPaused should only return true when:
/// - Session exists
/// - Session is not running
/// - Session was previously started (WasStarted = true)
/// 
/// This ensures that after a reset (when WasStarted = false),
/// IsPaused returns false so PiP toggle logic correctly calls
/// StartPomodoroAsync instead of ResumeAsync.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    /// <summary>
    /// Test class for IsPaused property tests
    /// </summary>
    [Trait("Category", "Service")]
    public class IsPausedTests : TimerServiceTests
    {
        [Fact]
        public void IsPaused_WhenNoSession_ReturnsFalse()
        {
            // Arrange
            ClearCurrentSession();
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsPaused);
        }

        [Fact]
        public void IsPaused_WhenSessionExistsButNotStarted_ReturnsFalse()
        {
            // Arrange - This is the bug fix scenario:
            // After reset, session exists with WasStarted = false
            // IsPaused should return false so PiP starts fresh
            SetupCurrentSession(isRunning: false, wasStarted: false);
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsPaused);
        }

        [Fact]
        public void IsPaused_WhenRunning_ReturnsFalse()
        {
            // Arrange - Running state means not paused
            SetupCurrentSession(isRunning: true, wasStarted: true);
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsPaused);
        }

        [Fact]
        public void IsPaused_WhenPausedAfterStart_ReturnsTrue()
        {
            // Arrange - Normal paused state: was started, then paused
            SetupCurrentSession(isRunning: false, wasStarted: true);
            var service = CreateService();

            // Act & Assert
            Assert.True(service.IsPaused);
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, true, true)]
        public void IsPaused_AllCombinations(
            bool sessionExists,
            bool isRunning,
            bool wasStarted,
            bool expectedIsPaused)
        {
            // Arrange
            if (sessionExists)
            {
                SetupCurrentSession(isRunning, wasStarted);
            }
            else
            {
                ClearCurrentSession();
            }
            var service = CreateService();

            // Act & Assert
            Assert.Equal(expectedIsPaused, service.IsPaused);
        }

        [Fact]
        public void IsPaused_AfterReset_ReturnsFalse()
        {
            // Arrange - Simulate reset state:
            // Session exists but WasStarted was reset to false
            SetupCurrentSession(isRunning: false, wasStarted: false);
            var service = CreateService();

            // Act & Assert
            // After reset, IsPaused should be false so PiP toggle
            // calls StartPomodoroAsync instead of ResumeAsync
            Assert.False(service.IsPaused);
        }

        [Fact]
        public void IsPaused_RunningButNotStarted_EdgeCase_ReturnsFalse()
        {
            // Arrange - Edge case: somehow running without WasStarted
            // This shouldn't happen in practice but IsPaused should still be false
            // because IsRunning = true
            SetupCurrentSession(isRunning: true, wasStarted: false);
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsPaused);
        }
    }
}

