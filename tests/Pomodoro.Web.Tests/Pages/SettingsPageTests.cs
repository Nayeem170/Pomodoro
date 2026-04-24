using Bunit;
using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for the Settings page component.
/// These tests verify rendering and basic interactions of the Settings page.
/// </summary>
[Trait("Category", "Page")]
public class SettingsPageTests : TestHelper
{
    public SettingsPageTests()
    {
        var defaultSettings = new TimerSettings();
        TimerServiceMock
            .SetupGet(x => x.Settings)
            .Returns(defaultSettings);

        TimerServiceMock
            .Setup(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public void SettingsPage_RendersWithoutErrors()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SettingsPage_HasSettingsSections()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        var headers = cut.FindAll(".ss-hdr");
        headers.Should().NotBeEmpty();
        headers.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void SettingsPage_HasTimerDurationSteppers()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        var steppers = cut.FindAll(".stepper");
        steppers.Should().NotBeEmpty();
        steppers.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void SettingsPage_HasToggleSwitches()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        var toggles = cut.FindAll(".tog");
        toggles.Should().NotBeEmpty();
        toggles.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void SettingsPage_HasDataManagementButtons()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        var clearButton = cut.Find(".danger-btn");

        clearButton.Should().NotBeNull();
    }
}
