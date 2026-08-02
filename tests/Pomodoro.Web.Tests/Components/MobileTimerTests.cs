using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class MobileTimerTests : TestContext
{
    private readonly Mock<ITimerService> _mockTimerService = new();

    public MobileTimerTests()
    {
        _mockTimerService.SetupGet(x => x.RemainingTime).Returns(TimeSpan.FromMinutes(25));
        _mockTimerService.SetupGet(x => x.Settings).Returns(new TimerSettings());
        Services.AddSingleton(_mockTimerService.Object);
    }

    [Fact]
    public void Collapsed_ShowsClockAndExpandButton()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.SessionType, SessionType.Pomodoro)
            .Add(x => x.CanStart, true));

        cut.FindAll(".mobile-timer.collapsed").Should().HaveCount(1);
        cut.FindAll(".mt-expand-btn").Should().HaveCount(1);
        cut.Markup.Should().Contain("25:00");
    }

    [Fact]
    public void Expanded_ShowsLargeRingAndCollapseButton()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, true)
            .Add(x => x.SessionType, SessionType.Pomodoro)
            .Add(x => x.CanStart, true));

        cut.FindAll(".mobile-timer.expanded").Should().HaveCount(1);
        cut.FindAll(".mt-collapse-btn").Should().HaveCount(1);
        cut.FindAll(".mt-ring-large").Should().HaveCount(1);
    }

    [Fact]
    public void ExpandButton_InvokesOnToggleExpand()
    {
        var toggled = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.OnToggleExpand, EventCallback.Factory.Create(this, () => toggled = true)));

        cut.Find(".mt-expand-btn").Click();

        toggled.Should().BeTrue();
    }

    [Fact]
    public void CollapseButton_InvokesOnToggleExpand()
    {
        var toggled = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, true)
            .Add(x => x.OnToggleExpand, EventCallback.Factory.Create(this, () => toggled = true)));

        cut.Find(".mt-collapse-btn").Click();

        toggled.Should().BeTrue();
    }

    [Fact]
    public void Collapsed_StartButton_InvokesOnStart()
    {
        var started = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.CanStart, true)
            .Add(x => x.OnStart, EventCallback.Factory.Create(this, () => started = true)));

        cut.Find(".mt-btn-primary").Click();

        started.Should().BeTrue();
    }

    [Fact]
    public void Running_ShowsResetButton()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.IsRunning, true));

        cut.FindAll("button[aria-label=\"Reset timer\"]").Should().HaveCount(1);
    }

    [Fact]
    public void Idle_NoResetButton()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.IsRunning, false)
            .Add(x => x.IsStarted, false));

        cut.FindAll("button[aria-label=\"Reset timer\"]").Should().HaveCount(0);
    }

    [Fact]
    public void Running_PrimaryButton_InvokesOnPause()
    {
        var paused = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.IsRunning, true)
            .Add(x => x.OnPause, EventCallback.Factory.Create(this, () => paused = true)));

        cut.Find(".mt-btn-primary").Click();

        paused.Should().BeTrue();
    }

    [Fact]
    public void Paused_PrimaryButton_InvokesOnResume()
    {
        var resumed = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.IsStarted, true)
            .Add(x => x.IsPaused, true)
            .Add(x => x.OnResume, EventCallback.Factory.Create(this, () => resumed = true)));

        cut.Find(".mt-btn-primary").Click();

        resumed.Should().BeTrue();
    }

    [Fact]
    public void ResetButton_InvokesOnReset()
    {
        var reset = false;
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.IsRunning, true)
            .Add(x => x.OnReset, EventCallback.Factory.Create(this, () => reset = true)));

        cut.Find("button[aria-label=\"Reset timer\"]").Click();

        reset.Should().BeTrue();
    }

    [Fact]
    public void Expanded_ShowsTaskContext_WhenTaskNameProvided()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, true)
            .Add(x => x.CurrentTaskName, "Review PR"));

        cut.FindAll(".mt-task-ctx").Should().HaveCount(1);
        cut.Markup.Should().Contain("Review PR");
    }

    [Fact]
    public void Collapsed_ShowsTaskName()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.CurrentTaskName, "Review PR"));

        cut.Markup.Should().Contain("Review PR");
    }

    [Fact]
    public void Collapsed_NoTask_ShowsHint()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.CanStart, false)
            .Add(x => x.CurrentTaskName, (string?)null));

        cut.Markup.Should().Contain("Select a task");
    }

    [Fact]
    public void ShortBreak_AppliesShortBreakClass()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.SessionType, SessionType.ShortBreak));

        cut.Find(".mobile-timer").ClassList.Should().Contain("short-break");
    }

    [Fact]
    public void StartDisabled_WhenCannotStart()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.CanStart, false)
            .Add(x => x.IsStarted, false)
            .Add(x => x.IsRunning, false));

        cut.Find(".mt-btn-primary").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Expanded_ShowsModeLabel()
    {
        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, true)
            .Add(x => x.SessionType, SessionType.Pomodoro));

        cut.Markup.Should().Contain("POMODORO");
    }

    [Fact]
    public void Collapsed_ShowsProgressPercent()
    {
        _mockTimerService.SetupGet(x => x.RemainingTime).Returns(TimeSpan.FromMinutes(15));
        var settings = new TimerSettings();
        _mockTimerService.SetupGet(x => x.Settings).Returns(settings);

        var cut = RenderComponent<MobileTimer>(p => p
            .Add(x => x.IsExpanded, false)
            .Add(x => x.SessionType, SessionType.Pomodoro));

        cut.Markup.Should().Contain("%");
    }
}
