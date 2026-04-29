using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class IndexPageCoverageTests : TestHelper
{
    public IndexPageCoverageTests()
    {
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
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
        TodayStatsServiceMock.Setup(x => x.GetTodayTotalFocusMinutes()).Returns(120);
        TodayStatsServiceMock.Setup(x => x.GetTodayPomodoroCount()).Returns(4);
        TodayStatsServiceMock.Setup(x => x.GetTodayTasksWorkedOn()).Returns(2);
        KeyboardShortcutServiceMock.Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()));
        JSRuntimeMock.Setup(x => x.InvokeAsync<string?>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync((string?)null);
    }

    [Fact]
    public void IndexPage_RendersTimerCardWithThemeClass()
    {
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        cut.Markup.Should().Contain("timer-card");
        cut.Markup.Should().Contain("running");
    }

    [Fact]
    public void IndexPage_RendersPipButton()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        cut.Markup.Should().Contain("pip-btn");
    }

    [Fact]
    public void IndexPage_RendersModeTabs()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        cut.Markup.Should().Contain("mode-tabs");
        cut.Markup.Should().Contain("Pomodoro");
        cut.Markup.Should().Contain("Short break");
        cut.Markup.Should().Contain("Long break");
    }

    [Fact]
    public void IndexPage_RendersTasksSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        cut.Markup.Should().Contain("tasks-section");
    }
}
