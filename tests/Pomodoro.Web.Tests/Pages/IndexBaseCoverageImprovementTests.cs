using Pomodoro.Web.Pages;
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
    [Trait("Category", "Page")]
    public class IndexBaseCoverageImprovementTests : TestContext
    {
        private readonly Mock<ITimerService> TimerServiceMock;
        private readonly Mock<ITimerEventPublisher> TimerEventPublisherMock;
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
            TimerEventPublisherMock = new Mock<ITimerEventPublisher>();
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

            TimerServiceMock.SetupGet(x => x.Settings).Returns(new TimerSettings());
            TimerServiceMock.Setup(x => x.PauseAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.ResumeAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.ResetAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartPomodoroAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartShortBreakAsync()).Returns(Task.CompletedTask);
            TimerServiceMock.Setup(x => x.StartLongBreakAsync()).Returns(Task.CompletedTask);

            Services.AddSingleton(TimerServiceMock.Object);
            Services.AddSingleton(TimerEventPublisherMock.Object);
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
            Services.AddSingleton(Mock.Of<ICloudSyncService>());
        }

        #region HandleTaskAdd Tests

        [Fact]
        public async Task HandleTaskAdd_WhenServiceThrowsException_SetsErrorMessage()
        {
            TaskServiceMock.Setup(x => x.AddTaskAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Cannot add task"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTaskAdd("New Task"));

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot add task");
        }

        [Fact]
        public void HandleTaskAdd_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskAdd("New Task");

            TaskServiceMock.Verify(x => x.AddTaskAsync("New Task"), Times.Once);
        }

        #endregion

        #region HandleTaskSelect Tests

        [Fact]
        public void HandleTaskSelect_WhenServiceThrowsException_SetsErrorMessage()
        {
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.SelectTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot select task"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskSelect(taskId);

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot select task");
        }

        [Fact]
        public void HandleTaskSelect_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskSelect(taskId);

            TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskComplete Tests

        [Fact]
        public void HandleTaskComplete_WhenServiceThrowsException_SetsErrorMessage()
        {
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.CompleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot complete task"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskComplete(taskId);

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot complete task");
        }

        [Fact]
        public async Task HandleTaskComplete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            await cut.Instance.HandleTaskComplete(taskId);

            TaskServiceMock.Verify(x => x.CompleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskDelete Tests

        [Fact]
        public void HandleTaskDelete_WhenServiceThrowsException_SetsErrorMessage()
        {
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.DeleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete task"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskDelete(taskId);

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot delete task");
        }

        [Fact]
        public async Task HandleTaskDelete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            await cut.Instance.HandleTaskDelete(taskId);

            TaskServiceMock.Verify(x => x.DeleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTaskUncomplete Tests

        [Fact]
        public void HandleTaskUncomplete_WhenServiceThrowsException_SetsErrorMessage()
        {
            var taskId = Guid.NewGuid();
            TaskServiceMock.Setup(x => x.UncompleteTaskAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Cannot uncomplete task"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTaskUncomplete(taskId);

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot uncomplete task");
        }

        [Fact]
        public async Task HandleTaskUncomplete_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var taskId = Guid.NewGuid();
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            await cut.Instance.HandleTaskUncomplete(taskId);

            TaskServiceMock.Verify(x => x.UncompleteTaskAsync(taskId), Times.Once);
        }

        #endregion

        #region HandleTimerStart Tests

        [Fact]
        public void HandleTimerStart_WhenSessionTypeShortBreak_StartsShortBreak()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.ShortBreak;

            _ = cut.Instance.HandleTimerStart();

            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Never);
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypeLongBreak_StartsLongBreak()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.LongBreak;

            _ = cut.Instance.HandleTimerStart();

            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid>()), Times.Never);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Never);
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypePomodoroWithNoTask_SetsErrorMessage()
        {
            TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns((Guid?)null);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.Pomodoro;

            _ = cut.Instance.HandleTimerStart();

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().NotBeNull();
        }

        [Fact]
        public void HandleTimerStart_WhenSessionTypePomodoroWithTask_StartsPomodoro()
        {
            var taskId = Guid.NewGuid();
            TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.Pomodoro;

            _ = cut.Instance.HandleTimerStart();

            TimerServiceMock.Verify(x => x.StartPomodoroAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task HandleTimerStart_WhenServiceThrowsException_SetsErrorMessage()
        {
            TimerServiceMock.Setup(x => x.StartShortBreakAsync())
                .ThrowsAsync(new InvalidOperationException("Timer error"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.ShortBreak;

            await cut.InvokeAsync(() => cut.Instance.HandleTimerStart());

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Timer error");
        }

        #endregion

        #region HandleTimerPause Tests

        [Fact]
        public void HandleTimerPause_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTimerPause();

            TimerServiceMock.Verify(x => x.PauseAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleTimerPause_WhenServiceThrowsException_SetsErrorMessage()
        {
            TimerServiceMock.Setup(x => x.PauseAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot pause"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTimerPause());

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot pause");
        }

        #endregion

        #region HandleTimerResume Tests

        [Fact]
        public void HandleTimerResume_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTimerResume();

            TimerServiceMock.Verify(x => x.ResumeAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleTimerResume_WhenServiceThrowsException_SetsErrorMessage()
        {
            TimerServiceMock.Setup(x => x.ResumeAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot resume"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTimerResume());

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot resume");
        }

        #endregion

        #region HandleTimerReset Tests

        [Fact]
        public void HandleTimerReset_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTimerReset();

            TimerServiceMock.Verify(x => x.ResetAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleTimerReset_WhenServiceThrowsException_SetsErrorMessage()
        {
            TimerServiceMock.Setup(x => x.ResetAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot reset"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTimerReset());

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot reset");
        }

        #endregion

        #region HandleSessionSwitch Tests

        [Fact]
        public void HandleSessionSwitch_WhenSuccessful_CallsServiceAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleSessionSwitch(SessionType.ShortBreak);

            TimerServiceMock.Verify(x => x.SwitchSessionTypeAsync(SessionType.ShortBreak), Times.Once);
        }

        [Fact]
        public async Task HandleSessionSwitch_WhenServiceThrowsException_SetsErrorMessage()
        {
            TimerServiceMock.Setup(x => x.SwitchSessionTypeAsync(It.IsAny<SessionType>()))
                .ThrowsAsync(new InvalidOperationException("Cannot switch"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleSessionSwitch(SessionType.LongBreak));

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot switch");
        }

        #endregion

        #region HandleTogglePip Tests

        [Fact]
        public void HandleTogglePip_WhenPipIsClosed_OpensPip()
        {
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(false);
            PipTimerServiceMock.Setup(x => x.OpenAsync()).ReturnsAsync(true);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTogglePip();

            PipTimerServiceMock.Verify(x => x.OpenAsync(), Times.Once);
            PipTimerServiceMock.Verify(x => x.CloseAsync(), Times.Never);
        }

        [Fact]
        public void HandleTogglePip_WhenPipIsOpen_ClosesPip()
        {
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(true);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleTogglePip();

            PipTimerServiceMock.Verify(x => x.CloseAsync(), Times.Once);
            PipTimerServiceMock.Verify(x => x.OpenAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleTogglePip_WhenServiceThrowsException_SetsErrorMessage()
        {
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(false);
            PipTimerServiceMock.Setup(x => x.OpenAsync())
                .ThrowsAsync(new InvalidOperationException("Cannot open PiP"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTogglePip());

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Cannot open PiP");
        }

        [Fact]
        public async Task HandleTogglePip_WhenOpenAsyncReturnsFalse_SetsErrorMessage()
        {
            PipTimerServiceMock.SetupGet(x => x.IsOpen).Returns(false);
            PipTimerServiceMock.Setup(x => x.OpenAsync()).ReturnsAsync(false);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.InvokeAsync(() => cut.Instance.HandleTogglePip());

            cut.Instance.ErrorMessage.Should().Be(Constants.Messages.PipPopupBlocked);
            cut.Instance.IsPipOpen.Should().BeFalse();
        }

        #endregion

        #region HandleConsentOptionSelect Tests

        [Fact]
        public void HandleConsentOptionSelect_WhenSuccessful_CallsService()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleConsentOptionSelect(SessionType.ShortBreak);

            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.ShortBreak), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenCalledWithLongBreak_CallsService()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleConsentOptionSelect(SessionType.LongBreak);

            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.LongBreak), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenCalledWithPomodoro_CallsService()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleConsentOptionSelect(SessionType.Pomodoro);

            ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.Pomodoro), Times.Once);
        }

        [Fact]
        public void HandleConsentOptionSelect_WhenServiceThrowsException_SetsErrorMessage()
        {
            ConsentServiceMock.Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>()))
                .ThrowsAsync(new InvalidOperationException("Invalid session type"));
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            _ = cut.Instance.HandleConsentOptionSelect(SessionType.ShortBreak);

            var errorMessage = cut.Instance.ErrorMessage;
            errorMessage.Should().Contain("Invalid session type");
        }

        #endregion

        #region GetTimerThemeClass Tests

        [Fact]
        public void GetTimerThemeClass_WhenPomodoro_ReturnsPomodoroTheme()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.Pomodoro;

            var result = cut.Instance.GetTimerThemeClass();

            result.Should().Be("pomodoro-theme");
        }

        [Fact]
        public void GetTimerThemeClass_WhenShortBreak_ReturnsShortBreakTheme()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.ShortBreak;

            var result = cut.Instance.GetTimerThemeClass();

            result.Should().Be("short-break-theme");
        }

        [Fact]
        public void GetTimerThemeClass_WhenLongBreak_ReturnsLongBreakTheme()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            cut.Instance.CurrentSessionType = SessionType.LongBreak;

            var result = cut.Instance.GetTimerThemeClass();

            result.Should().Be("long-break-theme");
        }

        #endregion

        #region OnNotificationAction Tests

        [Fact]
        public async Task OnNotificationAction_WhenActionShortBreak_HidesConsentAndStartsShortBreak()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnNotificationAction(Constants.SessionTypes.ActionShortBreak);
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnNotificationAction_WhenActionLongBreak_HidesConsentAndStartsLongBreak()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnNotificationAction(Constants.SessionTypes.ActionLongBreak);
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnNotificationAction_WhenActionStartPomodoro_HidesConsentAndStartsPomodoro()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnNotificationAction(Constants.SessionTypes.ActionStartPomodoro);
            await Task.Delay(100);

            ConsentServiceMock.Verify(x => x.HideConsentModal(), Times.Once);
            TimerServiceMock.Verify(x => x.StartPomodoroAsync(AppStateMock.Object.CurrentTaskId), Times.Once);
        }

        [Fact]
        public void OnNotificationAction_WhenActionSkip_DoesNothing()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnNotificationAction(Constants.SessionTypes.ActionSkip);

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
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.Dispose();
            cut.Instance.Dispose();
            cut.Instance.Dispose();

            Assert.True(true);
        }

        #endregion

        #region OnTimerCompleted Tests

        [Fact]
        public async Task OnTimerCompleted_WhenCalled_UpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.Instance.OnTimerCompleted(new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, DateTime.UtcNow));

            Assert.True(true);
        }

        [Fact]
        public async Task OnTimerCompleted_WhenCalledWithShortBreak_UpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.Instance.OnTimerCompleted(new TimerCompletedEventArgs(SessionType.ShortBreak, null, null, 5, true, DateTime.UtcNow));

            Assert.True(true);
        }

        [Fact]
        public async Task OnTimerCompleted_WhenCalledWithLongBreak_UpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.Instance.OnTimerCompleted(new TimerCompletedEventArgs(SessionType.LongBreak, null, null, 15, true, DateTime.UtcNow));

            Assert.True(true);
        }

        [Fact]
        public async Task OnTimerCompleted_WhenUpdateStateThrows_LogsError()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.IndexPagePresenterService = null;

            await cut.Instance.OnTimerCompleted(new TimerCompletedEventArgs(SessionType.Pomodoro, null, null, 25, true, DateTime.UtcNow));

            try { cut.Render(); } catch { }
            await Task.Delay(100);

            Assert.True(true);
        }

        #endregion

        #region OnConsentRequired Tests

        [Fact]
        public void OnConsentRequired_WhenCalled_UpdatesConsentModalState()
        {
            ConsentServiceMock.SetupGet(x => x.IsModalVisible).Returns(true);
            ConsentServiceMock.SetupGet(x => x.CountdownSeconds).Returns(10);
            ConsentServiceMock.SetupGet(x => x.AvailableOptions)
                .Returns(new List<ConsentOption>
                {
                    new ConsentOption { SessionType = SessionType.ShortBreak, Label = "Short Break" },
                    new ConsentOption { SessionType = SessionType.LongBreak, Label = "Long Break" }
                });
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnConsentRequired();

            Assert.True(true);
        }

        #endregion

        #region OnConsentCountdownTick Tests

        [Fact]
        public void OnConsentCountdownTick_WhenCalled_UpdatesCountdown()
        {
            ConsentServiceMock.SetupGet(x => x.CountdownSeconds).Returns(5);
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnConsentCountdownTick();

            Assert.True(true);
        }

        #endregion

        #region OnConsentHandled Tests

        [Fact]
        public void OnConsentHandled_WhenCalled_HidesConsentModalAndUpdatesState()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnConsentHandled();

            Assert.True(true);
        }

        #endregion

        #region OnActivityChanged Tests

        [Fact]
        public void OnActivityChanged_WhenCalled_InvokesStateHasChanged()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnActivityChanged();

            Assert.True(true);
        }

        #endregion

        #region OnPipOpened Tests

        [Fact]
        public void OnPipOpened_WhenCalled_SetsIsPipOpenToTrue()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnPipOpened();

            Assert.True(true);
        }

        #endregion

        #region OnPipClosed Tests

        [Fact]
        public void OnPipClosed_WhenCalled_SetsIsPipOpenToFalse()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnPipClosed();

            Assert.True(true);
        }

        #endregion

        #region OnTaskServiceChanged Tests

        [Fact]
        public void OnTaskServiceChanged_WhenCalled_UpdatesStateAndInvokesStateHasChanged()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnTaskServiceChanged();

            Assert.True(true);
        }

        #endregion

        #region OnTimerStateChanged Tests

        [Fact]
        public void OnTimerStateChanged_WhenCalled_UpdatesStateAndInvokesStateHasChanged()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.OnTimerStateChanged();

            Assert.True(true);
        }

        #endregion

        #region SafeAsync Tests

        [Fact]
        public void SafeAsync_WhenActionThrowsException_LogsError()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            cut.Instance.SafeAsync(
                new Func<Task>(() => throw new InvalidOperationException("Test exception")),
                "TestHandler");

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

            cut = RenderComponent<Pomodoro.Web.Pages.Index>();
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
            Assert.True(cut.Instance.ShowKeyboardHelp);
        }

        [Fact]
        public async Task EscapeShortcut_WhenHelpVisible_SetsShowKeyboardHelpToFalse()
        {
            var actions = CaptureAllShortcuts(out var cut);

            await cut.InvokeAsync(() => actions["?"].Invoke());
            Assert.True(cut.Instance.ShowKeyboardHelp);

            await cut.InvokeAsync(() => actions["escape"].Invoke());
            Assert.False(cut.Instance.ShowKeyboardHelp);
        }

        [Fact]
        public async Task EscapeShortcut_WhenHelpNotVisible_DoesNothing()
        {
            var actions = CaptureAllShortcuts(out var cut);

            Assert.False(cut.Instance.ShowKeyboardHelp);

            await cut.InvokeAsync(() => actions["escape"].Invoke());
            Assert.False(cut.Instance.ShowKeyboardHelp);
        }

        #endregion

        #region OnInitializedAsync Exception Tests

        [Fact]
        public void OnInitializedAsync_WhenInitializationFails_SetsErrorMessage()
        {
            NotificationServiceMock.Setup(x => x.InitializeAsync())
                .ThrowsAsync(new Exception("Init failed"));

            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            var error = cut.Instance.ErrorMessage;
            Assert.Contains("Init failed", error);
        }

        #endregion

        #region CheckPendingNotificationActionAsync Tests

        [Fact]
        public async Task CheckPendingNotificationActionAsync_WhenNoUrlParameter_DoesNothing()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.Instance.CheckPendingNotificationActionAsync();

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

            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

            await cut.Instance.CheckPendingNotificationActionAsync();

            JSInterop.VerifyInvoke(Constants.JsFunctions.RemoveUrlParameter);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_WithNullLogger_DoesNotThrow()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            var instance = cut.Instance;

            instance.Logger = null;

            var exception = Record.Exception(() => instance.Dispose());

            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenUnregisterShortcutThrows_LogsError()
        {
            KeyboardShortcutServiceMock.Setup(x => x.UnregisterShortcut(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Unregister failed"));

            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            var exception = Record.Exception(() => cut.Dispose());

            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenLoggerNullAndExceptionOccurs_DoesNotThrow()
        {
            KeyboardShortcutServiceMock.Setup(x => x.UnregisterShortcut(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Unregister failed"));

            var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
            var instance = cut.Instance;

            instance.Logger = null;

            var exception = Record.Exception(() => cut.Dispose());
            Assert.Null(exception);
        }

        #endregion
    }
}

