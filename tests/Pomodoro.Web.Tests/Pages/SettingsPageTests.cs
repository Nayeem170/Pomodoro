using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Xunit;
using Moq;
using Microsoft.JSInterop;

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
        // Set up TimerService with default settings
        var defaultSettings = new TimerSettings();
        TimerServiceMock
            .SetupGet(x => x.Settings)
            .Returns(defaultSettings);

        // Set up UpdateSettingsAsync to return completed task
        TimerServiceMock
            .Setup(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public void SettingsPage_RendersWithoutErrors()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SettingsPage_HasSettingsSections()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert - Settings page should have sections for Timer Durations, Preferences, Auto-Start, and Data Management
        var headers = cut.FindAll("h2");
        headers.Should().NotBeEmpty();
        headers.Count.Should().BeGreaterThanOrEqualTo(3); // At least 3 sections
    }

    [Fact]
    public void SettingsPage_HasTimerDurationInputs()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert - Settings page should have number inputs for timer durations
        var inputs = cut.FindAll(".setting-input[type='number']");
        inputs.Should().NotBeEmpty();
        inputs.Count.Should().BeGreaterThanOrEqualTo(3); // Pomodoro, Short Break, Long Break
    }

    [Fact]
    public void SettingsPage_HasToggleSwitches()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert - Settings page should have toggle switches for preferences
        var toggles = cut.FindAll(".toggle-input[type='checkbox']");
        toggles.Should().NotBeEmpty();
        toggles.Count.Should().BeGreaterThanOrEqualTo(2); // Sound, Notifications, Auto-start
    }

    [Fact]
    public void SettingsPage_HasDataManagementButtons()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert - Settings page should have buttons for data management
        var exportButton = cut.Find(".btn-export");
        var importLabel = cut.Find(".btn-import");
        var clearButton = cut.Find(".btn-clear");

        exportButton.Should().NotBeNull();
        importLabel.Should().NotBeNull();
        clearButton.Should().NotBeNull();
    }
}

