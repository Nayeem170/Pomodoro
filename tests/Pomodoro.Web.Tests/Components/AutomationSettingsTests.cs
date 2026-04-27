using Bunit;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class AutomationSettingsTests : TestContext
{
    [Fact]
    public void ToggleAutoStartPomodoros_TogglesValue()
    {
        var settings = new TimerSettings { AutoStartPomodoros = true };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.Find(".tog").Click();

        Assert.False(settings.AutoStartPomodoros);
    }

    [Fact]
    public void ToggleAutoStartBreaks_TogglesValue()
    {
        var settings = new TimerSettings { AutoStartBreaks = true };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".tog")[1].Click();

        Assert.False(settings.AutoStartBreaks);
    }

    [Fact]
    public void ToggleAutoStartPomodoros_WhenFalse_TurnsTrue()
    {
        var settings = new TimerSettings { AutoStartPomodoros = false };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.Find(".tog").Click();

        Assert.True(settings.AutoStartPomodoros);
    }
}
