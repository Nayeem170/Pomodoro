using System;
using System.IO;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class SettingsPageBaseCoverageTests : TestContext
{
    private readonly Mock<ITimerService> _mockTimerService;
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IImportService> _mockImportService;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<IJSInteropService> _mockJSInteropService;
    private readonly Mock<ILogger<SettingsPageBase>> _mockLogger;
    private readonly Mock<ILogger<SettingsPresenterService>> _mockPresenterLogger;
    private readonly Mock<ICloudSyncService> _mockCloudSyncService;

    public SettingsPageBaseCoverageTests()
    {
        _mockTimerService = new Mock<ITimerService>();
        _mockExportService = new Mock<IExportService>();
        _mockImportService = new Mock<IImportService>();
        _mockTaskService = new Mock<ITaskService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockJSInteropService = new Mock<IJSInteropService>();
        _mockLogger = new Mock<ILogger<SettingsPageBase>>();
        _mockPresenterLogger = new Mock<ILogger<SettingsPresenterService>>();
        _mockCloudSyncService = new Mock<ICloudSyncService>();
        _mockCloudSyncService.SetupGet(x => x.IsConnected).Returns(false);

        _mockTimerService.Setup(x => x.Settings).Returns(new TimerSettings());

        Services.AddSingleton(_mockTimerService.Object);
        Services.AddSingleton(_mockExportService.Object);
        Services.AddSingleton(_mockImportService.Object);
        Services.AddSingleton(_mockTaskService.Object);
        Services.AddSingleton(_mockActivityService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
        Services.AddSingleton(_mockJSInteropService.Object);
        Services.AddSingleton<NavigationManager, TestNavManager>();
        Services.AddSingleton(_mockLogger.Object);
        Services.AddSingleton(_mockPresenterLogger.Object);
        Services.AddSingleton(new SettingsPresenterService(_mockPresenterLogger.Object));
        Services.AddSingleton(_mockCloudSyncService.Object);
        Services.AddSingleton(new Mock<ILogger<CloudSyncSettings>>().Object);
    }

    [Fact]
    public async Task ResetSettings_UpdatesTimerAndShowsToast()
    {
        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;
        component.TestSettings = new TimerSettings { PomodoroMinutes = 30 };
        component.SetTimerService(_mockTimerService.Object);

        await renderedComponent.InvokeAsync(async () => await component.TestResetSettings());

        Assert.Equal(25, component.TestSettings.PomodoroMinutes);
        _mockTimerService.Verify(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()), Times.Once);
        Assert.True(component.TestShowToast);
        Assert.Equal("Settings reset to defaults!", component.TestToastMessage);
    }

    [Fact]
    public void MarkDirty_CallsSaveAndRefresh()
    {
        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;
        component.SetTimerService(_mockTimerService.Object);

        component.TestMarkDirty();

        _mockTimerService.Verify(x => x.UpdateSettingsAsync(It.IsAny<TimerSettings>()), Times.Once);
    }

    [Fact]
    public async Task ClearData_WithCloudConnected_ClearsRemoteData()
    {
        _mockCloudSyncService.SetupGet(x => x.IsConnected).Returns(true);
        _mockCloudSyncService.Setup(x => x.ClearRemoteDataAsync()).Returns(Task.CompletedTask);

        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;
        component.SetExportService(_mockExportService.Object);
        component.SetTaskService(_mockTaskService.Object);
        component.SetActivityService(_mockActivityService.Object);
        component.SetTimerService(_mockTimerService.Object);
        component.TestShowClearConfirmation = true;

        await renderedComponent.InvokeAsync(async () => await component.TestClearData());

        _mockCloudSyncService.Verify(x => x.ClearRemoteDataAsync(), Times.Once);
        _mockExportService.Verify(x => x.ClearAllDataAsync(), Times.Once);
        Assert.False(component.TestIsClearing);
    }

    [Fact]
    public async Task HandleImport_FileTooLarge_ShowsMessage()
    {
        var mockFile = new Mock<IBrowserFile>();
        mockFile.SetupGet(f => f.Size).Returns(100_000_000);
        mockFile.SetupGet(f => f.Name).Returns("big.json");

        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;

        var files = new[] { mockFile.Object };
        await renderedComponent.InvokeAsync(async () => await component.TestHandleImport(new InputFileChangeEventArgs(files)));

        component.TestImportResult.Should().Contain("File too large");
    }

    [Fact]
    public async Task HandleImport_SuccessfulImport_ReloadsServices()
    {
        var mockFile = new Mock<IBrowserFile>();
        mockFile.SetupGet(f => f.Size).Returns(1000);
        mockFile.SetupGet(f => f.Name).Returns("backup.json");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream("{\"activities\":[],\"tasks\":[],\"settings\":{}}"u8.ToArray()));

        _mockImportService.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(ImportResult.Succeeded(3, 0, 2, 0, false));

        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;
        component.SetTimerService(_mockTimerService.Object);

        var files = new[] { mockFile.Object };
        await renderedComponent.InvokeAsync(async () => await component.TestHandleImport(new InputFileChangeEventArgs(files)));

        _mockTaskService.Verify(x => x.ReloadAsync(), Times.Once);
        _mockActivityService.Verify(x => x.ReloadAsync(), Times.Once);
        Assert.True(component.TestShowToast);
    }

    [Fact]
    public async Task HandleImport_FailedImport_ShowsError()
    {
        var mockFile = new Mock<IBrowserFile>();
        mockFile.SetupGet(f => f.Size).Returns(1000);
        mockFile.SetupGet(f => f.Name).Returns("bad.json");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream("invalid json"u8.ToArray()));

        _mockImportService.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ReturnsAsync(new ImportResult { Success = false, ErrorMessage = "Parse error" });

        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;

        var files = new[] { mockFile.Object };
        await renderedComponent.InvokeAsync(async () => await component.TestHandleImport(new InputFileChangeEventArgs(files)));

        component.TestImportResult.Should().Contain("Parse error");
        Assert.False(component.TestIsImporting);
    }

    [Fact]
    public async Task HandleImport_Exception_ShowsError()
    {
        var mockFile = new Mock<IBrowserFile>();
        mockFile.SetupGet(f => f.Size).Returns(1000);
        mockFile.SetupGet(f => f.Name).Returns("error.json");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream("data"u8.ToArray()));

        _mockImportService.Setup(x => x.ImportFromJsonAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Import crashed"));

        var renderedComponent = RenderComponent<TestableSettingsPageBase2>();
        var component = renderedComponent.Instance;

        var files = new[] { mockFile.Object };
        await renderedComponent.InvokeAsync(async () => await component.TestHandleImport(new InputFileChangeEventArgs(files)));

        component.TestImportResult.Should().Contain("Import failed");
        Assert.False(component.TestIsImporting);
    }

    public class TestableSettingsPageBase2 : SettingsPageBase
    {
        public TimerSettings TestSettings { get => Settings; set => Settings = value; }
        public TimerSettings TestOriginalSettings { get => OriginalSettings; set => OriginalSettings = value; }
        public bool TestShowToast { get => ShowToast; set => ShowToast = value; }
        public string? TestToastMessage { get => ToastMessage; set => ToastMessage = value; }
        public bool TestIsExporting { get => IsExporting; set => IsExporting = value; }
        public bool TestIsImporting { get => IsImporting; set => IsImporting = value; }
        public bool TestIsClearing { get => IsClearing; set => IsClearing = value; }
        public bool TestShowClearConfirmation { get => ShowClearConfirmation; set => ShowClearConfirmation = value; }
        public string? TestImportResult { get => ImportResult; set => ImportResult = value; }

        public void SetTimerService(ITimerService s) => TimerService = s;
        public void SetExportService(IExportService s) => ExportService = s;
        public void SetTaskService(ITaskService s) => TaskService = s;
        public void SetActivityService(IActivityService s) => ActivityService = s;
        public void TestMarkDirty() => MarkDirty();
        public Task TestResetSettings() => ResetSettings();
        public Task TestClearData() => ClearData();
        public Task TestHandleImport(InputFileChangeEventArgs e) => HandleImport(e);
    }

    internal class TestNavManager : NavigationManager
    {
        public TestNavManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }
        protected override void NavigateToCore(string uri, bool forceLoad) { }
    }
}
