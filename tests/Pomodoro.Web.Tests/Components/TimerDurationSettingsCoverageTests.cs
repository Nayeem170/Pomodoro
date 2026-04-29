using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TimerDurationSettingsCoverageTests : TestContext
{
    [Fact]
    public void ValidatePomodoro_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[0].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(25, settings.PomodoroMinutes);
    }

    [Fact]
    public void ValidatePomodoro_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[0].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(25, settings.PomodoroMinutes);
    }

    [Fact]
    public void ValidateShortBreak_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[1].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(5, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ValidateShortBreak_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[1].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(5, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ValidateLongBreak_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[2].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(15, settings.LongBreakMinutes);
    }

    [Fact]
    public void ValidateLongBreak_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[2].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(15, settings.LongBreakMinutes);
    }

    [Fact]
    public void ValidateDailyGoal_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[3].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(8, settings.DailyGoal);
    }

    [Fact]
    public void ValidateDailyGoal_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-input")[3].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(8, settings.DailyGoal);
    }

    [Fact]
    public void OnParametersSet_SyncsAllInputs()
    {
        var settings = new TimerSettings { PomodoroMinutes = 30, ShortBreakMinutes = 10, LongBreakMinutes = 20, DailyGoal = 12 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var inputs = cut.FindAll(".step-input");
        Assert.Equal("30", inputs[0].GetAttribute("value"));
        Assert.Equal("10", inputs[1].GetAttribute("value"));
        Assert.Equal("20", inputs[2].GetAttribute("value"));
        Assert.Equal("12", inputs[3].GetAttribute("value"));
    }

    [Fact]
    public void IsShortBreakMin_DisabledAtOne()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 1 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Decrease]")[1].HasAttribute("disabled"));
    }

    [Fact]
    public void IsShortBreakMax_DisabledAt60()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 60 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Increase]")[1].HasAttribute("disabled"));
    }

    [Fact]
    public void IsLongBreakMin_DisabledAtOne()
    {
        var settings = new TimerSettings { LongBreakMinutes = 1 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Decrease]")[2].HasAttribute("disabled"));
    }

    [Fact]
    public void IsLongBreakMax_DisabledAt60()
    {
        var settings = new TimerSettings { LongBreakMinutes = 60 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Increase]")[2].HasAttribute("disabled"));
    }

    [Fact]
    public void IsDailyGoalMin_DisabledAtOne()
    {
        var settings = new TimerSettings { DailyGoal = 1 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Decrease]")[3].HasAttribute("disabled"));
    }

    [Fact]
    public void IsDailyGoalMax_DisabledAt20()
    {
        var settings = new TimerSettings { DailyGoal = 20 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        Assert.True(cut.FindAll("button[aria-label=Increase]")[3].HasAttribute("disabled"));
    }

    [Fact]
    public void OnChanged_FiredOnIncrement()
    {
        var changed = false;
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.OnChanged, () => { changed = true; }));

        cut.Find("button[aria-label=Increase]").Click();

        Assert.True(changed);
    }

    [Fact]
    public void OnChanged_FiredOnDecrement()
    {
        var changed = false;
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.OnChanged, () => { changed = true; }));

        cut.Find("button[aria-label=Decrease]").Click();

        Assert.True(changed);
    }

    [Fact]
    public void OnChanged_FiredOnValidate()
    {
        var changed = false;
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.OnChanged, () => { changed = true; }));

        cut.FindAll(".step-input")[0].TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.True(changed);
    }

    [Fact]
    public void IncrementLongBreak_IncrementsValue()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Increase\"]")[2].Click();

        Assert.Equal(16, settings.LongBreakMinutes);
    }

    [Fact]
    public void DecrementLongBreak_DecrementsValue()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Decrease\"]")[2].Click();

        Assert.Equal(14, settings.LongBreakMinutes);
    }

    [Fact]
    public void IncrementDailyGoal_IncrementsValue()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Increase\"]")[3].Click();

        Assert.Equal(9, settings.DailyGoal);
    }

    [Fact]
    public void DecrementDailyGoal_DecrementsValue()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        cut.FindAll(".step-btn[aria-label=\"Decrease\"]")[3].Click();

        Assert.Equal(7, settings.DailyGoal);
    }
}
