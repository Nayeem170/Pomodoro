using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using System.Reflection;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Comprehensive tests for Settings.razor.cs code-behind
/// Tests initialization, actions, export/import, and state management
/// </summary>
[Trait("Category", "Page")]
public class SettingsBaseTests : TestContext
{
    #region Services and Mocks

    private Mock<ITimerService> TimerServiceMock { get; set; } = null!;
    private Mock<IExportService> ExportServiceMock { get; set; } = null!;
    private Mock<IImportService> ImportServiceMock { get; set; } = null!;
    private Mock<ITaskService> TaskServiceMock { get; set; } = null!;
    private Mock<IActivityService> ActivityServiceMock { get; set; } = null!;
    private Mock<IJSRuntime> JSRuntimeMock { get; set; } = null!;
    private Mock<IJSInteropService> JSInteropServiceMock { get; set; } = null!;
    private Mock<ILogger<SettingsPageBase>> LoggerMock { get; set; } = null!;
    private Mock<ILogger<SettingsPresenterService>> SettingsPresenterLoggerMock { get; set; } = null!;
    private SettingsPresenterService SettingsPresenterService { get; set; } = null!;

    #endregion

    #region Test Setup

    public SettingsBaseTests()
    {
        // Create mocks
        TimerServiceMock = new Mock<ITimerService>();
        ExportServiceMock = new Mock<IExportService>();
        ImportServiceMock = new Mock<IImportService>();
        TaskServiceMock = new Mock<ITaskService>();
        ActivityServiceMock = new Mock<IActivityService>();
        JSRuntimeMock = new Mock<IJSRuntime>();
        JSInteropServiceMock = new Mock<IJSInteropService>();
        LoggerMock = new Mock<ILogger<SettingsPageBase>>();
        SettingsPresenterLoggerMock = new Mock<ILogger<SettingsPresenterService>>();

        // Create real SettingsPresenterService instance
        SettingsPresenterService = new SettingsPresenterService(SettingsPresenterLoggerMock.Object);

        // Setup TimerService mock with default settings
        var defaultSettings = new TimerSettings();
        TimerServiceMock.Setup(x => x.Settings).Returns(defaultSettings);
        TimerServiceMock.Setup(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()))
            .Returns(Task.CompletedTask);

        // Setup ExportService mock
        ExportServiceMock.Setup(x => x.ExportToJsonAsync())
            .ReturnsAsync("{}");
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = true, ActivitiesImported = 5, TasksImported = 5, ActivitiesSkipped = 0, TasksSkipped = 0 });
        ExportServiceMock.Setup(x => x.ClearAllDataAsync())
            .Returns(Task.CompletedTask);

        // Setup TaskService mock
        TaskServiceMock.Setup(x => x.ReloadAsync())
            .Returns(Task.CompletedTask);

        // Setup ActivityService mock
        ActivityServiceMock.Setup(x => x.ReloadAsync())
            .Returns(Task.CompletedTask);

        // Setup JSInteropService mock
        JSInteropServiceMock.Setup(x => x.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        // Register all required services
        Services.AddSingleton(TimerServiceMock.Object);
        Services.AddSingleton(ExportServiceMock.Object);
        Services.AddSingleton(ImportServiceMock.Object);
        Services.AddSingleton(TaskServiceMock.Object);
        Services.AddSingleton(ActivityServiceMock.Object);
        Services.AddSingleton(JSRuntimeMock.Object);
        Services.AddSingleton(JSInteropServiceMock.Object);
        Services.AddSingleton(LoggerMock.Object);
        Services.AddSingleton(SettingsPresenterService);
        Services.AddSingleton(Mock.Of<ICloudSyncService>());
        Services.AddSingleton(new Mock<ILogger<Pomodoro.Web.Components.Settings.CloudSyncSettings>>().Object);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets value of a protected property using reflection
    /// </summary>
    private static T? GetProtectedProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return property != null ? (T?)property.GetValue(obj) : default;
    }

    /// <summary>
    /// Sets value of a protected property using reflection
    /// </summary>
    private static void SetProtectedProperty(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property?.SetValue(obj, value);
    }

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_CanBeRendered()
    {
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert
        Assert.NotNull(cut);
    }

    #endregion

    #region Initialization Tests

    [Fact]
    public void OnInitialized_SetsSettingsFromTimerService()
    {
        // Arrange
        var testSettings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15
        };
        TimerServiceMock.Setup(x => x.Settings).Returns(testSettings);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        // Assert
        var instance = cut.Instance;
        var settings = GetProtectedProperty<TimerSettings>(instance, "Settings");
        Assert.NotNull(settings);
        Assert.Equal(testSettings.PomodoroMinutes, settings.PomodoroMinutes);
        Assert.Equal(testSettings.ShortBreakMinutes, settings.ShortBreakMinutes);
        Assert.Equal(testSettings.LongBreakMinutes, settings.LongBreakMinutes);
    }

    #endregion

    #region Save Settings Tests

    [Fact]
    public async Task HandleSave_UpdatesTimerServiceAndShowsToast()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.HandleSave());

        // Assert
        TimerServiceMock.Verify(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()), Times.Once);
        var showToast = GetProtectedProperty<bool>(instance, "ShowToast");
        var toastMessage = GetProtectedProperty<string>(instance, "ToastMessage");
        Assert.True(showToast);
        Assert.Equal("Settings saved successfully!", toastMessage);
    }

    [Fact]
    public async Task HandleSave_ThrowsExceptionWhenTimerServiceFails()
    {
        // Arrange
        TimerServiceMock.Setup(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()))
            .ThrowsAsync(new Exception("Update failed"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => instance.HandleSave());
    }

    #endregion

    #region Reset to Defaults Tests

    [Fact]
    public void ResetToDefaults_ResetsSettingsToNewInstance()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var settings = GetProtectedProperty<TimerSettings>(instance, "Settings");
        SetProtectedProperty(instance, "Settings", new TimerSettings { PomodoroMinutes = 50 });

        // Act
        instance.ResetToDefaults();

        // Assert
        var updatedSettings = GetProtectedProperty<TimerSettings>(instance, "Settings");
        Assert.NotNull(updatedSettings);
        var defaults = new TimerSettings();
        Assert.Equal(defaults.PomodoroMinutes, updatedSettings.PomodoroMinutes);
        Assert.Equal(defaults.ShortBreakMinutes, updatedSettings.ShortBreakMinutes);
        Assert.Equal(defaults.LongBreakMinutes, updatedSettings.LongBreakMinutes);
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportJson_ExportsDataAndDownloadsFile()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.ExportJson());

        // Assert
        ExportServiceMock.Verify(x => x.ExportToJsonAsync(), Times.Once);
        JSInteropServiceMock.Verify(x => x.InvokeVoidAsync(It.IsAny<string>(),
            It.Is<object[]>(args => args != null && args.Length > 0)), Times.Once);
        var isExporting = GetProtectedProperty<bool>(instance, "IsExporting");
        Assert.False(isExporting);
    }

    [Fact]
    public async Task ExportJson_HandlesExceptionAndShowsError()
    {
        // Arrange
        ExportServiceMock.Setup(x => x.ExportToJsonAsync())
            .ThrowsAsync(new Exception("Export failed"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.ExportJson());

        // Assert
        var isExporting = GetProtectedProperty<bool>(instance, "IsExporting");
        var showToast = GetProtectedProperty<bool>(instance, "ShowToast");
        var toastMessage = GetProtectedProperty<string>(instance, "ToastMessage");
        Assert.False(isExporting);
        Assert.True(showToast);
        Assert.Contains("Failed to export JSON backup", toastMessage);
    }

    #endregion

    #region Clear Data Tests

    [Fact]
    public void ConfirmClearData_SetsShowClearConfirmationToTrue()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act
        instance.ConfirmClearData();

        // Assert
        var showClearConfirmation = GetProtectedProperty<bool>(instance, "ShowClearConfirmation");
        Assert.True(showClearConfirmation);
    }

    [Fact]
    public async Task ClearData_CallsExportService()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.ClearData());

        // Assert
        ExportServiceMock.Verify(x => x.ClearAllDataAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearData_ReloadsServices()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.ClearData());

        // Assert
        TaskServiceMock.Verify(x => x.ReloadAsync(), Times.Once);
        ActivityServiceMock.Verify(x => x.ReloadAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearData_HandlesExceptionAndSetsIsClearingToFalse()
    {
        // Arrange
        ExportServiceMock.Setup(x => x.ClearAllDataAsync())
            .ThrowsAsync(new Exception("Clear failed"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act - Use InvokeAsync to run within component's dispatcher context
        await cut.InvokeAsync(() => instance.ClearData());

        // Assert - ClearData only sets IsClearing to false on exception, doesn't show toast
        var isClearing = GetProtectedProperty<bool>(instance, "IsClearing");
        Assert.False(isClearing);
    }

    #endregion

    #region Download File Tests

    [Fact]
    public async Task DownloadFileAsync_CallsJSInteropService()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var fileName = "test.json";
        var content = "test content";
        var mimeType = "application/json";

        // Act
        await SettingsPresenterService.DownloadFileAsync(JSInteropServiceMock.Object, fileName, content, mimeType);

        // Assert - Verify the actual method name used in Settings.razor.cs
        JSInteropServiceMock.Verify(x => x.InvokeVoidAsync("fileInterop.downloadFile",
            It.Is<object[]>(args => args != null && args.Length >= 3)), Times.Once);
    }

    #endregion

    #region State Property Tests

    [Fact]
    public void HasChanges_ReturnsFalse_WhenSettingsEqualOriginal()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act
        var hasChanges = GetProtectedProperty<bool>(instance, "HasChanges");

        // Assert
        Assert.False(hasChanges);
    }

    [Fact]
    public void HasChanges_ReturnsTrue_WhenSettingsDifferFromOriginal()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var settings = GetProtectedProperty<TimerSettings>(instance, "Settings");
        SetProtectedProperty(instance, "Settings", new TimerSettings { PomodoroMinutes = 50 });

        // Act
        var hasChanges = GetProtectedProperty<bool>(instance, "HasChanges");

        // Assert
        Assert.True(hasChanges);
    }

    #endregion

    #region IsAtDefaults Property Tests

    [Fact]
    public void IsAtDefaults_ReturnsTrue_WhenSettingsMatchDefaults()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;

        // Act
        var isAtDefaults = GetProtectedProperty<bool>(instance, "IsAtDefaults");

        // Assert
        Assert.True(isAtDefaults);
    }

    [Fact]
    public void IsAtDefaults_ReturnsFalse_WhenSettingsDifferFromDefaults()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var settings = GetProtectedProperty<TimerSettings>(instance, "Settings");
        SetProtectedProperty(instance, "Settings", new TimerSettings { PomodoroMinutes = 50 });

        // Act
        var isAtDefaults = GetProtectedProperty<bool>(instance, "IsAtDefaults");

        // Assert
        Assert.False(isAtDefaults);
    }

    #endregion

    #region HandleImport Tests

    [Fact]
    public async Task HandleImport_SetsImportResult_WhenFileIsEmpty()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(0);
        mockFile.Setup(x => x.Name).Returns("test.json");
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Equal("No file selected", importResult);
    }

    [Fact]
    public async Task HandleImport_SetsImportResult_WhenFileIsNull()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        // Create InputFileChangeEventArgs with a null file in the list
        // This tests the file is null branch
#pragma warning disable CS8625 // Testing null file case intentionally
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new Microsoft.AspNetCore.Components.Forms.IBrowserFile[] { null });
#pragma warning restore CS8625

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Equal("No file selected", importResult);
    }

    [Fact]
    public async Task HandleImport_SetsImportResult_WhenFileIsTooLarge()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(20 * 1024 * 1024); // 20 MB
        mockFile.Setup(x => x.Name).Returns("large.json");
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Contains("File too large", importResult);
    }

    [Fact]
    public async Task HandleImport_SetsImportResult_WhenImportFails()
    {
        // Arrange
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = false, ErrorMessage = "Invalid format" });
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(100);
        mockFile.Setup(x => x.Name).Returns("test.json");
        mockFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")));
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Equal("Invalid format", importResult);
    }

    [Fact]
    public async Task HandleImport_SetsImportResult_WhenImportFailsWithNullErrorMessage()
    {
        // Arrange
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = false, ErrorMessage = null });
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(100);
        mockFile.Setup(x => x.Name).Returns("test.json");
        mockFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")));
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Equal("Import failed. Please check the file format.", importResult);
    }

    [Fact]
    public async Task HandleImport_HandlesException_AndSetsImportResult()
    {
        // Arrange
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Import error"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(100);
        mockFile.Setup(x => x.Name).Returns("test.json");
        mockFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")));
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var importResult = GetProtectedProperty<string>(instance, "ImportResult");
        Assert.Equal("Import failed. Please check the file format.", importResult);
        var isImporting = GetProtectedProperty<bool>(instance, "IsImporting");
        Assert.False(isImporting);
    }

    [Fact]
    public async Task HandleImport_ShowsToast_WhenImportSucceeds()
    {
        // Arrange
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = true, ActivitiesImported = 5, TasksImported = 5 });
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(100);
        mockFile.Setup(x => x.Name).Returns("test.json");
        mockFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")));
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));

        // Assert
        var showToast = GetProtectedProperty<bool>(instance, "ShowToast");
        var toastMessage = GetProtectedProperty<string>(instance, "ToastMessage");
        Assert.True(showToast);
        Assert.Contains("imported", toastMessage?.ToLower() ?? "");
        TaskServiceMock.Verify(x => x.ReloadAsync(), Times.Once);
        ActivityServiceMock.Verify(x => x.ReloadAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleImport_AutoHidesToast_AfterDelay()
    {
        // Arrange
        ImportServiceMock.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = true, ActivitiesImported = 5, TasksImported = 5 });
        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();
        var instance = cut.Instance;
        var mockFile = new Mock<Microsoft.AspNetCore.Components.Forms.IBrowserFile>();
        mockFile.Setup(x => x.Size).Returns(100);
        mockFile.Setup(x => x.Name).Returns("test.json");
        mockFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")));
        var args = new Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs(
            new[] { mockFile.Object });

        // Act
        await cut.InvokeAsync(() => instance.HandleImport(args));
        Assert.True(GetProtectedProperty<bool>(instance, "ShowToast"));

        // Wait for auto-hide (ToastDurationMs = 2000ms + buffer)
        await Task.Delay(2500);

        // Assert - toast should be hidden
        var showToast = GetProtectedProperty<bool>(instance, "ShowToast");
        var toastMessage = GetProtectedProperty<string>(instance, "ToastMessage");
        Assert.False(showToast);
        Assert.Null(toastMessage);
    }

    #endregion
}

