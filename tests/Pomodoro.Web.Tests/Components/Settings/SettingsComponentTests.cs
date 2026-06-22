using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Settings;

[Trait("Category", "Component")]
public class TimerDurationSettingsTests : TestContext
{
    public TimerDurationSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_BindsPomodoroMinutes()
    {
        var settings = new TimerSettings { PomodoroMinutes = 30 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var vals = cut.FindAll(".step-input");
        vals[0].GetAttribute("value").Should().Be("30");
    }

    [Fact]
    public void Render_BindsShortBreakMinutes()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 7 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var vals = cut.FindAll(".step-input");
        vals[1].GetAttribute("value").Should().Be("7");
    }

    [Fact]
    public void Render_BindsLongBreakMinutes()
    {
        var settings = new TimerSettings { LongBreakMinutes = 20 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var vals = cut.FindAll(".step-input");
        vals[2].GetAttribute("value").Should().Be("20");
    }

    [Fact]
    public void IncrementPomodoro_UpdatesValue()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => { }));

        var buttons = cut.FindAll(".step-btn");
        buttons[1].Click();

        settings.PomodoroMinutes.Should().Be(26);
    }

    [Fact]
    public void DecrementPomodoro_UpdatesValue()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var cut = RenderComponent<TimerDurationSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => { }));

        var buttons = cut.FindAll(".step-btn");
        buttons[0].Click();

        settings.PomodoroMinutes.Should().Be(24);
    }

    [Fact]
    public void DecrementPomodoro_DisabledAtMin()
    {
        var settings = new TimerSettings { PomodoroMinutes = 1 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var buttons = cut.FindAll(".step-btn");
        buttons[0].HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void IncrementPomodoro_DisabledAtMax()
    {
        var settings = new TimerSettings { PomodoroMinutes = 120 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var buttons = cut.FindAll(".step-btn");
        buttons[1].HasAttribute("disabled").Should().BeTrue();
    }


}

[Trait("Category", "Component")]
public class SoundNotificationSettingsTests : TestContext
{
    public SoundNotificationSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_BindsSoundEnabled()
    {
        var settings = new TimerSettings { SoundEnabled = true };
        var cut = RenderComponent<SoundNotificationSettings>(p => p.Add(x => x.Settings, settings));

        cut.Find(".tog.on").Should().NotBeNull();
    }

    [Fact]
    public void Render_BindsNotificationsEnabled()
    {
        var settings = new TimerSettings { NotificationsEnabled = true, SoundEnabled = false };
        var cut = RenderComponent<SoundNotificationSettings>(p => p.Add(x => x.Settings, settings));

        cut.FindAll(".tog.on").Should().HaveCount(1);
    }

    [Fact]
    public void ToggleSound_InvokesOnChanged()
    {
        var settings = new TimerSettings { SoundEnabled = true };
        var callbackInvoked = false;
        var cut = RenderComponent<SoundNotificationSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var toggles = cut.FindAll(".tog");
        toggles[0].Click();

        callbackInvoked.Should().BeTrue();
        settings.SoundEnabled.Should().BeFalse();
    }

    [Fact]
    public void ToggleNotifications_InvokesOnChanged()
    {
        var settings = new TimerSettings { NotificationsEnabled = true };
        var callbackInvoked = false;
        var cut = RenderComponent<SoundNotificationSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var toggles = cut.FindAll(".tog");
        toggles[1].Click();

        callbackInvoked.Should().BeTrue();
        settings.NotificationsEnabled.Should().BeFalse();
    }
}

[Trait("Category", "Component")]
public class AutomationSettingsTests : TestContext
{
    public AutomationSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_BindsAutoStartSession()
    {
        var settings = new TimerSettings { AutoStartSession = true };
        var cut = RenderComponent<AutomationSettings>(p => p.Add(x => x.Settings, settings));

        cut.FindAll(".tog.on").Should().HaveCount(1);
    }

    [Fact]
    public void ToggleAutoStart_InvokesOnChanged()
    {
        var settings = new TimerSettings { AutoStartSession = true };
        var callbackInvoked = false;
        var cut = RenderComponent<AutomationSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        cut.Find(".tog").Click();

        callbackInvoked.Should().BeTrue();
        settings.AutoStartSession.Should().BeFalse();
    }

    [Fact]
    public void Render_ShowsAutoStartSessionToggle()
    {
        var settings = new TimerSettings();
        var cut = RenderComponent<AutomationSettings>(p => p.Add(x => x.Settings, settings));

        cut.FindAll(".tog").Should().HaveCount(1);
    }
}

[Trait("Category", "Component")]
public class KeyboardShortcutsSectionTests : TestContext
{
    public KeyboardShortcutsSectionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_ShowsAllShortcuts()
    {
        var cut = RenderComponent<KeyboardShortcutsSection>();

        cut.FindAll(".kr").Should().HaveCount(6);
        cut.FindAll(".kbd").Should().HaveCount(6);
    }

    [Fact]
    public void Render_ShowsSpaceShortcut()
    {
        var cut = RenderComponent<KeyboardShortcutsSection>();

        cut.Find(".kbd").TextContent.Should().Be("Space");
    }
}

[Trait("Category", "Component")]
public class DataManagementSettingsTests : TestContext
{
    public DataManagementSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_ExportButton_DisabledWhenExporting()
    {
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.IsExporting, true));

        cut.Find(".sec-btn").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Render_ImportResult_ShowsWhenNotNull()
    {
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.ImportResult, "Import succeeded!"));

        cut.Find(".import-result").TextContent.Should().Be("Import succeeded!");
    }

    [Fact]
    public void Render_ImportResult_HiddenWhenNull()
    {
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.ImportResult, (string?)null));

        cut.FindAll(".import-result").Should().BeEmpty();
    }

    [Fact]
    public void Render_ClearButton_DisabledWhenClearing()
    {
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.IsClearing, true));

        cut.Find(".danger-btn").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ClickExportButton_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.OnExportJson, () => invoked = true));

        cut.Find(".sec-btn").Click();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void ClickClearButton_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.OnConfirmClearData, () => invoked = true));

        cut.Find(".danger-btn").Click();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Render_ShowsExportSubtitle()
    {
        var cut = RenderComponent<DataManagementSettings>();

        cut.FindAll(".sr-sub").Should().Contain(s => s.TextContent.Contains("Download as JSON"));
    }

    [Fact]
    public void Render_ShowsClearSubtitle()
    {
        var cut = RenderComponent<DataManagementSettings>();

        cut.FindAll(".sr-sub").Should().Contain(s => s.TextContent.Contains("Cannot be undone"));
    }
}

[Trait("Category", "Component")]
public class ClearConfirmationModalTests : TestContext
{
    public ClearConfirmationModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(Mock.Of<ICloudSyncService>());
    }

    [Fact]
    public void Render_Visible_ShowsModal()
    {
        var cut = RenderComponent<ClearConfirmationModal>(p => p
            .Add(x => x.IsVisible, true));

        cut.Find(".confirmation-modal").Should().NotBeNull();
    }

    [Fact]
    public void Render_Hidden_DoesNotShowModal()
    {
        var cut = RenderComponent<ClearConfirmationModal>(p => p
            .Add(x => x.IsVisible, false));

        cut.FindAll(".confirmation-modal").Should().BeEmpty();
    }

    [Fact]
    public void ClickConfirm_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<ClearConfirmationModal>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.OnConfirm, () => invoked = true));

        cut.Find(".btn-confirm-danger").Click();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void ClickCancel_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<ClearConfirmationModal>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.OnCancel, () => invoked = true));

        cut.Find(".btn-cancel-action").Click();
        invoked.Should().BeTrue();
    }
}
