using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Pages
{
    [Trait("Category", "Page")]
    public class SettingsPageBaseTests : TestContext
    {
        private readonly Mock<ITimerService> _mockTimerService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<IJSInteropService> _mockJSInteropService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<ILogger<SettingsPageBase>> _mockLogger;
        private readonly Mock<ILogger<SettingsPresenterService>> _mockPresenterLogger;

        public SettingsPageBaseTests()
        {
            _mockTimerService = new Mock<ITimerService>();
            _mockExportService = new Mock<IExportService>();
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
            Services.AddSingleton(_mockTaskService.Object);
            Services.AddSingleton(_mockActivityService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockJSInteropService.Object);
            Services.AddSingleton(_mockNavigationManager.Object);
            Services.AddSingleton(_mockLogger.Object);
            Services.AddSingleton(_mockPresenterLogger.Object);
             
            // Register SettingsPresenterService
            Services.AddSingleton(new SettingsPresenterService(_mockPresenterLogger.Object));
        }

        [Fact]
        public void OnInitialized_SetsSettingsFromTimerService()
        {
            // Arrange
            var testSettings = new TimerSettings { PomodoroMinutes = 30 };
            _mockTimerService.Setup(x => x.Settings).Returns(testSettings);

            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetTimerService(_mockTimerService.Object);

            // Act
            component.TestOnInitialized();

            // Assert
            Assert.Equal(testSettings.PomodoroMinutes, component.TestSettings.PomodoroMinutes);
            Assert.Equal(testSettings.PomodoroMinutes, component.TestOriginalSettings.PomodoroMinutes);
        }

        [Fact]
        public void HasChanges_ReturnsTrue_WhenSettingsDifferFromOriginal()
        {
            // Arrange
            var originalSettings = new TimerSettings { PomodoroMinutes = 25 };
            var modifiedSettings = new TimerSettings { PomodoroMinutes = 30 };

            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestOriginalSettings = originalSettings;
            component.TestSettings = modifiedSettings;

            // Act
            var hasChanges = component.TestHasChanges;

            // Assert
            Assert.True(hasChanges);
        }

        [Fact]
        public void HasChanges_ReturnsFalse_WhenSettingsEqualOriginal()
        {
            // Arrange
            var settings = new TimerSettings { PomodoroMinutes = 25 };

            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestOriginalSettings = settings;
            component.TestSettings = settings;

            // Act
            var hasChanges = component.TestHasChanges;

            // Assert
            Assert.False(hasChanges);
        }

        [Fact]
        public void IsAtDefaults_ReturnsTrue_WhenSettingsMatchDefaults()
        {
            // Arrange
            var defaultSettings = new TimerSettings();
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestSettings = defaultSettings;

            // Act
            var isAtDefaults = component.TestIsAtDefaults;

            // Assert
            Assert.True(isAtDefaults);
        }

        [Fact]
        public void IsAtDefaults_ReturnsFalse_WhenSettingsDifferFromDefaults()
        {
            // Arrange
            var modifiedSettings = new TimerSettings { PomodoroMinutes = 30 };
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestSettings = modifiedSettings;

            // Act
            var isAtDefaults = component.TestIsAtDefaults;

            // Assert
            Assert.False(isAtDefaults);
        }

        [Fact]
        public async Task HandleSave_UpdatesTimerServiceAndShowsToast()
        {
            // Arrange
            var testSettings = new TimerSettings { PomodoroMinutes = 30 };
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestSettings = testSettings;
            component.SetTimerService(_mockTimerService.Object);

            // Act
            await renderedComponent.InvokeAsync(async () => await component.TestHandleSave());

            // Assert
            _mockTimerService.Verify(x => x.UpdateSettingsAsync(testSettings), Times.Once);
            Assert.True(component.TestShowToast);
            Assert.Equal("Settings saved successfully!", component.TestToastMessage);
        }

        [Fact]
        public async Task HandleSave_ThrowsExceptionWhenTimerServiceFails()
        {
            // Arrange
            var testSettings = new TimerSettings { PomodoroMinutes = 30 };
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestSettings = testSettings;
            component.SetTimerService(_mockTimerService.Object);

            _mockTimerService.Setup(x => x.UpdateSettingsAsync(testSettings))
                .ThrowsAsync(new Exception("Test error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => renderedComponent.InvokeAsync(async () => await component.TestHandleSave()));
            
            Assert.Equal("Test error", exception.Message);
            // Note: HandleSave doesn't catch exceptions in current implementation
        }

        [Fact]
        public void ResetToDefaults_ResetsSettingsToNewInstance()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.TestSettings = new TimerSettings { PomodoroMinutes = 30 };

            // Act
            component.TestResetToDefaults();

            // Assert
            Assert.Equal(25, component.TestSettings.PomodoroMinutes); // Default value
        }

        [Fact]
        public async Task ExportJson_ExportsDataAndDownloadsFile()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetExportService(_mockExportService.Object);
            component.SetJSInteropService(_mockJSInteropService.Object);

            var expectedJson = "{\"test\": \"data\"}";
            _mockExportService.Setup(x => x.ExportToJsonAsync())
                .ReturnsAsync(expectedJson);

            // Act
            await renderedComponent.InvokeAsync(async () => await component.TestExportJson());

            // Assert
            _mockExportService.Verify(x => x.ExportToJsonAsync(), Times.Once);
            _mockJSInteropService.Verify(x => x.InvokeVoidAsync("fileInterop.downloadFile", 
                It.IsAny<object[]>()), Times.Once);
            Assert.False(component.TestIsExporting);
        }

        [Fact]
        public async Task ExportJson_HandlesExceptionAndShowsError()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetExportService(_mockExportService.Object);

            _mockExportService.Setup(x => x.ExportToJsonAsync())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await renderedComponent.InvokeAsync(async () => await component.TestExportJson());

            // Assert
            Assert.True(component.TestShowToast);
            Assert.Equal("Failed to export JSON backup. Please try again.", component.TestToastMessage);
            Assert.False(component.TestIsExporting);
        }

        [Fact]
        public void ConfirmClearData_SetsShowClearConfirmationToTrue()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;

            // Act
            component.TestConfirmClearData();

            // Assert
            Assert.True(component.TestShowClearConfirmation);
        }

        [Fact]
        public async Task ClearData_ClearsAllDataAndReloadsServices()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetExportService(_mockExportService.Object);
            component.SetTaskService(_mockTaskService.Object);
            component.SetActivityService(_mockActivityService.Object);
            component.SetTimerService(_mockTimerService.Object);

            // Act
            await renderedComponent.InvokeAsync(async () => await component.TestClearData());

            // Assert
            _mockExportService.Verify(x => x.ClearAllDataAsync(), Times.Once);
            _mockTaskService.Verify(x => x.ReloadAsync(), Times.Once);
            _mockActivityService.Verify(x => x.ReloadAsync(), Times.Once);
            _mockTimerService.Verify(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()), Times.Once);
            Assert.False(component.TestIsClearing);
            Assert.False(component.TestShowClearConfirmation);
        }

        [Fact]
        public async Task ClearData_HandlesExceptionAndSetsIsClearingToFalse()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetExportService(_mockExportService.Object);

            _mockExportService.Setup(x => x.ClearAllDataAsync())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await renderedComponent.InvokeAsync(async () => await component.TestClearData());

            // Assert
            Assert.False(component.TestIsClearing);
            // Verify that the exception was handled (no toast message shown in current implementation)
        }

        [Fact]
        public async Task DownloadFileAsync_CallsJSInteropService()
        {
            // Arrange
            var renderedComponent = RenderComponent<TestableSettingsPageBase>();
            var component = renderedComponent.Instance;
            component.SetJSInteropService(_mockJSInteropService.Object);

            var testData = "test content";
            var fileName = "test.json";
            var mimeType = "application/json";

            // Act
            await component.TestDownloadFileAsync(fileName, testData, mimeType);

            // Assert
            _mockJSInteropService.Verify(x => x.InvokeVoidAsync("fileInterop.downloadFile", 
                fileName, It.IsAny<string>(), mimeType), Times.Once);
        }
    }

    // Testable class that exposes protected members
    public class TestableSettingsPageBase : SettingsPageBase
    {
        public TimerSettings TestSettings 
        { 
            get => Settings; 
            set => Settings = value; 
        }
        
        public TimerSettings TestOriginalSettings 
        { 
            get => OriginalSettings; 
            set => OriginalSettings = value; 
        }
        
        public bool TestShowToast 
        { 
            get => ShowToast; 
            set => ShowToast = value; 
        }
        
        public string? TestToastMessage 
        { 
            get => ToastMessage; 
            set => ToastMessage = value; 
        }
        
        public bool TestIsExporting 
        { 
            get => IsExporting; 
            set => IsExporting = value; 
        }
        
        public bool TestIsImporting 
        { 
            get => IsImporting; 
            set => IsImporting = value; 
        }
        
        public bool TestIsClearing 
        { 
            get => IsClearing; 
            set => IsClearing = value; 
        }
        
        public bool TestShowClearConfirmation 
        { 
            get => ShowClearConfirmation; 
            set => ShowClearConfirmation = value; 
        }
        
        public string? TestImportResult 
        { 
            get => ImportResult; 
            set => ImportResult = value; 
        }

        public bool TestHasChanges => HasChanges;
        public bool TestIsAtDefaults => IsAtDefaults;

        public void SetTimerService(ITimerService timerService) => TimerService = timerService;
        public void SetExportService(IExportService exportService) => ExportService = exportService;
        public void SetTaskService(ITaskService taskService) => TaskService = taskService;
        public void SetActivityService(IActivityService activityService) => ActivityService = activityService;
        public void SetJSInteropService(IJSInteropService jsInteropService) => JSInteropService = jsInteropService;
        public void SetSettingsPresenterService(SettingsPresenterService settingsPresenterService) => SettingsPresenterService = settingsPresenterService;

        public void TestOnInitialized() => OnInitialized();
        public Task TestHandleSave() => HandleSave();
        public void TestResetToDefaults() => ResetToDefaults();
        public Task TestExportJson() => ExportJson();
        public Task TestHandleImport(InputFileChangeEventArgs e) => HandleImport(e);
        public void TestConfirmClearData() => ConfirmClearData();
        public Task TestClearData() => ClearData();
        public Task TestDownloadFileAsync(string filename, string content, string mimeType) => SettingsPresenterService.DownloadFileAsync(JSInteropService, filename, content, mimeType);
    }
}
