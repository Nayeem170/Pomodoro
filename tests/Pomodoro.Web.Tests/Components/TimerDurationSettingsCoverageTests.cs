using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Xunit;
using System.Reflection;

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

        var field = typeof(TimerDurationSettings).GetField("pomodoroInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "30");

        var method = typeof(TimerDurationSettings).GetMethod("ValidatePomodoro", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(30, settings.PomodoroMinutes);
    }

    [Fact]
    public void ValidatePomodoro_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("pomodoroInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "abc");

        var method = typeof(TimerDurationSettings).GetMethod("ValidatePomodoro", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(25, settings.PomodoroMinutes);
        Assert.Equal("25", field.GetValue(cut.Instance));
    }

    [Fact]
    public void ValidateShortBreak_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("shortBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "10");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateShortBreak", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(10, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ValidateShortBreak_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("shortBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "xyz");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateShortBreak", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(5, settings.ShortBreakMinutes);
        Assert.Equal("5", field.GetValue(cut.Instance));
    }

    [Fact]
    public void ValidateLongBreak_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("longBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "20");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateLongBreak", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(20, settings.LongBreakMinutes);
    }

    [Fact]
    public void ValidateLongBreak_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("longBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "bad");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateLongBreak", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(15, settings.LongBreakMinutes);
        Assert.Equal("15", field.GetValue(cut.Instance));
    }

    [Fact]
    public void ValidateDailyGoal_ValidNumber_UpdatesSettings()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("dailyGoalInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "12");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateDailyGoal", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(12, settings.DailyGoal);
    }

    [Fact]
    public void ValidateDailyGoal_InvalidNumber_RevertsToSettings()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("dailyGoalInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "notanumber");

        var method = typeof(TimerDurationSettings).GetMethod("ValidateDailyGoal", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        Assert.Equal(8, settings.DailyGoal);
        Assert.Equal("8", field.GetValue(cut.Instance));
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

    [Fact]
    public void HandlePomodoroKey_EnterKey_Validate()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("pomodoroInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "abc");

        var method = typeof(TimerDurationSettings).GetMethod("HandlePomodoroKey", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, new object[] { new KeyboardEventArgs { Key = "Enter" } });

        var value = (string)field.GetValue(cut.Instance)!;
        Assert.Equal("25", value);
    }

    [Fact]
    public void HandleShortBreakKey_EnterKey_Validate()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 5 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("shortBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "xyz");

        var method = typeof(TimerDurationSettings).GetMethod("HandleShortBreakKey", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, new object[] { new KeyboardEventArgs { Key = "Enter" } });

        var value = (string)field.GetValue(cut.Instance)!;
        Assert.Equal("5", value);
    }

    [Fact]
    public void HandleLongBreakKey_EnterKey_Validate()
    {
        var settings = new TimerSettings { LongBreakMinutes = 15 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("longBreakInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "bad");

        var method = typeof(TimerDurationSettings).GetMethod("HandleLongBreakKey", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, new object[] { new KeyboardEventArgs { Key = "Enter" } });

        var value = (string)field.GetValue(cut.Instance)!;
        Assert.Equal("15", value);
    }

    [Fact]
    public void HandleDailyGoalKey_EnterKey_Validate()
    {
        var settings = new TimerSettings { DailyGoal = 8 };
        var cut = RenderComponent<TimerDurationSettings>(parameters => parameters
            .Add(p => p.Settings, settings));

        var field = typeof(TimerDurationSettings).GetField("dailyGoalInput", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(cut.Instance, "nope");

        var method = typeof(TimerDurationSettings).GetMethod("HandleDailyGoalKey", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, new object[] { new KeyboardEventArgs { Key = "Enter" } });

        var value = (string)field.GetValue(cut.Instance)!;
        Assert.Equal("8", value);
    }
}
