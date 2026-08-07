using Bunit;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class AutomationSettingsTests : TestContext
{
    [Fact]
    public void ToggleAutoStartSession_TogglesValue()
    {
        var settings = new TimerSettings { AutoStartSession = true };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".tog")[0].Click();

        Assert.False(settings.AutoStartSession);
    }

    [Fact]
    public void ToggleAutoStartSession_WhenFalse_TurnsTrue()
    {
        var settings = new TimerSettings { AutoStartSession = false };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".tog")[0].Click();

        Assert.True(settings.AutoStartSession);
    }

    [Fact]
    public void ToggleRecordPartialSessions_TogglesValue()
    {
        var settings = new TimerSettings { RecordPartialSessions = true };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".tog")[1].Click();

        Assert.False(settings.RecordPartialSessions);
    }

    [Fact]
    public void ToggleRecordPartialSessions_WhenFalse_TurnsTrue()
    {
        var settings = new TimerSettings { RecordPartialSessions = false };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.FindAll(".tog")[1].Click();

        Assert.True(settings.RecordPartialSessions);
    }
}
