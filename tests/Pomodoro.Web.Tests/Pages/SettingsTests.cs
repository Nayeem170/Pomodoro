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

            Services.AddSingleton(new SettingsPresenterService(_mockPresenterLogger.Object));

            Services.AddSingleton<NavigationManager>(new MockNavigationManager());
        }

        [Fact]
        public void SettingsPage_Renders_Successfully()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.NotNull(cut);
            cut.Markup.Contains("Timer durations");
        }

        [Fact]
        public void SettingsPage_ShowsTimerDurationsSection()
        {
            var settings = new TimerSettings
            {
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15
            };
            _mockTimerService.Setup(x => x.Settings).Returns(settings);

            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("Timer durations");
            cut.Markup.Contains("Pomodoro");
            cut.Markup.Contains("Short break");
            cut.Markup.Contains("Long break");
        }

        [Fact]
        public void SettingsPage_ShowsSoundNotificationSection()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("Sound & notifications");
            cut.Markup.Contains("Sound on completion");
            cut.Markup.Contains("Browser notifications");
        }

        [Fact]
        public void SettingsPage_ShowsAutomationSection()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("Automation");
            cut.Markup.Contains("Auto-start pomodoros");
        }

        [Fact]
        public void SettingsPage_ShowsKeyboardShortcutsSection()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("Keyboard shortcuts");
            cut.Markup.Contains("kbd-grid");
        }

        [Fact]
        public void SettingsPage_ShowsDataSection()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("Data");
            cut.Markup.Contains("Export data");
            cut.Markup.Contains("Import data");
            cut.Markup.Contains("Clear all data");
        }

        [Fact]
        public void SettingsPage_ExportButton_NotDisabledInitially()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            var exportButton = cut.Find("button.sec-btn");

            Assert.False(exportButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ClearButton_NotDisabledInitially()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            var clearButton = cut.Find("button.danger-btn");

            Assert.False(clearButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SettingsPage_ShowsClearConfirmationModal_WhenShowClearConfirmationIsTrue()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));
        }

        [Fact]
        public void SettingsPage_HidesClearConfirmationModal_WhenCancelled()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));

            cut.Find("button.btn-cancel-action").Click();

            cut.WaitForAssertion(() => Assert.DoesNotContain("Clear All Data?", cut.Markup));
        }

        [Fact]
        public async Task SettingsPage_ExportJson_CallsExportService()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
            _mockExportService.Setup(x => x.ExportToJsonAsync())
                .ReturnsAsync("{\"test\": \"data\"}");

            cut.Find("button.sec-btn").Click();

            _mockExportService.Verify(x => x.ExportToJsonAsync(), Times.Once);
        }

        [Fact]
        public async Task SettingsPage_ClearData_CallsClearAllData()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();
            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));

            cut.Find("button.btn-confirm-danger").Click();

            _mockExportService.Verify(x => x.ClearAllDataAsync(), Times.Once);
        }

        [Fact]
        public void SettingsPage_ShowsImportResult_WhenImportResultIsNotNull()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Markup.Contains("import-container");
        }

        [Fact]
        public void SettingsPage_ImportButton_NotDisabledInitially()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            var importLabel = cut.Find("label.sec-btn");

            Assert.False(importLabel.ClassList.Contains("disabled"));
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasWarningIcon()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => cut.Markup.Contains("⚠️"));
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasWarningMessage()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("This will permanently delete all your activities, tasks, and reset settings to defaults.");
                cut.Markup.Contains("This action cannot be undone.");
            });
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasConfirmAndCancelButtons()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("Yes, Clear All");
                cut.Markup.Contains("Cancel");
            });
        }

        [Fact]
        public void SettingsPage_ExportButton_HasCorrectTitle()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Export all data as JSON backup", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ClearButton_HasCorrectTitle()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Delete all data", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllSettingsSections()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Timer durations", cut.Markup);
            Assert.Contains("Sound & notifications", cut.Markup);
            Assert.Contains("Automation", cut.Markup);
            Assert.Contains("Keyboard shortcuts", cut.Markup);
            Assert.Contains("Data", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllTimerDurationRows()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Pomodoro", cut.Markup);
            Assert.Contains("Short break", cut.Markup);
            Assert.Contains("Long break", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllSoundNotificationToggles()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Sound on completion", cut.Markup);
            Assert.Contains("Browser notifications", cut.Markup);
        }

        [Fact]
        public void SettingsPage_RendersAllDataManagementButtons()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("Export data", cut.Markup);
            Assert.Contains("Import data", cut.Markup);
            Assert.Contains("Clear all data", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSettBodyClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("sett-body", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSectionClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("ss", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasRowClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("sr", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasRowLabelClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("sr-lbl", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("tog", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasToggleKnobClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("tog-k", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasStepperClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("stepper", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasStepButtonClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("step-btn", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasStepValClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("step-val", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSecBtnClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("sec-btn", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasDangerBtnClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("danger-btn", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasSubtitleClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("sr-sub", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasKeyboardShortcutClasses()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("kbd-grid", cut.Markup);
            Assert.Contains("kr", cut.Markup);
            Assert.Contains("kbd", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasBtnConfirmDangerClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => Assert.Contains("btn-confirm-danger", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasBtnCancelActionClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => Assert.Contains("btn-cancel-action", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationModalClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => Assert.Contains("confirmation-modal", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationContentClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => Assert.Contains("confirmation-content", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasConfirmationButtonsClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => Assert.Contains("confirmation-buttons", cut.Markup));
        }

        [Fact]
        public void SettingsPage_HasImportContainerClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("import-container", cut.Markup);
        }

        [Fact]
        public void SettingsPage_HasFileInputClass()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            Assert.Contains("file-input", cut.Markup);
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasCorrectHeading()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => cut.Markup.Contains("Clear All Data?"));
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasCorrectParagraphs()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("This will permanently delete all your activities, tasks, and reset settings to defaults.");
                cut.Markup.Contains("This action cannot be undone.");
            });
        }

        [Fact]
        public void SettingsPage_ConfirmationModal_HasStrongWarning()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.Find("button.danger-btn").Click();

            cut.WaitForAssertion(() => cut.Markup.Contains("<strong>This action cannot be undone.</strong>"));
        }

        [Fact]
        public void SettingsPage_Toast_ShowsDefaultMessageWhenToastMessageIsNull()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.InvokeAsync(() =>
            {
                var showToastProp = typeof(SettingsPageBase).GetProperty("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic);
                var toastMessageProp = typeof(SettingsPageBase).GetProperty("ToastMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                showToastProp?.SetValue(cut.Instance, true);
                toastMessageProp?.SetValue(cut.Instance, null);
                var stateHasChanged = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                stateHasChanged?.Invoke(cut.Instance, null);
            });

            Assert.Contains("Settings saved successfully!", cut.Markup);
        }

        [Fact]
        public void SettingsPage_Toast_ShowsCustomMessageWhenToastMessageIsNotNull()
        {
            var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

            cut.InvokeAsync(() =>
            {
                var showToastProp = typeof(SettingsPageBase).GetProperty("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic);
                var toastMessageProp = typeof(SettingsPageBase).GetProperty("ToastMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                showToastProp?.SetValue(cut.Instance, true);
                toastMessageProp?.SetValue(cut.Instance, "Custom toast message!");
                var stateHasChanged = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                stateHasChanged?.Invoke(cut.Instance, null);
            });

            Assert.Contains("Custom toast message!", cut.Markup);
        }
    }

    internal class MockNavigationManager : NavigationManager
    {
        public MockNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/settings");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
        }
    }
}
