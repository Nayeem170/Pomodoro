using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

/// <summary>
/// Tests for edge cases and error handling in ConsentService.
/// </summary>
[Trait("Category", "Service")]
public partial class ConsentServiceTests
{
    [Fact]
    public async Task PlayCompletionSoundAndNotifyAsync_WhenAppStateIsNull_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();

        var appState = (AppState?)null;

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState!,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act & Assert - should not throw
        var args = new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, DateTime.UtcNow);
        await service.HandleTimerCompletedAsync(args);
    }

    [Fact]
    public async Task PlayCompletionSoundAndNotifyAsync_WhenSettingsIsNull_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();

        var appState = new AppState
        {
            Settings = null!
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act & Assert - should not throw
        var args = new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, DateTime.UtcNow);
        await service.HandleTimerCompletedAsync(args);
    }

    [Fact]
    public void ShowConsentModal_WhenAppStateIsNull_UsesDefaultCountdown()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();

        // AppState is null
        var appState = (AppState?)null;

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState!,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Pomodoro.Web.Constants.UI.DefaultConsentCountdownSeconds, service.CountdownSeconds);
    }

    [Fact]
    public void ShowConsentModal_WhenSettingsIsNull_UsesDefaultCountdown()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();

        // AppState exists but Settings is null
        var appState = new AppState
        {
            Settings = null!
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Pomodoro.Web.Constants.UI.DefaultConsentCountdownSeconds, service.CountdownSeconds);
    }

    [Fact]
    public async Task RunCountdownAsync_WhenTimerIsNullAfterLock_ReturnsGracefully()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = true,
                AutoStartBreaks = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act - show and immediately hide modal to trigger timer null after lock scenario
        service.ShowConsentModal(SessionType.Pomodoro);

        // Wait a bit to allow the timer to start
        await Task.Delay(200);

        // Hide the modal which stops the timer
        service.HideConsentModal();

        // Wait a bit more to allow the countdown to process
        await Task.Delay(200);

        // Assert - service should be in a stable state
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public void ShowConsentModal_WhenTimerServiceIsNull_DoesNotThrow()
    {
        // Arrange
        ITimerService? timerServiceMock = null;
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = true,
                AutoStartBreaks = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock!,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act & Assert - should not throw even with null timer service
        service.ShowConsentModal(SessionType.Pomodoro);
        Assert.True(service.IsModalVisible);
    }

    [Fact]
    public void ShowConsentModal_WhenTaskServiceIsNull_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        ITaskService? taskServiceMock = null;
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = true,
                AutoStartBreaks = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock!,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act & Assert - should not throw even with null task service
        service.ShowConsentModal(SessionType.Pomodoro);
        Assert.True(service.IsModalVisible);
    }

    [Fact]
    public void ShowConsentModal_WhenNotificationServiceIsNull_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        INotificationService? notificationServiceMock = null;
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = true,
                AutoStartBreaks = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock!,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act & Assert - should not throw even with null notification service
        service.ShowConsentModal(SessionType.Pomodoro);
        Assert.True(service.IsModalVisible);
    }

    [Fact]
    public async Task RunCountdownAsync_WhenCountdownTimerBecomesNullAfterLock_ReturnsGracefully()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act - Show modal, immediately hide it to trigger countdown cleanup
        service.ShowConsentModal(SessionType.Pomodoro);
        await Task.Delay(50);
        service.HideConsentModal();
        await Task.Delay(50);

        // Assert - service should be stable
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task RunCountdownAsync_WhenModalHiddenDuringCountdown_BreaksFromLoop()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 10,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act - Start modal, wait a bit, then hide while countdown is running
        service.ShowConsentModal(SessionType.Pomodoro);

        await Task.Run(async () =>
        {
            await Task.Delay(100);
            service.HideConsentModal();
        });

        await Task.Delay(200);

        // Assert - modal should be hidden
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task RunCountdownAsync_WhenServiceDisposed_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act - Show modal and immediately dispose service
        service.ShowConsentModal(SessionType.Pomodoro);
        await Task.Delay(100);
        await service.DisposeAsync();

        await Task.Delay(200);

        // Assert - service should be disposed without throwing
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task RunCountdownAsync_WhenModalHiddenDuringIteration_BreaksFromLoop()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        // Act - Start countdown, immediately hide modal to trigger !IsModalVisible check during countdown
        service.ShowConsentModal(SessionType.Pomodoro);

        await Task.Run(async () =>
        {
            await Task.Delay(50);
            service.HideConsentModal();
        });

        await Task.Delay(400);

        // Assert - modal should be hidden and countdown should have stopped
        Assert.False(service.IsModalVisible);
    }
}

