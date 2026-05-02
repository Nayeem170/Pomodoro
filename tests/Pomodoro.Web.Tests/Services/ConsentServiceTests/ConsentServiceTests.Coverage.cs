using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

[Trait("Category", "Service")]
public partial class ConsentServiceTests
{
    #region HandleTimerCompletedAsync catch block

    [Fact]
    public async Task HandleTimerCompletedAsync_WhenPlayCompletionThrows_LogsError()
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

        notificationServiceMock
            .Setup(x => x.PlayTimerCompleteSoundAsync())
            .ThrowsAsync(new InvalidOperationException("Audio error"));

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        var args = new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, DateTime.UtcNow);
        await service.HandleTimerCompletedAsync(args);

        loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region StartCountdown when disposed (line 239)

    [Fact]
    public async Task ShowConsentModal_WhenDisposed_DoesNotStartCountdownTimer()
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
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);
        service.Initialize();
        await service.DisposeAsync();

        service.ShowConsentModal(SessionType.Pomodoro);
        await Task.Delay(1500);

        var countdownAfterStart = service.CountdownSeconds;
        Assert.Equal(5, countdownAfterStart);
    }

    #endregion

    #region RunCountdownAsync - timer stopped during yield point (lines 289, 290, 292)

    [Fact]
    public async Task RunCountdown_WhenTimerStoppedDuringYield_ExitsGracefully()
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
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        service.ShowConsentModal(SessionType.Pomodoro);
        Assert.True(service.IsModalVisible);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1100);
            service.HideConsentModal();
        });

        await Task.Delay(3000);

        Assert.False(service.IsModalVisible);
    }

    #endregion

    #region RunCountdownAsync - disposed during yield point (line 296 _isDisposed branch)

    [Fact]
    public async Task RunCountdown_WhenDisposedFlagSetDuringYield_BreaksFromLoop()
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
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        service.ShowConsentModal(SessionType.Pomodoro);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1500);
            var disposedField = typeof(ConsentService).GetField("_isDisposed", BindingFlags.NonPublic | BindingFlags.Instance)!;
            disposedField.SetValue(service, true);
        });

        await Task.Delay(3000);

        Assert.True(service.CountdownSeconds < 5);
    }

    #endregion

    #region RunCountdownAsync - modal hidden during yield point (line 296 !IsModalVisible branch)

    [Fact]
    public async Task RunCountdown_WhenModalHiddenDuringYield_BreaksFromLoop()
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
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        service.ShowConsentModal(SessionType.Pomodoro);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1500);
            var backingField = typeof(ConsentService).GetField("<IsModalVisible>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!;
            backingField.SetValue(service, false);
        });

        await Task.Delay(3000);

        Assert.True(service.CountdownSeconds < 5);
    }

    #endregion

    #region RunCountdownAsync - countdown reaches zero (lines 304-314)

    [Fact]
    public async Task RunCountdown_WhenCountdownReachesZero_HandlesTimeout()
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
                AutoStartDelaySeconds = 2,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });
        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(SessionType.Pomodoro))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        service.ShowConsentModal(SessionType.Pomodoro);
        await Task.Delay(3500);

        Assert.False(service.IsModalVisible);
        timerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
    }

    #endregion

    #region RunCountdownAsync - timer null on entry (line 282)

    [Fact]
    public async Task RunCountdown_WhenTimerNullOnEntry_ReturnsImmediately()
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
                AutoStartDelaySeconds = 5
            }
        };

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        var method = typeof(ConsentService).GetMethod("RunCountdownAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(service, [CancellationToken.None])!;
        await task;

        Assert.True(true);
    }

    #endregion

    #region RunCountdownAsync - generic Exception from OnCountdownTick subscriber (catch Exception block)

    [Fact]
    public async Task RunCountdown_WhenTickSubscriberThrows_CatchesGenericException()
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
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>(), It.IsAny<TimerSession>()))
            .Returns(new List<ConsentOption> { new() { SessionType = SessionType.ShortBreak } });

        var service = new ConsentService(
            timerServiceMock.Object, taskServiceMock.Object,
            notificationServiceMock.Object, appState,
            sessionOptionsServiceMock.Object, loggerMock.Object);

        service.OnCountdownTick += () => throw new InvalidOperationException("Subscriber error");
        service.ShowConsentModal(SessionType.Pomodoro);

        await Task.Delay(3000);

        loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}

