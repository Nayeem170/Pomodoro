using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
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

        var input = cut.Find("input[type='number']");
        input.GetAttribute("value").Should().Be("30");
    }

    [Fact]
    public void Render_BindsShortBreakMinutes()
    {
        var settings = new TimerSettings { ShortBreakMinutes = 7 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var inputs = cut.FindAll("input[type='number']");
        inputs[1].GetAttribute("value").Should().Be("7");
    }

    [Fact]
    public void Render_BindsLongBreakMinutes()
    {
        var settings = new TimerSettings { LongBreakMinutes = 20 };
        var cut = RenderComponent<TimerDurationSettings>(p => p.Add(x => x.Settings, settings));

        var inputs = cut.FindAll("input[type='number']");
        inputs[2].GetAttribute("value").Should().Be("20");
    }
}

[Trait("Category", "Component")]
public class PreferenceSettingsTests : TestContext
{
    public PreferenceSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_BindsSoundEnabled()
    {
        var settings = new TimerSettings { SoundEnabled = false };
        var cut = RenderComponent<PreferenceSettings>(p => p.Add(x => x.Settings, settings));

        cut.Find("#soundToggle").Should().NotBeNull();
    }

    [Fact]
    public void Render_BindsNotificationsEnabled()
    {
        var settings = new TimerSettings { NotificationsEnabled = true };
        var cut = RenderComponent<PreferenceSettings>(p => p.Add(x => x.Settings, settings));

        cut.Find("#notifToggle").Should().NotBeNull();
    }

    [Fact]
    public void ToggleSound_InvokesOnChanged()
    {
        var settings = new TimerSettings { SoundEnabled = true };
        var callbackInvoked = false;
        var cut = RenderComponent<PreferenceSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var checkbox = cut.Find("#soundToggle");
        checkbox.Change(false);

        callbackInvoked.Should().BeTrue();
        settings.SoundEnabled.Should().BeFalse();
    }

    [Fact]
    public void ToggleNotifications_InvokesOnChanged()
    {
        var settings = new TimerSettings { NotificationsEnabled = true };
        var callbackInvoked = false;
        var cut = RenderComponent<PreferenceSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var checkbox = cut.Find("#notifToggle");
        checkbox.Change(false);

        callbackInvoked.Should().BeTrue();
        settings.NotificationsEnabled.Should().BeFalse();
    }
}

[Trait("Category", "Component")]
public class AutoStartSettingsTests : TestContext
{
    public AutoStartSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_WithAutoStartEnabled_ShowsDelayInput()
    {
        var settings = new TimerSettings { AutoStartEnabled = true, AutoStartDelaySeconds = 5 };
        var cut = RenderComponent<AutoStartSettings>(p => p.Add(x => x.Settings, settings));

        cut.Find("input[type='number']").GetAttribute("value").Should().Be("5");
    }

    [Fact]
    public void Render_WithAutoStartDisabled_HidesDelayInput()
    {
        var settings = new TimerSettings { AutoStartEnabled = false };
        var cut = RenderComponent<AutoStartSettings>(p => p.Add(x => x.Settings, settings));

        cut.FindAll("input[type='number']").Should().BeEmpty();
    }

    [Fact]
    public void ToggleAutoStart_InvokesOnChanged()
    {
        var settings = new TimerSettings { AutoStartEnabled = true };
        var callbackInvoked = false;
        var cut = RenderComponent<AutoStartSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var checkbox = cut.Find("#autoStartEnabled");
        checkbox.Change(false);

        callbackInvoked.Should().BeTrue();
        settings.AutoStartEnabled.Should().BeFalse();
    }

    [Fact]
    public void ChangeDelayInput_InvokesOnChanged()
    {
        var settings = new TimerSettings { AutoStartEnabled = true, AutoStartDelaySeconds = 5 };
        var callbackInvoked = false;
        var cut = RenderComponent<AutoStartSettings>(p => p
            .Add(x => x.Settings, settings)
            .Add(x => x.OnChanged, () => callbackInvoked = true));

        var input = cut.Find("input[type='number']");
        input.Change("10");

        callbackInvoked.Should().BeTrue();
        settings.AutoStartDelaySeconds.Should().Be(10);
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

        cut.Find(".btn-export").HasAttribute("disabled").Should().BeTrue();
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

        cut.Find(".btn-clear").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ClickExportButton_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.OnExportJson, () => invoked = true));

        cut.Find(".btn-export").Click();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void ClickClearButton_InvokesCallback()
    {
        var invoked = false;
        var cut = RenderComponent<DataManagementSettings>(p => p
            .Add(x => x.OnConfirmClearData, () => invoked = true));

        cut.Find(".btn-clear").Click();
        invoked.Should().BeTrue();
    }
}

[Trait("Category", "Component")]
public class ClearConfirmationModalTests : TestContext
{
    public ClearConfirmationModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
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

        cut.Find(".btn-cancel").Click();
        invoked.Should().BeTrue();
    }
}

