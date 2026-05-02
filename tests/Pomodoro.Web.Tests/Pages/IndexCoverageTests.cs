using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class IndexCoverageTests : TestHelper
{
    public IndexCoverageTests()
    {
        NotificationServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        PipTimerServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TaskServiceMock.SetupGet(x => x.Tasks).Returns(new List<TaskItem>());
        TaskServiceMock.SetupGet(x => x.AllTasks).Returns(new List<TaskItem>());
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns((Guid?)null);
        TaskServiceMock.SetupGet(x => x.CurrentTask).Returns((TaskItem?)null);
        TimerServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TimerServiceMock.SetupGet(x => x.RemainingTime).Returns(TimeSpan.FromMinutes(25));
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
        TimerServiceMock.SetupGet(x => x.IsPaused).Returns(false);
        TimerServiceMock.SetupGet(x => x.IsStarted).Returns(false);
        TimerServiceMock.SetupGet(x => x.Settings).Returns(new TimerSettings());
        ActivityServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TodayStatsServiceMock.Setup(x => x.GetTodayStats()).Returns((120, 4, 2));
        KeyboardShortcutServiceMock.Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()));
    }

    [Fact]
    public async Task HandleTaskEdit_CallsUpdateTaskAsync()
    {
        TaskServiceMock.Setup(x => x.UpdateTaskAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Updated" };

        await cut.Instance.HandleTaskEdit(task);

        TaskServiceMock.Verify(x => x.UpdateTaskAsync(task), Times.Once);
    }

    [Fact]
    public async Task HandleTaskEdit_OnException_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.UpdateTaskAsync(It.IsAny<TaskItem>())).ThrowsAsync(new Exception("update failed"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Updated" };

        await cut.Instance.HandleTaskEdit(task);

        cut.Instance.ErrorMessage.Should().Contain("update failed");
    }

    [Fact]
    public async Task HandleConsentOptionSelect_IsResume_CallsResumeInterrupted()
    {
        TimerServiceMock.Setup(x => x.ResumeInterruptedPomodoroAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var option = new ConsentOption { IsResume = true, SessionType = SessionType.Pomodoro };

        await cut.Instance.HandleConsentOptionSelect(option);

        TimerServiceMock.Verify(x => x.ResumeInterruptedPomodoroAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleConsentOptionSelect_NotResume_CallsSelectOptionAsync()
    {
        ConsentServiceMock.Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>())).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var option = new ConsentOption { IsResume = false, SessionType = SessionType.ShortBreak };

        await cut.Instance.HandleConsentOptionSelect(option);

        ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.ShortBreak), Times.Once);
    }

    [Fact]
    public async Task HandleConsentOptionSelect_OnException_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.ResumeInterruptedPomodoroAsync()).ThrowsAsync(new Exception("resume failed"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var option = new ConsentOption { IsResume = true, SessionType = SessionType.Pomodoro };

        await cut.Instance.HandleConsentOptionSelect(option);

        cut.Instance.ErrorMessage.Should().Contain("resume failed");
    }

    [Fact]
    public async Task OnTimerCompleted_UpdatesStateSuccessfully()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var args = new TimerCompletedEventArgs(SessionType.Pomodoro, Guid.NewGuid(), "Task", 25, true, DateTime.UtcNow);

        await cut.Instance.OnTimerCompleted(args);

        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task SafeAsyncInternal_Exception_LogsError()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.SafeAsyncInternal(() => throw new Exception("safe error"), "TestHandler");

        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CheckPendingNotificationActionAsync_Exception_LogsError()
    {
        Services.AddSingleton<IJSRuntime>(new ThrowingTestIndexJsRuntime());
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.CheckPendingNotificationActionAsync();

        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void TodayStats_CacheBehavior()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        var prop = typeof(IndexBase).GetProperty("TodayTotalFocusMinutes", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var focus1 = (int)prop!.GetValue(cut.Instance)!;
        var focus2 = (int)prop.GetValue(cut.Instance)!;

        focus1.Should().Be(120);
        focus2.Should().Be(120);
        TodayStatsServiceMock.Verify(x => x.GetTodayStats(), Times.Once);
    }

    [Fact]
    public void OnActivityChanged_InvalidatesCache()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        var prop = typeof(IndexBase).GetProperty("TodayTotalFocusMinutes", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var focus1 = (int)prop!.GetValue(cut.Instance)!;
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        var focus2 = (int)prop.GetValue(cut.Instance)!;

        TodayStatsServiceMock.Verify(x => x.GetTodayStats(), Times.AtLeast(2));
    }

    [Fact]
    public async Task HandleTimerStart_PomodoroWithoutTask_SetsError()
    {
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns((Guid?)null);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTimerStart());

        cut.Instance.ErrorMessage.Should().Contain("select a task");
    }

    [Fact]
    public async Task HandleTimerStart_PomodoroWithTask_StartsPomodoro()
    {
        var taskId = Guid.NewGuid();
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);
        TimerServiceMock.Setup(x => x.StartPomodoroAsync(taskId)).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTimerStart();

        TimerServiceMock.Verify(x => x.StartPomodoroAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTimerStart_ShortBreak_StartsShortBreak()
    {
        TimerServiceMock.Setup(x => x.StartShortBreakAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        cut.Instance.CurrentSessionType = SessionType.ShortBreak;

        await cut.Instance.HandleTimerStart();

        TimerServiceMock.Verify(x => x.StartShortBreakAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerStart_LongBreak_StartsLongBreak()
    {
        TimerServiceMock.Setup(x => x.StartLongBreakAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        cut.Instance.CurrentSessionType = SessionType.LongBreak;

        await cut.Instance.HandleTimerStart();

        TimerServiceMock.Verify(x => x.StartLongBreakAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerStart_Exception_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.StartShortBreakAsync()).ThrowsAsync(new Exception("start error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        cut.Instance.CurrentSessionType = SessionType.ShortBreak;

        await cut.Instance.HandleTimerStart();

        cut.Instance.ErrorMessage.Should().Contain("start error");
    }

    [Fact]
    public async Task HandleTimerPause_Exception_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.PauseAsync()).ThrowsAsync(new Exception("pause error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTimerPause();

        cut.Instance.ErrorMessage.Should().Contain("pause error");
    }

    [Fact]
    public async Task HandleTimerPause_Success_UpdatesState()
    {
        TimerServiceMock.Setup(x => x.PauseAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTimerPause());

        TimerServiceMock.Verify(x => x.PauseAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerResume_Exception_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.ResumeAsync()).ThrowsAsync(new Exception("resume error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTimerResume();

        cut.Instance.ErrorMessage.Should().Contain("resume error");
    }

    [Fact]
    public async Task HandleTimerResume_Success_UpdatesState()
    {
        TimerServiceMock.Setup(x => x.ResumeAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTimerResume());

        TimerServiceMock.Verify(x => x.ResumeAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleTimerReset_Exception_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.ResetAsync()).ThrowsAsync(new Exception("reset error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTimerReset();

        cut.Instance.ErrorMessage.Should().Contain("reset error");
    }

    [Fact]
    public async Task HandleTimerReset_Success_UpdatesState()
    {
        TimerServiceMock.Setup(x => x.ResetAsync()).Returns(Task.CompletedTask);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTimerReset());

        TimerServiceMock.Verify(x => x.ResetAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleSessionSwitch_Exception_SetsErrorMessage()
    {
        TimerServiceMock.Setup(x => x.SwitchSessionTypeAsync(It.IsAny<SessionType>())).ThrowsAsync(new Exception("switch error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleSessionSwitch(SessionType.ShortBreak);

        cut.Instance.ErrorMessage.Should().Contain("switch error");
    }

    [Fact]
    public async Task HandleTogglePip_OpenFails_SetsError()
    {
        PipTimerServiceMock.Setup(x => x.IsOpen).Returns(false);
        PipTimerServiceMock.Setup(x => x.OpenAsync()).ReturnsAsync(false);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTogglePip());

        cut.Instance.ErrorMessage.Should().Contain("Pop-up");
    }

    [Fact]
    public async Task HandleTogglePip_Exception_SetsErrorMessage()
    {
        PipTimerServiceMock.Setup(x => x.IsOpen).Returns(false);
        PipTimerServiceMock.Setup(x => x.OpenAsync()).ThrowsAsync(new Exception("pip error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTogglePip();

        cut.Instance.ErrorMessage.Should().Contain("pip error");
    }

    [Fact]
    public async Task HandleTaskAdd_Exception_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.AddTaskAsync(It.IsAny<string>())).ThrowsAsync(new Exception("add error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskAdd("Test");

        cut.Instance.ErrorMessage.Should().Contain("add error");
    }

    [Fact]
    public async Task HandleTaskSelect_Exception_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.SelectTaskAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("select error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskSelect(Guid.NewGuid());

        cut.Instance.ErrorMessage.Should().Contain("select error");
    }

    [Fact]
    public async Task HandleTaskComplete_Exception_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.CompleteTaskAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("complete error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskComplete(Guid.NewGuid());

        cut.Instance.ErrorMessage.Should().Contain("complete error");
    }

    [Fact]
    public async Task HandleTaskDelete_Exception_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.DeleteTaskAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("delete error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskDelete(Guid.NewGuid());

        cut.Instance.ErrorMessage.Should().Contain("delete error");
    }

    [Fact]
    public async Task HandleTaskUncomplete_Exception_SetsErrorMessage()
    {
        TaskServiceMock.Setup(x => x.UncompleteTaskAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("uncomplete error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskUncomplete(Guid.NewGuid());

        cut.Instance.ErrorMessage.Should().Contain("uncomplete error");
    }
}

file sealed class TestIndexJsRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default) => default;
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => default;
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
}

file sealed class ThrowingTestIndexJsRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default) => throw new Exception("js error");
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new Exception("js error");
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new Exception("js error");
}
