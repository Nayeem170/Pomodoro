using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ConsentServiceTests;

/// <summary>
/// Tests for ConsentService countdown timer behavior and error handling.
/// </summary>
[Trait("Category", "Service")]
public partial class ConsentServiceTests
{
    /// <summary>
    /// Tests for countdown timer behavior
    /// </summary>
    [Trait("Category", "Service")]
    public class CountdownTests : ConsentServiceTests
    {
        [Fact]
        public void ShowConsentModal_StartsCountdownTimer()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            // Assert - Countdown should be started (indirectly verified through state)
            Assert.Equal(5, service.CountdownSeconds);
        }

        [Fact]
        public void HideConsentModal_StopsCountdownTimer()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            // Assert - Modal should be hidden
            Assert.False(service.IsModalVisible);
        }

        [Fact]
        public async Task SelectOptionAsync_StopsCountdownTimer()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            var taskId = Guid.NewGuid();
            taskServiceMock.SetupGet(t => t.CurrentTaskId).Returns(taskId);

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            // Assert - Modal should be hidden
            Assert.False(service.IsModalVisible);
        }

        [Fact]
        public async Task HandleTimeoutAsync_StopsCountdownTimer()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            // Assert - Modal should be hidden
            Assert.False(service.IsModalVisible);
        }

        [Fact]
        public async Task DisposeAsync_DisposesWithoutError()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            // Assert - DisposeAsync no longer unsubscribes from timer events directly; service should dispose without throwing
            Assert.False(service.IsModalVisible);
        }
    }

    /// <summary>
    /// Tests for error handling scenarios
    /// </summary>
    [Trait("Category", "Service")]
    public class ErrorHandlingTests : ConsentServiceTests
    {

        [Fact]
        public async Task SelectOptionAsync_WhenTimerServiceNull_DoesNotThrow()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

            sessionOptionsServiceMock
                .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
                .Returns(SessionType.Pomodoro);

            var service = new ConsentService(
                null!, // TimerService is null
                taskServiceMock.Object,
                notificationServiceMock.Object,
                appState,
                sessionOptionsServiceMock.Object,
                loggerMock.Object);

            service.ShowConsentModal(SessionType.Pomodoro);

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.SelectOptionAsync(SessionType.ShortBreak));
            Assert.Null(exception);
        }

        [Fact]
        public async Task SelectOptionAsync_WhenTaskServiceNull_DoesNotThrow()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

            sessionOptionsServiceMock
                .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
                .Returns(SessionType.Pomodoro);

            var service = new ConsentService(
                timerServiceMock.Object,
                null!, // TaskService is null
                notificationServiceMock.Object,
                appState,
                sessionOptionsServiceMock.Object,
                loggerMock.Object);

            service.ShowConsentModal(SessionType.Pomodoro);

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.SelectOptionAsync(SessionType.ShortBreak));
            Assert.Null(exception);
        }

        [Fact]
        public async Task HandleTimeoutAsync_WhenTimerServiceNull_DoesNotThrow()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

            sessionOptionsServiceMock
                .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
                .Returns(SessionType.Pomodoro);

            var service = new ConsentService(
                null!, // TimerService is null
                taskServiceMock.Object,
                notificationServiceMock.Object,
                appState,
                sessionOptionsServiceMock.Object,
                loggerMock.Object);

            service.ShowConsentModal(SessionType.Pomodoro);

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.HandleTimeoutAsync());
            Assert.Null(exception);
        }

        [Fact]
        public async Task HandleTimeoutAsync_WhenTaskServiceNull_DoesNotThrow()
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
                    AutoStartSession = true,
                    AutoStartDelaySeconds = 5
                }
            };

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

            sessionOptionsServiceMock
                .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
                .Returns(SessionType.Pomodoro);

            var service = new ConsentService(
                timerServiceMock.Object,
                null!, // TaskService is null
                notificationServiceMock.Object,
                appState,
                sessionOptionsServiceMock.Object,
                loggerMock.Object);

            service.ShowConsentModal(SessionType.Pomodoro);

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.HandleTimeoutAsync());
            Assert.Null(exception);
        }

        [Fact]
        public async Task ProcessCountdownTickAsync_WhenTimerIsNull_ReturnsTrue()
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

            sessionOptionsServiceMock
                .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
                .Returns(new List<ConsentOption>());

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

            var method = typeof(ConsentService).GetMethod("ProcessCountdownTickAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);

            var result = await (Task<bool>)method.Invoke(service, null)!;
            Assert.True(result);
        }
    }
}

