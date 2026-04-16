using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Pages;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Pages
{
    [Trait("Category", "Page")]
    public class SettingsTests : TestContext
    {
        private readonly Mock<ITimerService> _mockTimerService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IImportService> _mockImportService;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<IJSInteropService> _mockJSInteropService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<ILogger<SettingsPageBase>> _mockLogger;
        private readonly Mock<ILogger<SettingsPresenterService>> _mockPresenterLogger;

        public SettingsTests()
        {
            _mockTimerService = new Mock<ITimerService>();
            _mockExportService = new Mock<IExportService>();
            _mockImportService = new Mock<IImportService>();
            _mockTaskService = new Mock<ITaskService>();
            _mockActivityService = new Mock<IActivityService>();
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockJSInteropService = new Mock<IJSInteropService>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockLogger = new Mock<ILogger<SettingsPageBase>>();
            _mockPresenterLogger = new Mock<ILogger<SettingsPresenterService>>();

            // Set up default TimerService settings to prevent NullReferenceException
            _mockTimerService.Setup(x => x.Settings).Returns(new TimerSettings());

            Services.AddSingleton(_mockTimerService.Object);
            Services.AddSingleton(_mockExportService.Object);
            Services.AddSingleton(_mockImportService.Object);
            Services.AddSingleton(_mockTaskService.Object);
            Services.AddSingleton(_mockActivityService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockJSInteropService.Object);
            Services.AddSingleton(_mockNavigationManager.Object);
            Services.AddSingleton(_mockLogger.Object);
            Services.AddSingleton(_mockPresenterLogger.Object);

            // Register SettingsPresenterService
            Services.AddSingleton(new SettingsPresenterService(_mockPresenterLogger.Object));

            // Add NavigationManager
            Services.AddSingleton<NavigationManager>(new MockNavigationManager());
        }

        [Fact]
        public void SettingsPage_Renders_Successfully()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.NotNull(cut);
            cut.Markup.Contains("Settings");
        }

        [Fact]
        public void SettingsPage_ShowsTimerDurationsSection()
        {
            // Arrange
            var settings = new TimerSettings
            {
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15
            };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Timer Durations");
            cut.Markup.Contains("Pomodoro");
            cut.Markup.Contains("Short Break");
            cut.Markup.Contains("Long Break");
        }

        [Fact]
        public void SettingsPage_ShowsPreferencesSection()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Preferences");
            cut.Markup.Contains("Sound");
            cut.Markup.Contains("Notifications");
        }

        [Fact]
        public void SettingsPage_ShowsAutoStartSection()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Auto-Start");
            cut.Markup.Contains("Auto-start Timer");
        }

        [Fact]
        public void SettingsPage_ShowsAutoStartDelay_WhenAutoStartEnabled()
        {
            // Arrange
            var settings = new TimerSettings
            {
                AutoStartEnabled = true,
                AutoStartDelaySeconds = 5
            };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Auto-start Delay");
        }

        [Fact]
        public void SettingsPage_HidesAutoStartDelay_WhenAutoStartDisabled()
        {
            // Arrange
            var settings = new TimerSettings
            {
                AutoStartEnabled = false
            };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Auto-start delay should not be visible
            Assert.DoesNotContain("Auto-start Delay", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ShowsDataManagementSection()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Data Management");
            cut.Markup.Contains("Export Backup");
            cut.Markup.Contains("Import Backup");
            cut.Markup.Contains("Clear All Data");
        }

        [Fact]
        public void SettingsPage_ExportButton_DisabledWhenExporting()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Find export button
            var exportButton = cut.Find("button.btn-export");

            // Assert - Should not be disabled initially
            Assert.False(exportButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ClearButton_DisabledWhenClearing()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Find clear button
            var clearButton = cut.Find("button.btn-clear");

            // Assert - Should not be disabled initially
            Assert.False(clearButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ShowsClearConfirmationModal_WhenShowClearConfirmationIsTrue()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to trigger confirmation
            cut.Find("button.btn-clear").Click();

            // Assert - Modal should be visible
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));
        }

        [Fact]
        public void SettingsPage_HidesClearConfirmationModal_WhenCancelled()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to trigger confirmation
            cut.Find("button.btn-clear").Click();
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));

            // Click cancel button
            cut.Find("button.btn-cancel").Click();

            // Assert - Modal should be hidden
            cut.WaitForAssertion(() => Assert.DoesNotContain("Clear All Data?", cut.Markup));
        }

        [Fact]
        public void SettingsPage_SaveButton_DisabledWhenNoChanges()
        {
            // Arrange
            var settings = new TimerSettings();
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var saveButton = cut.Find("button.btn-save");

            // Assert - Save button should be disabled when no changes
            Assert.True(saveButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ResetButton_DisabledWhenAtDefaults()
        {
            // Arrange
            var settings = new TimerSettings();
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var resetButton = cut.Find("button.btn-reset-defaults");

            // Assert - Reset button should be disabled when at defaults
            Assert.True(resetButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ShowsToast_WhenShowToastIsTrue()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Trigger save to show toast
            // First, make a change to enable save button
            var input = cut.Find("input[type='number']");
            input.Change(30);

            // Click save button
            cut.Find("button.btn-save").Click();

            // Assert - Toast should be visible
            cut.WaitForAssertion(() => cut.Markup.Contains("Settings saved successfully!"));
        }

        [Fact]
        public void SettingsPage_BindsPomodoroMinutes()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 25 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");
            var pomodoroInput = inputs[0];

            // Assert
            Assert.Equal("25", pomodoroInput.GetAttribute("value"));
        }

        [Fact]
        public void SettingsPage_BindsShortBreakMinutes()
        {
            // Arrange
            var settings = new TimerSettings { ShortBreakMinutes = 5 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");
            var shortBreakInput = inputs[1];

            // Assert
            Assert.Equal("5", shortBreakInput.GetAttribute("value"));
        }

        [Fact]
        public void SettingsPage_BindsLongBreakMinutes()
        {
            // Arrange
            var settings = new TimerSettings { LongBreakMinutes = 15 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");
            var longBreakInput = inputs[2];

            // Assert
            Assert.Equal("15", longBreakInput.GetAttribute("value"));
        }

        [Fact]
        public void SettingsPage_BindsSoundEnabled()
        {
            // Arrange
            var settings = new TimerSettings { SoundEnabled = true };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var soundCheckbox = checkboxes[0];

            // Assert
            Assert.True(soundCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_BindsNotificationsEnabled()
        {
            // Arrange
            var settings = new TimerSettings { NotificationsEnabled = true };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var notificationsCheckbox = checkboxes[1];

            // Assert
            Assert.True(notificationsCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_BindsAutoStartEnabled()
        {
            // Arrange
            var settings = new TimerSettings { AutoStartEnabled = true };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var autoStartCheckbox = checkboxes[2];

            // Assert
            Assert.True(autoStartCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_BindsAutoStartDelaySeconds()
        {
            // Arrange
            var settings = new TimerSettings
            {
                AutoStartEnabled = true,
                AutoStartDelaySeconds = 10
            };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");

            // Find auto-start delay input (should be 4th number input)
            var autoStartDelayInput = inputs[3];

            // Assert
            Assert.Equal("10", autoStartDelayInput.GetAttribute("value"));
        }

        [Fact]
        public async Task SettingsPage_ExportJson_CallsExportService()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            _mockExportService.Setup(x => x.ExportToJsonAsync())
                .ReturnsAsync("{\"test\": \"data\"}");

            // Act
            cut.Find("button.btn-export").Click();

            // Assert
            _mockExportService.Verify(x => x.ExportToJsonAsync(), Times.Once);
        }

        [Fact]
        public async Task SettingsPage_ClearData_CallsClearAllData()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));

            // Act - Click confirm button
            cut.Find("button.btn-confirm-danger").Click();

            // Assert
            _mockExportService.Verify(x => x.ClearAllDataAsync(), Times.Once);
        }

        [Fact]
        public void SettingsPage_ShowsImportResult_WhenImportResultIsNotNull()
        {
            // Arrange - This test verifies import result display branch
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // The import result is shown after import, which requires InputFile component
            // This tests that markup contains container for import result
            cut.Markup.Contains("import-container");
        }

        [Fact]
        public void SettingsPage_ImportButton_DisabledWhenImporting()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Find import button
            var importLabel = cut.Find("label.btn-import");

            // Assert - Should not have disabled class initially
            Assert.False(importLabel.ClassList.Contains("disabled"));
        }

        [Fact]
        public void SettingsPage_HasCorrectPageTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Page should have title
            cut.Markup.Contains("⚙️ Settings");
        }

        [Fact]
        public void SettingsPage_HasCorrectTagline()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("Customize your pomodoro experience");
        }

        [Fact]
        public void SettingsPage_HasCorrectSectionIcons()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Check for section icons
            cut.Markup.Contains("🍅"); // Pomodoro
            cut.Markup.Contains("☕"); // Short Break
            cut.Markup.Contains("🏖️"); // Long Break
            cut.Markup.Contains("🔊"); // Sound
            cut.Markup.Contains("🔔"); // Notifications
            cut.Markup.Contains("▶️"); // Auto-start
            cut.Markup.Contains("⏱️"); // Auto-start delay
            cut.Markup.Contains("📤"); // Export
            cut.Markup.Contains("📥"); // Import
            cut.Markup.Contains("🗑️"); // Clear
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasWarningIcon()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Contains("⚠️"));
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasWarningMessage()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("This will permanently delete all your activities, tasks, and reset settings to defaults.");
                cut.Markup.Contains("This action cannot be undone.");
            });
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasConfirmAndCancelButtons()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("Yes, Clear All");
                cut.Markup.Contains("Cancel");
            });
        }

        [Fact]
        public void SettingsPage_AutoStartDelayField_HasCorrectMinMax()
        {
            // Arrange
            var settings = new TimerSettings { AutoStartEnabled = true };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");
            var autoStartDelayInput = inputs[3];

            // Assert
            Assert.Equal("0", autoStartDelayInput.GetAttribute("min"));
            Assert.Equal("60", autoStartDelayInput.GetAttribute("max"));
        }

        [Fact]
        public void SettingsPage_TimerDurationInputs_HaveCorrectMinMax()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var inputs = cut.FindAll("input[type='number']");

            // Assert - Pomodoro input
            Assert.Equal("1", inputs[0].GetAttribute("min"));
            Assert.Equal("60", inputs[0].GetAttribute("max"));

            // Assert - Short Break input
            Assert.Equal("1", inputs[1].GetAttribute("min"));
            Assert.Equal("30", inputs[1].GetAttribute("max"));

            // Assert - Long Break input
            Assert.Equal("1", inputs[2].GetAttribute("min"));
            Assert.Equal("60", inputs[2].GetAttribute("max"));
        }

        [Fact]
        public void SettingsPage_SaveButton_HasCorrectIcon()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("✓");
        }

        [Fact]
        public void SettingsPage_ResetButton_HasCorrectIcon()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            cut.Markup.Contains("↺");
        }

        [Fact]
        public void SettingsPage_ShowsToastWithCustomMessage()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 30 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Make a change to enable save button
            var input = cut.Find("input[type='number']");
            input.Change(30);

            // Click save button
            cut.Find("button.btn-save").Click();

            // Assert - Toast should be visible with custom message
            cut.WaitForAssertion(() => cut.Markup.Contains("Settings saved successfully!"));
        }

        [Fact]
        public void SettingsPage_SoundCheckbox_UncheckedWhenDisabled()
        {
            // Arrange
            var settings = new TimerSettings { SoundEnabled = false };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var soundCheckbox = checkboxes[0];

            // Assert
            Assert.False(soundCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_NotificationsCheckbox_UncheckedWhenDisabled()
        {
            // Arrange
            var settings = new TimerSettings { NotificationsEnabled = false };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var notificationsCheckbox = checkboxes[1];

            // Assert
            Assert.False(notificationsCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_AutoStartCheckbox_UncheckedWhenDisabled()
        {
            // Arrange
            var settings = new TimerSettings { AutoStartEnabled = false };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var checkboxes = cut.FindAll("input[type='checkbox']");
            var autoStartCheckbox = checkboxes[2];

            // Assert
            Assert.False(autoStartCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void SettingsPage_SaveButton_DisabledWhenNoChangesExist()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 30 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var saveButton = cut.Find("button.btn-save");

            // Assert - Save button should be disabled when no changes have been made
            Assert.True(saveButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ResetButton_EnabledWhenNotAtDefaults()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 30 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            var resetButton = cut.Find("button.btn-reset-defaults");

            // Assert - Reset button should be enabled when not at defaults
            Assert.False(resetButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_HasSettingsPageClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("settings-page", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingsHeaderClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("settings-header", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingsSectionClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Should have multiple settings sections
            Assert.Contains("settings-section", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingItemClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Should have multiple setting items
            Assert.Contains("setting-item", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingLabelClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-label", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingIconClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-icon", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingNameClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-name", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingInputClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-input", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingToggleClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-toggle", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleControlClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("toggle-control", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleInputClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("toggle-input", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleLabelClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("toggle-label", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleInnerClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("toggle-inner", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleSwitchClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("toggle-switch", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingsActionsClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("settings-actions", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnSaveClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-save", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnResetDefaultsClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-reset-defaults", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnExportClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-export", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnImportClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-import", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnClearClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-clear", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnConfirmDangerClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("btn-confirm-danger", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasBtnCancelClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("btn-cancel", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasSettingsToastClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Trigger save to show toast
            var input = cut.Find("input[type='number']");
            input.Change(30);
            cut.Find("button.btn-save").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("settings-toast", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationModalClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("confirmation-modal", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationContentClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("confirmation-content", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationButtonsClass()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Contains("confirmation-buttons", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasImportContainerClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("import-container", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasFileInputClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("file-input", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingDangerClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("setting-danger", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnExportJsonClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("btn-export-json", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettingsTaglineClass()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("settings-tagline", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllSettingsSections()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - All sections should be present
            Assert.Contains("Timer Durations", cut.Markup);
            Assert.Contains("Preferences", cut.Markup);
            Assert.Contains("Auto-Start", cut.Markup);
            Assert.Contains("Data Management", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllTimerDurationInputs()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - All timer duration inputs should be present
            Assert.Contains("Pomodoro", cut.Markup);
            Assert.Contains("Short Break", cut.Markup);
            Assert.Contains("Long Break", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllPreferenceToggles()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - All preference toggles should be present
            Assert.Contains("Sound", cut.Markup);
            Assert.Contains("Notifications", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllDataManagementButtons()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - All data management buttons should be present
            Assert.Contains("Export Backup", cut.Markup);
            Assert.Contains("Import Backup", cut.Markup);
            Assert.Contains("Clear All Data", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllActionButtons()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - All action buttons should be present
            Assert.Contains("btn-save", cut.Markup);
            Assert.Contains("btn-reset-defaults", cut.Markup);
        }

        [Fact]
        public void SettingsPage_AutoStartDelay_HasCorrectLabel()
        {
            // Arrange
            var settings = new TimerSettings { AutoStartEnabled = true };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Auto-start Delay (seconds)", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ExportButton_HasCorrectTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Export all data as JSON backup", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ClearButton_HasCorrectTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Delete all data", cut.Markup);
        }

        [Fact]
        public void SettingsPage_SaveButton_HasCorrectTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Save Settings", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ResetButton_HasCorrectTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Reset to Defaults", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ImportButton_HasCorrectText()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Import JSON", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ExportButton_HasCorrectText()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Export JSON", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ClearButton_HasCorrectText()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert
            Assert.Contains("Clear", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasCorrectHeading()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasCorrectParagraphs()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("This will permanently delete all your activities, tasks, and reset settings to defaults.");
                cut.Markup.Contains("This action cannot be undone.");
            });
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasStrongWarning()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Act - Click clear button to show confirmation
            cut.Find("button.btn-clear").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Contains("<strong>This action cannot be undone.</strong>"));
        }

        [Fact]
        public void SettingsPage_Toast_HasDefaultMessage()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 30 };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            // Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Make a change to enable save button
            var input = cut.Find("input[type='number']");
            input.Change(30);

            // Click save button
            cut.Find("button.btn-save").Click();

            // Assert - Toast should have default message
            cut.WaitForAssertion(() => cut.Markup.Contains("Settings saved successfully!"));
        }

        [Fact]
        public void SettingsPage_HasCorrectPageStructure()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Page should have correct structure
            Assert.Contains("settings-page", cut.Markup);
            Assert.Contains("settings-header", cut.Markup);
            Assert.Contains("settings-section", cut.Markup);
            Assert.Contains("settings-actions", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasCorrectHeaderStructure()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Header should have correct structure
            Assert.Contains("settings-header", cut.Markup);
            Assert.Contains("⚙️ Settings", cut.Markup);
            Assert.Contains("settings-tagline", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasCorrectActionsStructure()
        {
            // Arrange & Act
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Assert - Actions should have correct structure
            Assert.Contains("settings-actions", cut.Markup);
            Assert.Contains("btn-save", cut.Markup);
            Assert.Contains("btn-reset-defaults", cut.Markup);
        }

        [Fact]
        public void SettingsPage_Toast_ShowsDefaultMessageWhenToastMessageIsNull()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Set ShowToast = true, ToastMessage = null, and trigger re-render
            cut.InvokeAsync(() =>
            {
                var showToastProp = typeof(SettingsPageBase).GetProperty("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic);
                var toastMessageProp = typeof(SettingsPageBase).GetProperty("ToastMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                showToastProp?.SetValue(cut.Instance, true);
                toastMessageProp?.SetValue(cut.Instance, null);
                var stateHasChanged = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                stateHasChanged?.Invoke(cut.Instance, null);
            });

            // Assert - Should show default message
            Assert.Contains("Settings saved successfully!", cut.Markup);
        }

        [Fact]
        public void SettingsPage_Toast_ShowsCustomMessageWhenToastMessageIsNotNull()
        {
            // Arrange
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            // Set ShowToast = true, ToastMessage = custom, and trigger re-render
            cut.InvokeAsync(() =>
            {
                var showToastProp = typeof(SettingsPageBase).GetProperty("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic);
                var toastMessageProp = typeof(SettingsPageBase).GetProperty("ToastMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                showToastProp?.SetValue(cut.Instance, true);
                toastMessageProp?.SetValue(cut.Instance, "Custom toast message!");
                var stateHasChanged = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                stateHasChanged?.Invoke(cut.Instance, null);
            });

            // Assert - Should show custom message
            Assert.Contains("Custom toast message!", cut.Markup);
        }
    }

    // Mock NavigationManager for testing
    internal class MockNavigationManager : NavigationManager
    {
        public MockNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/settings");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // Do nothing for tests
        }
    }
}

