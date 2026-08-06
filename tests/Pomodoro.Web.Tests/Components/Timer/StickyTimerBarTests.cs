using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.Timer;

[Trait("Category", "Component")]
public class StickyTimerBarTests : TestHelper
{
    [Fact]
    public void Renders_ThreeSessionTabs()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        cut.FindAll(".stb-tab").Count.Should().Be(3);
    }

    [Fact]
    public void MarksPomodoroTabActive_WhenSessionIsPomodoro()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        cut.Find(".stb-tab.act").TextContent.Should().Contain("Pomodoro");
    }

    [Fact]
    public void MarksShortBreakTabActive_WhenSessionIsShortBreak()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.ShortBreak));

        cut.Find(".stb-tab.act.sb").TextContent.Should().Contain("Short break");
    }

    [Fact]
    public void MarksLongBreakTabActive_WhenSessionIsLongBreak()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.LongBreak));

        cut.Find(".stb-tab.act.lb").TextContent.Should().Contain("Long break");
    }

    [Fact]
    public void ShowsSelectTaskText_WhenPomodoroNotStartedAndNoTask()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.IsStarted, false)
            .Add(p => p.IsRunning, false)
            .Add(p => p.CurrentTaskName, null));

        cut.Find(".stb-task").TextContent.Should().Contain("Select a task");
    }

    [Fact]
    public void ShowsTaskName_WhenPomodoroHasTask()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.CurrentTaskName, "My Task"));

        cut.Find(".stb-task").TextContent.Should().Contain("My Task");
    }

    [Fact]
    public void ShowsModePrefix_WhenNotPomodoro()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.LongBreak));

        cut.Find(".stb-task").TextContent.Should().Contain("Long break");
    }

    [Fact]
    public async Task InvokesOnSessionChange_WhenTabClicked()
    {
        var invoked = false;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnSessionChange, EventCallback.Factory.Create<SessionType>(this, st => invoked = true)));

        await cut.InvokeAsync(() => cut.FindAll(".stb-tab")[1].Click());

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task InvokesOnStart_WhenPrimaryClickedAndNotStarted()
    {
        var invoked = false;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.IsStarted, false)
            .Add(p => p.IsRunning, false)
            .Add(p => p.OnStart, EventCallback.Factory.Create(this, () => invoked = true)));

        await cut.InvokeAsync(() => cut.Find(".stb-pause").Click());

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task InvokesOnPause_WhenPrimaryClickedAndRunning()
    {
        var invoked = false;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.IsStarted, true)
            .Add(p => p.IsRunning, true)
            .Add(p => p.OnPause, EventCallback.Factory.Create(this, () => invoked = true)));

        await cut.InvokeAsync(() => cut.Find(".stb-pause").Click());

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task InvokesOnResume_WhenPrimaryClickedAndPaused()
    {
        var invoked = false;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.IsStarted, true)
            .Add(p => p.IsRunning, false)
            .Add(p => p.OnResume, EventCallback.Factory.Create(this, () => invoked = true)));

        await cut.InvokeAsync(() => cut.Find(".stb-pause").Click());

        invoked.Should().BeTrue();
    }

    [Fact]
    public void SubscribesToTimerEvents_OnInit()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        TimerEventPublisherMock.VerifyAdd(m => m.OnTick += It.IsAny<Action>(), Times.Once);
        TimerEventPublisherMock.VerifyAdd(m => m.OnTimerStateChanged += It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        var act = () => cut.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnTimerStateChanged_RerendersWithoutError()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        await cut.InvokeAsync(() => TimerEventPublisherMock.Raise(m => m.OnTimerStateChanged += null));
        cut.WaitForState(() => cut.IsDisposed == false);
    }

    [Fact]
    public async Task OnTick_RerendersWithoutError()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        await cut.InvokeAsync(() => TimerEventPublisherMock.Raise(m => m.OnTick += null));
    }

    [Fact]
    public async Task ClickLongBreakTab_InvokesOnSessionChange()
    {
        SessionType? changedTo = null;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnSessionChange, EventCallback.Factory.Create<SessionType>(this, st => changedTo = st)));

        await cut.InvokeAsync(() => cut.FindAll(".stb-tab")[2].Click());

        changedTo.Should().Be(SessionType.LongBreak);
    }

    [Fact]
    public async Task ClickPomodoroTab_InvokesOnSessionChange()
    {
        SessionType? changedTo = null;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.ShortBreak)
            .Add(p => p.OnSessionChange, EventCallback.Factory.Create<SessionType>(this, st => changedTo = st)));

        await cut.InvokeAsync(() => cut.FindAll(".stb-tab")[0].Click());

        changedTo.Should().Be(SessionType.Pomodoro);
    }

    [Fact]
    public async Task ClickShortBreakTab_InvokesOnSessionChange()
    {
        SessionType? changedTo = null;
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnSessionChange, EventCallback.Factory.Create<SessionType>(this, st => changedTo = st)));

        await cut.InvokeAsync(() => cut.FindAll(".stb-tab")[1].Click());

        changedTo.Should().Be(SessionType.ShortBreak);
    }

    [Fact]
    public void GetSessionClass_DefaultCase_ReturnsPomodoroTheme()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, (SessionType)999));

        cut.Markup.Should().Contain("pomodoro");
    }

    [Fact]
    public void GetModePrefix_PomodoroCase_ReturnsFocusViaReflection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Timer.StickyTimerBar>(parameters => parameters
            .Add(p => p.SessionType, SessionType.Pomodoro));

        var method = typeof(Pomodoro.Web.Components.Timer.StickyTimerBar)
            .GetMethod("GetModePrefix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (string)method!.Invoke(cut.Instance, null)!;

        result.Should().Be("Focus");
    }
}
