using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

/// <summary>
/// Tests for ConsentService timer completion handling.
/// </summary>
public partial class ConsentServiceTests
{
    private static TimerCompletedEventArgs CreateArgs(SessionType sessionType) =>
        new(sessionType, null, null, 25, true, DateTime.UtcNow);

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenAutoStartEnabled_ShowsConsentModal()
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
                AutoStartEnabled = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = true }
            });
        
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        Assert.True(service.IsModalVisible);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenAutoStartDisabled_DoesNotShowConsentModal()
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
                AutoStartEnabled = false,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = true }
            });
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        Assert.False(service.IsModalVisible);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSoundEnabled_PlaysTimerCompleteSound()
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
                AutoStartEnabled = false,
                SoundEnabled = true,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        notificationServiceMock.Verify(x => x.PlayTimerCompleteSoundAsync(), Times.Once);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSoundEnabled_PlaysBreakCompleteSound()
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
                AutoStartEnabled = false,
                SoundEnabled = true,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.ShortBreak));
        
        // Assert
        notificationServiceMock.Verify(x => x.PlayBreakCompleteSoundAsync(), Times.Once);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationsEnabled_ShowsPomodoroNotification()
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
                AutoStartEnabled = false,
                SoundEnabled = false,
                NotificationsEnabled = true
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        notificationServiceMock.Verify(
            x => x.ShowNotificationAsync(
                It.Is<string>(s => s.Contains("🍅")),
                It.IsAny<string>(),
                SessionType.Pomodoro,
                It.IsAny<string?>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationsEnabled_ShowsBreakNotification()
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
                AutoStartEnabled = false,
                SoundEnabled = false,
                NotificationsEnabled = true
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.ShortBreak));
        
        // Assert
        notificationServiceMock.Verify(
            x => x.ShowNotificationAsync(
                It.Is<string>(s => s.Contains("⏱️")),
                It.IsAny<string>(),
                SessionType.ShortBreak,
                It.IsAny<string?>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSoundDisabled_DoesNotPlaySound()
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
                AutoStartEnabled = false,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        notificationServiceMock.Verify(x => x.PlayTimerCompleteSoundAsync(), Times.Never);
        notificationServiceMock.Verify(x => x.PlayBreakCompleteSoundAsync(), Times.Never);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationsDisabled_DoesNotShowNotification()
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
                AutoStartEnabled = false,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
        
        // Assert
        notificationServiceMock.Verify(
            x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionType>(), It.IsAny<string?>()),
            Times.Never);
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSettingsNull_DoesNotThrow()
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
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act & Assert - Should not throw
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
    }
    
    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationServiceNull_DoesNotThrow()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartEnabled = false,
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>());
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            null!,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        // Act & Assert - Should not throw
        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
    }
    
    [Fact]
    public void OnCountdownTick_FiresWhenCountdownDecrements()
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
                AutoStartEnabled = true,
                AutoStartDelaySeconds = 3,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };
        
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = true }
            });
        
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);
        
        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        
        var countdownTicks = new List<int>();
        service.OnCountdownTick += () => countdownTicks.Add(service.CountdownSeconds);
        
        // Act
        service.ShowConsentModal(SessionType.Pomodoro);
        
        // Wait for countdown to tick
        Thread.Sleep(1500);
        
        // Assert - Countdown should have decreased
        Assert.Contains(countdownTicks, t => t < 3);
    }
}
