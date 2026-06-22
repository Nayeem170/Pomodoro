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

        cut.Find(".tog").Click();

        Assert.False(settings.AutoStartSession);
    }

    [Fact]
    public void ToggleAutoStartSession_WhenFalse_TurnsTrue()
    {
        var settings = new TimerSettings { AutoStartSession = false };
        var cut = RenderComponent<AutomationSettings>(parameters => parameters.Add(p => p.Settings, settings));

        cut.Find(".tog").Click();

        Assert.True(settings.AutoStartSession);
    }
}
