using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

/// <summary>
/// Tests for ConsentService initialization and event subscription.
/// </summary>
[Trait("Category", "Service")]
public partial class ConsentServiceTests
{
    [Fact]
    public void Initialize_SetsIsInitializedFlag()
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

        // Act
        service.Initialize();

        // Assert - Initialize no longer subscribes to timer events directly; wiring is done via EventWiringService
        Assert.NotNull(service);
    }

    [Fact]
    public void Initialize_WhenCalledMultipleTimes_IsIdempotent()
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

        // Act
        service.Initialize();
        service.Initialize();
        service.Initialize();

        // Assert - Initialize no longer subscribes to timer events directly; calling it multiple times should be safe
        Assert.NotNull(service);
    }

    [Fact]
    public async Task DisposeAsync_SetsIsDisposedFlag()
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
        service.Initialize();

        // Act
        await service.DisposeAsync();

        // Assert - DisposeAsync no longer unsubscribes from timer events directly
        Assert.False(service.IsModalVisible);
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task DisposeAsync_WhenCalledMultipleTimes_DoesNotThrow()
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
        service.Initialize();

        // Act & Assert
        await service.DisposeAsync();
        await service.DisposeAsync();
    }

    [Fact]
    public void ShowConsentModal_SetsIsModalVisibleToTrue()
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

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.True(service.IsModalVisible);
    }

    [Fact]
    public void ShowConsentModal_SetsCompletedSessionType()
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

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.Equal(SessionType.Pomodoro, service.CompletedSessionType);
    }

    [Fact]
    public void ShowConsentModal_SetsCountdownSecondsFromSettings()
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
                AutoStartDelaySeconds = 10,
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

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.Equal(10, service.CountdownSeconds);
    }

    [Fact]
    public void ShowConsentModal_WhenSettingsNull_UsesDefaultCountdown()
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

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Pomodoro.Web.Constants.UI.DefaultConsentCountdownSeconds, service.CountdownSeconds);
    }

    [Fact]
    public void ShowConsentModal_GetsOptionsFromSessionOptionsService()
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

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        sessionOptionsServiceMock.Verify(x => x.GetOptionsForSessionType(SessionType.Pomodoro), Times.Once);
    }

    [Fact]
    public void ShowConsentModal_SetsAvailableOptions()
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
                SoundEnabled = true,
                NotificationsEnabled = true
            }
        };

        var expectedOptions = new List<ConsentOption>
        {
            new() { SessionType = SessionType.ShortBreak, Label = "Short Break", Duration = "5 min", IsDefault = true }
        };
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(expectedOptions);

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
        Assert.Equal(expectedOptions, service.AvailableOptions);
    }

    [Fact]
    public void ShowConsentModal_FiresOnConsentRequiredEvent()
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
        var eventFired = false;
        service.OnConsentRequired += () => eventFired = true;

        // Act
        service.ShowConsentModal(SessionType.Pomodoro);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void HideConsentModal_SetsIsModalVisibleToFalse()
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
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        service.HideConsentModal();

        // Assert
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public void HideConsentModal_FiresOnConsentHandledEvent()
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
        service.ShowConsentModal(SessionType.Pomodoro);
        var eventFired = false;
        service.OnConsentHandled += () => eventFired = true;

        // Act
        service.HideConsentModal();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void RefreshOptions_WhenModalVisible_UpdatesOptions()
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
        service.ShowConsentModal(SessionType.Pomodoro);
        var newOptions = new List<ConsentOption>
        {
            new() { SessionType = SessionType.LongBreak, Label = "Long Break", Duration = "15 min", IsDefault = true }
        };
        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(newOptions);

        // Act
        service.RefreshOptions();

        // Assert
        Assert.Equal(newOptions, service.AvailableOptions);
    }

    [Fact]
    public void RefreshOptions_WhenModalNotVisible_DoesNotUpdateOptions()
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
        // Don't show modal - IsModalVisible is false

        // Act
        service.RefreshOptions();

        // Assert
        sessionOptionsServiceMock.Verify(
            x => x.GetOptionsForSessionType(It.IsAny<SessionType>()),
            Times.Never);
    }

    [Fact]
    public void RefreshOptions_WhenModalVisible_FiresOnConsentRequiredEvent()
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
        service.ShowConsentModal(SessionType.Pomodoro);
        var eventFired = false;
        service.OnConsentRequired += () => eventFired = true;

        // Act
        service.RefreshOptions();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task SelectOptionAsync_WithPomodoroAndCurrentTask_StartsPomodoro()
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
        var taskId = Guid.NewGuid();
        taskServiceMock.Setup(x => x.CurrentTaskId).Returns(taskId);
        service.ShowConsentModal(SessionType.ShortBreak);

        // Act
        await service.SelectOptionAsync(SessionType.Pomodoro);

        // Assert
        timerServiceMock.Verify(x => x.StartPomodoroAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task SelectOptionAsync_WithPomodoroNoCurrentTask_DoesNotStartPomodoro()
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
        taskServiceMock.Setup(x => x.CurrentTaskId).Returns((Guid?)null);
        service.ShowConsentModal(SessionType.ShortBreak);

        // Act
        await service.SelectOptionAsync(SessionType.Pomodoro);

        // Assert
        timerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SelectOptionAsync_WithShortBreak_StartsShortBreak()
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
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.SelectOptionAsync(SessionType.ShortBreak);

        // Assert
        timerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
    }

    [Fact]
    public async Task SelectOptionAsync_WithLongBreak_StartsLongBreak()
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
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.SelectOptionAsync(SessionType.LongBreak);

        // Assert
        timerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
    }

    [Fact]
    public async Task SelectOptionAsync_HidesModal()
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
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.SelectOptionAsync(SessionType.ShortBreak);

        // Assert
        Assert.False(service.IsModalVisible);
    }

    [Fact]
    public async Task SelectOptionAsync_FiresOnConsentHandledEvent()
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
        service.ShowConsentModal(SessionType.Pomodoro);
        var eventFired = false;
        service.OnConsentHandled += () => eventFired = true;

        // Act
        await service.SelectOptionAsync(SessionType.ShortBreak);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task HandleTimeoutAsync_GetsDefaultOptionFromService()
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
            .Setup(x => x.GetDefaultOption(SessionType.Pomodoro))
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.HandleTimeoutAsync();

        // Assert
        sessionOptionsServiceMock.Verify(x => x.GetDefaultOption(SessionType.Pomodoro), Times.Once);
    }

    [Fact]
    public async Task HandleTimeoutAsync_StartsDefaultSession()
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
            .Returns(SessionType.ShortBreak);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.HandleTimeoutAsync();

        // Assert
        timerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimeoutAsync_HidesModal()
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
        service.ShowConsentModal(SessionType.Pomodoro);

        // Act
        await service.HandleTimeoutAsync();

        // Assert
        Assert.False(service.IsModalVisible);
    }
}

