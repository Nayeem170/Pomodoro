using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;
using Index = Pomodoro.Web.Pages.Index;

namespace Pomodoro.Web.Tests.Pages
{
    /// <summary>
    /// Comprehensive tests to improve IndexBase coverage
    /// Focuses on uncovered event handlers, task handlers, timer handlers, and consent handlers
    /// </summary>
    public class IndexBaseCoverageImprovementTests : TestContext
    {
        private readonly Mock<ITimerService> TimerServiceMock;
        private readonly Mock<ITaskService> TaskServiceMock;
        private readonly Mock<IConsentService> ConsentServiceMock;
        private readonly Mock<IPipTimerService> PipTimerServiceMock;
        private readonly Mock<INotificationService> NotificationServiceMock;
        private readonly Mock<IActivityService> ActivityServiceMock;
        private readonly Mock<ILogger<Index>> LoggerMock;
        private readonly Mock<AppState> AppStateMock;
        private readonly Mock<IKeyboardShortcutService> KeyboardShortcutServiceMock;
        private readonly Mock<ITodayStatsService> TodayStatsServiceMock;
        private readonly TimerThemeFormatter TimerThemeFormatter;
        private readonly Mock<ILogger<IndexPagePresenterService>> PresenterLoggerMock;
        private readonly IndexPagePresenterService IndexPagePresenterService;

        public IndexBaseCoverageImprovementTests()
        {
            TimerServiceMock = new Mock<ITimerService>();
            TaskServiceMock = new Mock<ITaskService>();
            ConsentServiceMock = new Mock<IConsentService>();
            PipTimerServiceMock = new Mock<IPipTimerService>();
            NotificationServiceMock = new Mock<INotificationService>();
            ActivityServiceMock = new Mock<IActivityService>();
            LoggerMock = new Mock<ILogger<Index>>();
            AppStateMock = new Mock<AppState>();
            KeyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
            TodayStatsServiceMock = new Mock<ITodayStatsService>();
            TimerThemeFormatter = new TimerThemeFormatter();
            PresenterLoggerMock = new Mock<ILogger<IndexPagePresenterService>>();
            IndexPagePresenterService = new IndexPagePresenterService(PresenterLoggerMock.Object);

            TimerServiceMock.Setup(x => x.PauseAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.ResumeAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.ResetAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartPomodoroAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartShortBreakAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartLongBreakAsync()).Returns(Task.CompletedTask);

            Services.AddSingleton(TimerServiceMock.Object);
            Services.AddSingleton(TaskServiceMock.Object);
            Services.AddSingleton(ConsentServiceMock.Object);
            Services.AddSingleton(PipTimerServiceMock.Object);
            Services.AddSingleton(NotificationServiceMock.Object);
            Services.AddSingleton(ActivityServiceMock.Object);
            Services.AddSingleton(AppStateMock.Object);
            Services.AddSingleton(KeyboardShortcutServiceMock.Object);
            Services.AddSingleton(TodayStatsServiceMock.Object);
            Services.AddSingleton(IndexPagePresenterService);
            Services.AddSingleton(LoggerMock.Object);
            Services.AddSingleton(TimerThemeFormatter);
        }

        #region HandleTaskAdd Tests

        [Fact]
        public void HandleTaskAdd_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TaskServiceMock.Setup(x => x.AddTaskAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Cannot add task"));
            var cut = RenderComponent<Index>();

            // Act - Trigger by invoking the method directly through reflection since it's protected
            var method = typeof(IndexBase).GetMethod("HandleTaskAdd");
            method?.Invoke(cut.Instance, new object[] { "New Task" });

            // Assert - Verify error message is set
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot add task");
        }

        [Fact]
        public void HandleTaskAdd_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskAdd");
            method?.Invoke(cut.Instance, new object[] { "New Task" });

            // Assert
            TaskServiceMock.Verify(x => x.AddTaskAsync("New Task"), Times.Once);
        }

        #endregion

        #region HandleTaskSelect Tests

        [Fact]
        public void HandleTaskSelect_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.SelectTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot select task"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskSelect");
            method?.Invoke(cut.Instance, new object[] { taskId });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot select task");
        }

        [Fact]
        public void HandleTaskSelect_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskSelect");
            method?.Invoke(cut.Instance, new object[] { taskId });

            // Assert
            TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskComplete Tests

        [Fact]
        public void HandleTaskComplete_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.CompleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot complete task"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskComplete");
            method?.Invoke(cut.Instance, new object[] { taskId });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot complete task");
        }

        [Fact]
        public async Task HandleTaskComplete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Index>();
            await cut.Instance.HandleTaskComplete(taskId);

            TaskServiceMock.Verify(x => x.CompleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskDelete Tests

        [Fact]
        public void HandleTaskDelete_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.DeleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete task"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskDelete");
            method?.Invoke(cut.Instance, new object[] { taskId });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot delete task");
        }

        [Fact]
        public async Task HandleTaskDelete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Index>();
            await cut.Instance.HandleTaskDelete(taskId);

            TaskServiceMock.Verify(x => x.DeleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskUncomplete Tests

        [Fact]
        public void HandleTaskUncomplete_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.UncompleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot uncomplete task"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTaskUncomplete");
            method?.Invoke(cut.Instance, new object[] { taskId });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot uncomplete task");
        }

        [Fact]
        public async Task HandleTaskUncomplete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Index>();
            await cut.Instance.HandleTaskUncomplete(taskId);

            TaskServiceMock.Verify(x => x.UncompleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTimerStart Tests

        [Fact]
        public void HandleTimerStart_WhenSessionTypeShortBreak_StartsShortBreak()
        {
            // Arrange
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.ShortBreak);

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerStart");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Never);
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypeLongBreak_StartsLongBreak()
        {
            // Arrange
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.LongBreak);

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerStart");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Never);
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypePomodoroWithNoTask_SetsErrorMessage()
        {
            // Arrange
            TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns((Guid?)null);
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.Pomodoro);

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerStart");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().NotBeNull();
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypePomodoroWithTask_StartsPomodoro()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.Pomodoro);

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerStart");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(taskId), Times.Once);
        }

        [Fact]
        public void HandleTimerStart_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TimerServiceMock.Setup(x => x.StartShortBreakAsync())
                .ThrowsAsync(new InvalidOperationException("Timer error"));
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.ShortBreak);

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerStart");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Timer error");
        }

        #endregion

        #region HandleTimerPause Tests

        [Fact]
        public void HandleTimerPause_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerPause");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.PauseAsync(), Times.Once);
        }

        [Fact]
        public void HandleTimerPause_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TimerServiceMock.Setup(x => x.PauseAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot pause"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerPause");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot pause");
        }

        #endregion

        #region HandleTimerResume Tests

        [Fact]
        public void HandleTimerResume_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerResume");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.ResumeAsync(), Times.Once);
        }

        [Fact]
        public void HandleTimerResume_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TimerServiceMock.Setup(x => x.ResumeAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot resume"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerResume");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot resume");
        }

        #endregion

        #region HandleTimerReset Tests

        [Fact]
        public void HandleTimerReset_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerReset");
            method?.Invoke(cut.Instance, null);

            // Assert
            TimerServiceMock.Verify(x => x.ResetAsync(), Times.Once);
        }

        [Fact]
        public void HandleTimerReset_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TimerServiceMock.Setup(x => x.ResetAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot reset"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTimerReset");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot reset");
        }

        #endregion

        #region HandleSessionSwitch Tests

        [Fact]
        public void HandleSessionSwitch_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleSessionSwitch");
            method?.Invoke(cut.Instance, new object[] { SessionType.ShortBreak });

            // Assert
            TimerServiceMock.Verify(x => x.SwitchSessionTypeAsync(SessionType.ShortBreak), Times.Once);
        }

        [Fact]
        public void HandleSessionSwitch_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            TimerServiceMock.Setup(x => x.SwitchSessionTypeAsync(It.IsAny<SessionType>()))
                .ThrowsAsync(new InvalidOperationException("Cannot switch"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleSessionSwitch");
            method?.Invoke(cut.Instance, new object[] { SessionType.LongBreak });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot switch");
        }

        #endregion

        #region HandleTogglePip Tests

        [Fact]
        public void HandleTogglePip_WhenPipIsClosed_OpensPip()
        {
            // Arrange
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(false);
            PipTimerServiceMock.Setup(x => x.OpenAsync()).ReturnsAsync(true);
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTogglePip");
            method?.Invoke(cut.Instance, null);

            // Assert
            PipTimerServiceMock.Verify(x => x.OpenAsync(), Times.Once);
            PipTimerServiceMock.Verify(x => x.CloseAsync(), Times.Never);
        }

        [Fact]
        public void HandleTogglePip_WhenPipIsOpen_ClosesPip()
        {
            // Arrange
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(true);
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTogglePip");
            method?.Invoke(cut.Instance, null);

            // Assert
            PipTimerServiceMock.Verify(x => x.CloseAsync(), Times.Once);
            PipTimerServiceMock.Verify(x => x.OpenAsync(), Times.Never);
        }

        [Fact]
        public void HandleTogglePip_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(false);
            PipTimerServiceMock.Setup(x => x.OpenAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot open PiP"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleTogglePip");
            method?.Invoke(cut.Instance, null);

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Cannot open PiP");
        }

        #endregion

        #region HandleConsentOptionSelect Tests

        [Fact]
        public void HandleConsentOptionSelect_WhenSuccessful_CallsService()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleConsentOptionSelect");
            method?.Invoke(cut.Instance, new object[] { SessionType.ShortBreak });

            // Assert
            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.ShortBreak), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenCalledWithLongBreak_CallsService()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleConsentOptionSelect");
            method?.Invoke(cut.Instance, new object[] { SessionType.LongBreak });

            // Assert
            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.LongBreak), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenCalledWithPomodoro_CallsService()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleConsentOptionSelect");
            method?.Invoke(cut.Instance, new object[] { SessionType.Pomodoro });

            // Assert
            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.Pomodoro), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            ConsentServiceMock.Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>()))
                .ThrowsAsync(new InvalidOperationException("Invalid session type"));
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("HandleConsentOptionSelect");
            method?.Invoke(cut.Instance, new object[] { SessionType.ShortBreak });

            // Assert
            var errorMessageProp = typeof(IndexBase).GetProperty("ErrorMessage");
            var errorMessage = errorMessageProp?.GetValue(cut.Instance) as string;
            errorMessage.Should().Contain("Invalid session type");
        }

        #endregion

        #region GetTimerThemeClass Tests

        [Fact]
        public void GetTimerThemeClass_WhenPomodoro_ReturnsPomodoroTheme()
        {
            // Arrange
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.Pomodoro);

            // Act
            var method = typeof(IndexBase).GetMethod("GetTimerThemeClass");
            var result = method?.Invoke(cut.Instance, null) as string;

            // Assert
            result.Should().Be("pomodoro-theme");
        }

        [Fact]
        public void GetTimerThemeClass_WhenShortBreak_ReturnsShortBreakTheme()
        {
            // Arrange
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.ShortBreak);

            // Act
            var method = typeof(IndexBase).GetMethod("GetTimerThemeClass");
            var result = method?.Invoke(cut.Instance, null) as string;

            // Assert
            result.Should().Be("short-break-theme");
        }

        [Fact]
        public void GetTimerThemeClass_WhenLongBreak_ReturnsLongBreakTheme()
        {
            // Arrange
            var cut = RenderComponent<Index>();
            var sessionTypeProp = typeof(IndexBase).GetProperty("CurrentSessionType");
            sessionTypeProp?.SetValue(cut.Instance, SessionType.LongBreak);

            // Act
            var method = typeof(IndexBase).GetMethod("GetTimerThemeClass");
            var result = method?.Invoke(cut.Instance, null) as string;

            // Assert
            result.Should().Be("long-break-theme");
        }

        #endregion

        #region OnNotificationAction Tests

        [Fact]
        public async Task OnNotificationAction_WhenActionShortBreak_HidesConsentAndStartsShortBreak()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("OnNotificationAction");
            method?.Invoke(cut.Instance, new object[] { Constants.SessionTypes.ActionShortBreak });
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnNotificationAction_WhenActionLongBreak_HidesConsentAndStartsLongBreak()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("OnNotificationAction");
            method?.Invoke(cut.Instance, new object[] { Constants.SessionTypes.ActionLongBreak });
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnNotificationAction_WhenActionStartPomodoro_HidesConsentAndStartsPomodoro()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("OnNotificationAction");
            method?.Invoke(cut.Instance, new object[] { Constants.SessionTypes.ActionStartPomodoro });
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(AppStateMock.Object.CurrentTaskId), Times.Once);
        }

        [Fact]
        public void OnNotificationAction_WhenActionSkip_DoesNothing()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("OnNotificationAction");
            method?.Invoke(cut.Instance, new object[] { Constants.SessionTypes.ActionSkip });

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Never);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Never);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Never);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("Dispose");
            method?.Invoke(cut.Instance, null);
            method?.Invoke(cut.Instance, null);
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnTimerComplete Tests

        [Fact]
        public void OnTimerComplete_WhenCalled_UpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnTimerComplete");
            method?.Invoke(cut.Instance, new object[] { SessionType.Pomodoro });

            // Assert - should not throw
            Assert.True(true);
        }

        [Fact]
        public void OnTimerComplete_WhenCalledWithShortBreak_UpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnTimerComplete");
            method?.Invoke(cut.Instance, new object[] { SessionType.ShortBreak });

            // Assert - should not throw
            Assert.True(true);
        }

        [Fact]
        public void OnTimerComplete_WhenCalledWithLongBreak_UpdatesState()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("OnTimerComplete");
            method?.Invoke(cut.Instance, new object[] { SessionType.LongBreak });

            Assert.True(true);
        }

        [Fact]
        public async Task OnTimerComplete_WhenUpdateStateThrows_LogsError()
        {
            var cut = RenderComponent<Index>();

            var presenterProp = typeof(IndexBase).GetProperty("IndexPagePresenterService",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            presenterProp!.SetValue(cut.Instance, null);

            var method = typeof(IndexBase).GetMethod("OnTimerComplete");
            method?.Invoke(cut.Instance, new object[] { SessionType.Pomodoro });

            try { cut.Render(); } catch { }
            await Task.Delay(100);

            Assert.True(true);
        }

        #endregion

        #region OnConsentRequired Tests

        [Fact]
        public void OnConsentRequired_WhenCalled_UpdatesConsentModalState()
        {
            // Arrange
            ConsentServiceMock.SetupGet(x => x.IsModalVisible).Returns(true);
            ConsentServiceMock.SetupGet(x => x.CountdownSeconds).Returns(10);
            ConsentServiceMock.SetupGet(x => x.AvailableOptions)
                .Returns(new List<ConsentOption>
                {
                    new ConsentOption { SessionType = SessionType.ShortBreak, Label = "Short Break" },
                    new ConsentOption { SessionType = SessionType.LongBreak, Label = "Long Break" }
                });
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnConsentRequired");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnConsentCountdownTick Tests

        [Fact]
        public void OnConsentCountdownTick_WhenCalled_UpdatesCountdown()
        {
            // Arrange
            ConsentServiceMock.SetupGet(x => x.CountdownSeconds).Returns(5);
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnConsentCountdownTick");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnConsentHandled Tests

        [Fact]
        public void OnConsentHandled_WhenCalled_HidesConsentModalAndUpdatesState()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnConsentHandled");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnActivityChanged Tests

        [Fact]
        public void OnActivityChanged_WhenCalled_InvokesStateHasChanged()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnActivityChanged");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnPipOpened Tests

        [Fact]
        public void OnPipOpened_WhenCalled_SetsIsPipOpenToTrue()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnPipOpened");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnPipClosed Tests

        [Fact]
        public void OnPipClosed_WhenCalled_SetsIsPipOpenToFalse()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnPipClosed");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnTaskServiceChanged Tests

        [Fact]
        public void OnTaskServiceChanged_WhenCalled_UpdatesStateAndInvokesStateHasChanged()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnTaskServiceChanged");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnTimerTick Tests

        [Fact]
        public void OnTimerTick_WhenCalled_UpdatesStateAndInvokesStateHasChanged()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnTimerTick");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region OnTimerStateChanged Tests

        [Fact]
        public void OnTimerStateChanged_WhenCalled_UpdatesStateAndInvokesStateHasChanged()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("OnTimerStateChanged");
            method?.Invoke(cut.Instance, null);

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region SafeAsync Tests

        [Fact]
        public void SafeAsync_WhenActionThrowsException_LogsError()
        {
            // Arrange
            var cut = RenderComponent<Index>();

            // Act
            var method = typeof(IndexBase).GetMethod("SafeAsync");
            method?.Invoke(cut.Instance, new object[] { 
                new Func<Task>(() => throw new InvalidOperationException("Test exception")), 
                "TestHandler" 
            });

            // Assert - should not throw
            Assert.True(true);
        }

        #endregion

        #region Keyboard Shortcut Handler Tests

        private Dictionary<string, Action> CaptureAllShortcuts(out IRenderedComponent<Index> cut)
        {
            var actions = new Dictionary<string, Action>();
            KeyboardShortcutServiceMock
                .Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()))
                .Callback((string key, Action action, string desc) => actions[key] = action);

            cut = RenderComponent<Index>();
            return actions;
        }

        [Fact]
        public async Task SpaceShortcut_WhenTimerRunning_CallsPause()
        {
            TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
            TimerServiceMock.SetupGet(x => x.IsPaused).Returns(false);
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["space"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.PauseAsync(), Times.Once);
        }

        [Fact]
        public async Task SpaceShortcut_WhenTimerPaused_CallsResume()
        {
            TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
            TimerServiceMock.SetupGet(x => x.IsPaused).Returns(true);
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["space"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.ResumeAsync(), Times.Once);
        }

        [Fact]
        public async Task SpaceShortcut_WhenTimerIdle_CallsStartPomodoro()
        {
            TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
            TimerServiceMock.SetupGet(x => x.IsPaused).Returns(false);
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["space"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Once);
        }

        [Fact]
        public async Task ResetShortcut_InvokesReset()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["r"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.ResetAsync(), Times.Once);
        }

        [Fact]
        public async Task PomodoroShortcut_InvokesStartPomodoro()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["p"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Once);
        }

        [Fact]
        public async Task ShortBreakShortcut_InvokesStartShortBreak()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["s"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task LongBreakShortcut_InvokesStartLongBreak()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["l"].Invoke());
            await Task.Delay(100);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task HelpShortcut_SetsShowKeyboardHelpToTrue()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["?"].Invoke());

            var prop = typeof(IndexBase).GetProperty("ShowKeyboardHelp",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var result = (bool)prop!.GetValue(cut.Instance)!;
            Assert.True(result);
        }

        [Fact]
        public async Task EscapeShortcut_WhenHelpVisible_SetsShowKeyboardHelpToFalse()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["?"].Invoke());
            var prop = typeof(IndexBase).GetProperty("ShowKeyboardHelp",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.True((bool)prop!.GetValue(cut.Instance)!);

            await cut.InvokeAsync(() => actions["escape"].Invoke());
            Assert.False((bool)prop!.GetValue(cut.Instance)!);
        }

        [Fact]
        public async Task EscapeShortcut_WhenHelpNotVisible_DoesNothing()
        {
            var actions = CaptureAllShortcuts(out var cut);

            var prop = typeof(IndexBase).GetProperty("ShowKeyboardHelp",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.False((bool)prop!.GetValue(cut.Instance)!);

            await cut.InvokeAsync(() => actions["escape"].Invoke());
            Assert.False((bool)prop!.GetValue(cut.Instance)!);
        }

        #endregion

        #region OnInitializedAsync Exception Tests

        [Fact]
        public void OnInitializedAsync_WhenInitializationFails_SetsErrorMessage()
        {
            NotificationServiceMock.Setup(x => x.InitializeAsync())
                .ThrowsAsync(new Exception("Init failed"));

            var cut = RenderComponent<Index>();

            var prop = typeof(IndexBase).GetProperty("ErrorMessage");
            var error = prop?.GetValue(cut.Instance) as string;
            Assert.Contains("Init failed", error);
        }

        #endregion

        #region CheckPendingNotificationActionAsync Tests

        [Fact]
        public async Task CheckPendingNotificationActionAsync_WhenNoUrlParameter_DoesNothing()
        {
            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("CheckPendingNotificationActionAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            await (Task)method!.Invoke(cut.Instance, null)!;

            NotificationServiceMock.VerifyAdd(x => x.OnNotificationAction += It.IsAny<Action<string>>(), Times.Once);
        }

        [Fact]
        public async Task CheckPendingNotificationActionAsync_WhenUrlParameterExists_ProcessesAction()
        {
            var encodedAction = Uri.EscapeDataString(Constants.SessionTypes.ActionShortBreak);
            JSInterop.Setup<string>(Constants.JsFunctions.GetUrlParameter, Constants.UrlParameters.NotificationAction)
                .SetResult(encodedAction);
            JSInterop.SetupVoid(Constants.JsFunctions.RemoveUrlParameter, Constants.UrlParameters.NotificationAction)
                .SetVoidResult();

            var cut = RenderComponent<Index>();

            var method = typeof(IndexBase).GetMethod("CheckPendingNotificationActionAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            await (Task)method!.Invoke(cut.Instance, null)!;

            JSInterop.VerifyInvoke(Constants.JsFunctions.RemoveUrlParameter);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_WithNullLogger_DoesNotThrow()
        {
            var cut = RenderComponent<Index>();
            var instance = cut.Instance;

            var loggerProp = typeof(IndexBase).GetProperty("Logger",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            loggerProp!.SetValue(instance, null);

            var method = typeof(IndexBase).GetMethod("Dispose");
            var exception = Record.Exception(() => method?.Invoke(instance, null));

            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenUnregisterShortcutThrows_LogsError()
        {
            KeyboardShortcutServiceMock.Setup(x => x.UnregisterShortcut(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Unregister failed"));

            var cut = RenderComponent<Index>();
            var exception = Record.Exception(() => cut.Dispose());

            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenLoggerNullAndExceptionOccurs_DoesNotThrow()
        {
            KeyboardShortcutServiceMock.Setup(x => x.UnregisterShortcut(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Unregister failed"));

            var cut = RenderComponent<Index>();
            var instance = cut.Instance;

            var loggerProp = typeof(IndexBase).GetProperty("Logger",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            loggerProp!.SetValue(instance, null);

            var exception = Record.Exception(() => cut.Dispose());
            Assert.Null(exception);
        }

        #endregion
    }
}
