using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

[Trait("Category", "Service")]
public partial class ConsentServiceTests
{
    private static TimerCompletedEventArgs CreateArgs(SessionType sessionType) =>
        new(sessionType, null, null, 25, true, DateTime.UtcNow);

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenAutoStartSessionEnabled_ShowsConsentModal()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = true,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

        Assert.True(service.IsModalVisible);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenAutoStartSessionDisabled_DoesNotShowConsentModal()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSoundEnabled_PlaysTimerCompleteSound()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

        notificationServiceMock.Verify(x => x.PlayTimerCompleteSoundAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSoundEnabled_PlaysBreakCompleteSound()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.ShortBreak));

        notificationServiceMock.Verify(x => x.PlayBreakCompleteSoundAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationsEnabled_ShowsPomodoroNotification()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

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
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.ShortBreak));

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
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

        notificationServiceMock.Verify(x => x.PlayTimerCompleteSoundAsync(), Times.Never);
        notificationServiceMock.Verify(x => x.PlayBreakCompleteSoundAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationsDisabled_DoesNotShowNotification()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));

        notificationServiceMock.Verify(
            x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionType>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenSettingsNull_DoesNotThrow()
    {
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenNotificationServiceNull_DoesNotThrow()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = false,
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

        await service.HandleTimerCompletedAsync(CreateArgs(SessionType.Pomodoro));
    }

    [Fact]
    public void OnCountdownTick_FiresWhenCountdownDecrements()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = true,
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

        service.ShowConsentModal(SessionType.Pomodoro);

        Thread.Sleep(1500);

        Assert.Contains(countdownTicks, t => t < 3);
    }
}
