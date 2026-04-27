using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TimerDurationSettingsTests : TestContext
{
    [Fact]
    public void OnParametersSet_InitializesInputs()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25, ShortBreakMinutes = 5, LongBreakMinutes = 15, DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        Assert.Equal("25", cut.Find(".step-input").GetAttribute("value"));
    }

    [Fact]
    public void IncrementPomodoro_IncrementsValue()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.Find(".step-btn[aria-label=\"Increase\"]").Click();

        Assert.Equal(26, settings.PomodoroMinutes);
    }

    [Fact]
    public void DecrementPomodoro_DecrementsValue()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.Find(".step-btn[aria-label=\"Decrease\"]").Click();

        Assert.Equal(24, settings.PomodoroMinutes);
    }

    [Fact]
    public void IncrementShortBreak_IncrementsValue()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Increase\"]")[1].Click();

        Assert.Equal(6, settings.ShortBreakMinutes);
    }

    [Fact]
    public void DecrementShortBreak_DecrementsValue()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Decrease\"]")[1].Click();

        Assert.Equal(4, settings.ShortBreakMinutes);
    }

    [Fact]
    public void IncrementLongBreak_IncrementsValue()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Increase\"]")[2].Click();

        Assert.Equal(16, settings.LongBreakMinutes);
    }

    [Fact]
    public void DecrementLongBreak_DecrementsValue()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Decrease\"]")[2].Click();

        Assert.Equal(14, settings.LongBreakMinutes);
    }

    [Fact]
    public void IncrementDailyGoal_IncrementsValue()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Increase\"]")[3].Click();

        Assert.Equal(9, settings.DailyGoal);
    }

    [Fact]
    public void DecrementDailyGoal_DecrementsValue()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Decrease\"]")[3].Click();

        Assert.Equal(7, settings.DailyGoal);
    }

    [Fact]
    public void IsPomodoroMin_DisabledAtOne()
    {
        var settings = new TimerSettings { PomodoroMinutes = 1 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        Assert.True(cut.Find("button[aria-label=Decrease]").HasAttribute("disabled"));
    }

    [Fact]
    public void IsPomodoroMax_DisabledAt120()
    {
        var settings = new TimerSettings { PomodoroMinutes = 120 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        Assert.True(cut.Find("button[aria-label=Increase]").HasAttribute("disabled"));
    }
}
