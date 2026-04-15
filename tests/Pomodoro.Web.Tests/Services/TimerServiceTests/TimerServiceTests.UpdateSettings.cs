using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService.UpdateSettingsAsync method
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    [Trait("Category", "Service")]
    public class UpdateSettingsAsync : TimerServiceTests
    {
        [Fact]
        public async Task ShouldUpdateAppStateSettings()
        {
            // Arrange
            var service = CreateService();
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            Assert.Equal(30, AppState.Settings.PomodoroMinutes);
            Assert.Equal(10, AppState.Settings.ShortBreakMinutes);
            Assert.Equal(20, AppState.Settings.LongBreakMinutes);
        }

        [Fact]
        public async Task ShouldSaveSettingsToRepository()
        {
            // Arrange
            var service = CreateService();
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 45,
                ShortBreakMinutes = 15,
                LongBreakMinutes = 30
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            MockSettingsRepository.Verify(x => x.SaveAsync(newSettings), Times.Once);
        }

        [Fact]
        public async Task ShouldUpdateSessionDuration_WhenTimerNotStarted()
        {
            // Arrange
            var service = CreateService();
            SetupCurrentSession(isRunning: false, wasStarted: false, remainingSeconds: 1500);
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 40,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            Assert.Equal(40 * 60, AppState.CurrentSession!.DurationSeconds);
            Assert.Equal(40 * 60, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task ShouldNotUpdateSessionDuration_WhenTimerWasStarted()
        {
            // Arrange
            var service = CreateService();
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 300);
            var originalDuration = AppState.CurrentSession!.DurationSeconds;
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 40,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert - Duration should not change when timer was already started
            Assert.Equal(originalDuration, AppState.CurrentSession.DurationSeconds);
            Assert.Equal(300, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task ShouldNotUpdateSessionDuration_WhenTimerIsRunning()
        {
            // Arrange
            var service = CreateService();
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1200);
            var originalDuration = AppState.CurrentSession!.DurationSeconds;
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 50,
                ShortBreakMinutes = 15,
                LongBreakMinutes = 25
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert - Duration should not change when timer is running
            Assert.Equal(originalDuration, AppState.CurrentSession.DurationSeconds);
            Assert.Equal(1200, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task ShouldInitializeJsConstants_WithNewSettings()
        {
            // Arrange
            var service = CreateService();
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 35,
                ShortBreakMinutes = 8,
                LongBreakMinutes = 18
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(35, 8, 18))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            MockIndexedDb.Verify(x => x.InitializeJsConstantsAsync(35, 8, 18), Times.Once);
        }

        [Fact]
        public async Task ShouldNotifyStateChanged()
        {
            // Arrange
            var service = CreateService();
            var newSettings = new TimerSettings { PomodoroMinutes = 25 };
            var stateChangedCalled = false;
            service.OnStateChanged += () => stateChangedCalled = true;

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            Assert.True(stateChangedCalled);
        }

        [Fact]
        public async Task ShouldUpdateShortBreakSession_WhenNotStarted()
        {
            // Arrange
            var service = CreateService();
            SetupCurrentSession(isRunning: false, wasStarted: false, remainingSeconds: 300, sessionType: SessionType.ShortBreak);
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 25,
                ShortBreakMinutes = 7,
                LongBreakMinutes = 20
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            Assert.Equal(7 * 60, AppState.CurrentSession!.DurationSeconds);
            Assert.Equal(7 * 60, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task ShouldUpdateLongBreakSession_WhenNotStarted()
        {
            // Arrange
            var service = CreateService();
            SetupCurrentSession(isRunning: false, wasStarted: false, remainingSeconds: 900, sessionType: SessionType.LongBreak);
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 25
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateSettingsAsync(newSettings);

            // Assert
            Assert.Equal(25 * 60, AppState.CurrentSession!.DurationSeconds);
            Assert.Equal(25 * 60, AppState.CurrentSession.RemainingSeconds);
        }

        [Fact]
        public async Task ShouldHandleNullSession()
        {
            // Arrange
            var service = CreateService();
            ClearCurrentSession();
            
            var newSettings = new TimerSettings
            {
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20
            };

            MockSettingsRepository
                .Setup(x => x.SaveAsync(It.IsAny<TimerSettings>()))
                .ReturnsAsync(true);

            MockIndexedDb
                .Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act & Assert - Should not throw
            await service.UpdateSettingsAsync(newSettings);
            
            Assert.Null(AppState.CurrentSession);
            Assert.Equal(30, AppState.Settings.PomodoroMinutes);
        }
    }
}

