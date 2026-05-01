using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class ConsentServiceBreakCompletionTests
{
    private static ConsentService CreateService(AppState appState)
    {
        return new ConsentService(
            new Mock<ITimerService>().Object,
            new Mock<ITaskService>().Object,
            new Mock<INotificationService>().Object,
            appState,
            new Mock<ISessionOptionsService>().Object,
            new Mock<ILogger<ConsentService>>().Object);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_BreakCompletedWithAutoStartPomodoros_ShowsConsentModal()
    {
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartPomodoros = true,
                AutoStartBreaks = false,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        var service = CreateService(appState);
        var args = new TimerCompletedEventArgs(SessionType.ShortBreak, null, null, 5, true, DateTime.UtcNow);

        await service.HandleTimerCompletedAsync(args);

        Assert.True(service.IsModalVisible);
    }
}

[Trait("Category", "Service")]
public class ImportServiceDuplicateTests
{
    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateTaskAndValidId_SkipsImport()
    {
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ImportService>>();

        var existingTaskId = Guid.NewGuid();
        var importTaskId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 1, 1);

        mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>
        {
            new() { Id = existingTaskId, Name = "Same Task", CreatedAt = createdAt }
        });
        mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var exportData = new
        {
            version = 1,
            exportDate = DateTime.UtcNow.ToString("O"),
            tasks = new[]
            {
                new { id = importTaskId.ToString(), name = "Same Task", createdAt = createdAt.ToString("O") }
            },
            activities = Array.Empty<object>(),
            settings = new { pomodoroDuration = 25, shortBreakDuration = 5, longBreakDuration = 15 }
        };

        var json = JsonSerializer.Serialize(exportData);
        var service = new ImportService(mockActivityRepo.Object, mockTaskRepo.Object, mockSettingsRepo.Object, mockLogger.Object);

        var result = await service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.True(result.TasksSkipped > 0);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateTaskAndEmptyId_SkipsMapping()
    {
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ImportService>>();

        var createdAt = new DateTime(2024, 1, 1);

        mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Same Task", CreatedAt = createdAt }
        });
        mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var exportData = new
        {
            version = 1,
            exportDate = DateTime.UtcNow.ToString("O"),
            tasks = new[]
            {
                new { id = Guid.Empty.ToString(), name = "Same Task", createdAt = createdAt.ToString("O") }
            },
            activities = Array.Empty<object>(),
            settings = new { pomodoroDuration = 25, shortBreakDuration = 5, longBreakDuration = 15 }
        };

        var json = JsonSerializer.Serialize(exportData);
        var service = new ImportService(mockActivityRepo.Object, mockTaskRepo.Object, mockSettingsRepo.Object, mockLogger.Object);

        var result = await service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.True(result.TasksSkipped > 0);
    }
}

[Trait("Category", "Service")]
public class CloudSyncServicePullBranchTests2
{
    [Fact]
    public async Task InitializeAsync_WhenLoadFailsOnce_RetriesAndSucceeds()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        var callCount = 0;
        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1) throw new Exception("DB error");
                return new SyncStateRecord();
            });

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();

        Assert.True(service.IsInitialized);
    }

    [Fact]
    public async Task SyncNowAsync_WhenRemoteNewer_PullsFromRemote()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SyncStateRecord { ClientId = "test-client", IsConnected = true, LastSyncedAt = DateTime.UtcNow.AddMinutes(-10) });

        var remoteEnvelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(remoteEnvelope);

        mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-123");
        mockGoogleDrive.Setup(g => g.ReadFileAsync("file-123")).ReturnsAsync(envelopeJson);
        mockJs.Setup(js => js.InvokeAsync<string>(Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        mockImport.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(1, 0, 1, 0, true));

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();
        var result = await service.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pulled, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenLocalNewer_PushesToRemote()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SyncStateRecord { ClientId = "test-client", IsConnected = true, LastSyncedAt = DateTime.UtcNow.AddMinutes(10) });

        var remoteEnvelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow.AddMinutes(-10),
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(remoteEnvelope);

        mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-123");
        mockGoogleDrive.Setup(g => g.ReadFileAsync("file-123")).ReturnsAsync(envelopeJson);
        mockExport.Setup(e => e.ExportToJsonAsync()).ReturnsAsync("{}");

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();
        var result = await service.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pushed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenRemoteDataNull_UsesEmptyString()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SyncStateRecord { ClientId = "test-client", IsConnected = true, LastSyncedAt = DateTime.UtcNow.AddMinutes(-10) });

        var remoteEnvelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            Data = null
        };
        var envelopeJson = JsonSerializer.Serialize(remoteEnvelope);

        mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-123");
        mockGoogleDrive.Setup(g => g.ReadFileAsync("file-123")).ReturnsAsync(envelopeJson);
        mockImport.Setup(i => i.ImportFromStringAsync(""))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();
        var result = await service.SyncNowAsync();

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SyncNowAsync_WhenImportFails_DoesNotReloadServices()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SyncStateRecord { ClientId = "test-client", IsConnected = true, LastSyncedAt = DateTime.UtcNow.AddMinutes(-10) });

        var remoteEnvelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(remoteEnvelope);

        mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-123");
        mockGoogleDrive.Setup(g => g.ReadFileAsync("file-123")).ReturnsAsync(envelopeJson);
        mockJs.Setup(js => js.InvokeAsync<string>(Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        mockImport.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Failed("Import failed"));

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();
        var result = await service.SyncNowAsync();

        Assert.True(result.Success);
        mockTask.Verify(t => t.ReloadAsync(), Times.Never);
        mockActivity.Verify(a => a.ReloadAsync(), Times.Never);
    }
}

[Trait("Category", "Component")]
public class TimerDisplayDashOffsetTests
{
    [Fact]
    public void GetDashOffset_WhenSettingsNull_ReturnsZero()
    {
        var mockTimer = new Mock<ITimerService>();
        mockTimer.SetupGet(t => t.Settings).Returns((TimerSettings?)null);
        mockTimer.SetupGet(t => t.RemainingTime).Returns(TimeSpan.FromMinutes(10));
        mockTimer.SetupGet(t => t.CurrentSessionType).Returns(SessionType.Pomodoro);

        var component = new TestableTimerDisplay(mockTimer.Object);

        var result = component.GetDashOffset();

        Assert.Equal("0", result);
    }

    [Fact]
    public void GetDashOffset_WhenDurationZero_ReturnsZero()
    {
        var mockTimer = new Mock<ITimerService>();
        var settings = new TimerSettings();
        var field = typeof(TimerSettings).GetField("_pomodoroMinutes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(settings, 0);
        mockTimer.SetupGet(t => t.Settings).Returns(settings);
        mockTimer.SetupGet(t => t.RemainingTime).Returns(TimeSpan.FromMinutes(10));
        mockTimer.SetupGet(t => t.CurrentSessionType).Returns(SessionType.Pomodoro);

        var component = new TestableTimerDisplay(mockTimer.Object);

        var result = component.GetDashOffset();

        Assert.Equal("0", result);
    }

    private class TestableTimerDisplay : TimerDisplayBase
    {
        public TestableTimerDisplay(ITimerService timerService)
        {
            TimerService = timerService;
        }
    }
}

[Trait("Category", "Component")]
public class WeeklyMiniChartDictionariesEqualTests
{
    [Fact]
    public void DictionariesEqual_SameReference_ReturnsTrue()
    {
        var dict = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };

        var result = CallDictionariesEqual(dict, dict);

        Assert.True(result);
    }

    [Fact]
    public void DictionariesEqual_OneNull_ReturnsFalse()
    {
        var dict = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };

        var result = CallDictionariesEqual(dict, null);

        Assert.False(result);
    }

    [Fact]
    public void DictionariesEqual_BothNull_ReturnsTrue()
    {
        var result = CallDictionariesEqual(null, null);

        Assert.True(result);
    }

    [Fact]
    public void DictionariesEqual_FirstArgNull_ReturnsFalse()
    {
        var dict = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };

        var result = CallDictionariesEqual(null, dict);

        Assert.False(result);
    }

    [Fact]
    public void DictionariesEqual_SecondArgNull_ReturnsFalse()
    {
        var dict = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };

        var result = CallDictionariesEqual(dict, null);

        Assert.False(result);
    }

    [Fact]
    public void DictionariesEqual_DifferentCounts_ReturnsFalse()
    {
        var a = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };
        var b = new Dictionary<DateTime, int> { { DateTime.Today, 10 }, { DateTime.Today.AddDays(1), 20 } };

        var result = CallDictionariesEqual(a, b);

        Assert.False(result);
    }

    [Fact]
    public void DictionariesEqual_DifferentValues_ReturnsFalse()
    {
        var a = new Dictionary<DateTime, int> { { DateTime.Today, 10 } };
        var b = new Dictionary<DateTime, int> { { DateTime.Today, 20 } };

        var result = CallDictionariesEqual(a, b);

        Assert.False(result);
    }

    private static bool CallDictionariesEqual(Dictionary<DateTime, int>? a, Dictionary<DateTime, int>? b)
    {
        var method = typeof(WeeklyMiniChartBase).GetMethod("DictionariesEqual",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var generic = method.MakeGenericMethod(typeof(DateTime), typeof(int));
        return (bool)generic.Invoke(null, new object?[] { a, b })!;
    }
}

[Trait("Category", "Service")]
public partial class TaskServiceCloudSyncTests
{
    [Fact]
    public async Task AddTaskAsync_WithCloudSync_SchedulesSync()
    {
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockIndexedDb = new Mock<IIndexedDbService>();
        var mockCloudSync = new Mock<ICloudSyncService>();
        var appState = new AppState();

        mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        var services = new ServiceCollection();
        services.AddSingleton<ICloudSyncService>(mockCloudSync.Object);
        services.AddSingleton<IIndexedDbService>(mockIndexedDb.Object);
        var serviceProvider = services.BuildServiceProvider();

        var taskService = new TaskService(mockTaskRepo.Object, mockIndexedDb.Object, appState, serviceProvider);
        await taskService.AddTaskAsync("Test Task");

        mockCloudSync.Verify(c => c.ScheduleSyncAsync(), Times.Once);
    }
}

[Trait("Category", "Service")]
public partial class TimerServiceCloudSyncTests
{
    [Fact]
    public async Task UpdateSettingsAsync_WithCloudSync_SchedulesSync()
    {
        var mockIndexedDb = new Mock<IIndexedDbService>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockJsRuntime = new Mock<IJSRuntime>();
        var mockLogger = new Mock<ILogger<TimerService>>();
        var mockCloudSync = new Mock<ICloudSyncService>();
        var appState = new AppState();

        var services = new ServiceCollection();
        services.AddSingleton<ICloudSyncService>(mockCloudSync.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dailyStatsService = new DailyStatsService(mockIndexedDb.Object, appState, new Mock<ILogger<DailyStatsService>>().Object);
        var jsTimerInterop = new JsTimerInterop(mockJsRuntime.Object, new Mock<ILogger<JsTimerInterop>>().Object);
        var timerService = new TimerService(
            mockIndexedDb.Object, mockSettingsRepo.Object, dailyStatsService,
            jsTimerInterop, appState, mockLogger.Object, serviceProvider);

        await timerService.UpdateSettingsAsync(new TimerSettings());

        mockCloudSync.Verify(c => c.ScheduleSyncAsync(), Times.Once);
    }

    [Fact]
    public async Task TimerCompletion_WithCloudSync_SchedulesSync()
    {
        var mockIndexedDb = new Mock<IIndexedDbService>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockJsRuntime = new Mock<IJSRuntime>();
        var mockLogger = new Mock<ILogger<TimerService>>();
        var mockCloudSync = new Mock<ICloudSyncService>();
        var appState = new AppState();

        var services = new ServiceCollection();
        services.AddSingleton<ICloudSyncService>(mockCloudSync.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dailyStatsService = new DailyStatsService(mockIndexedDb.Object, appState, new Mock<ILogger<DailyStatsService>>().Object);
        var jsTimerInterop = new JsTimerInterop(mockJsRuntime.Object, new Mock<ILogger<JsTimerInterop>>().Object);
        var timerService = new TimerService(
            mockIndexedDb.Object, mockSettingsRepo.Object, dailyStatsService,
            jsTimerInterop, appState, mockLogger.Object, serviceProvider);

        await timerService.InitializeAsync();
        await timerService.StartPomodoroAsync();
        appState.CurrentSession!.RemainingSeconds = 1;
        appState.CurrentSession!.EndAt = DateTime.UtcNow.AddSeconds(1);

        timerService.OnTimerTickJs();

        await Task.Delay(200);

        mockCloudSync.Verify(c => c.ScheduleSyncAsync(), Times.Once);
    }
}

[Trait("Category", "Service")]
public class ImportServiceDuplicateEmptyIdTests
{
    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateTaskEmptyId_SkipsMapping()
    {
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ImportService>>();

        var existingTaskId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 1, 1);

        mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>
        {
            new() { Id = existingTaskId, Name = "Same Task", CreatedAt = createdAt }
        });
        mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var emptyGuid = Guid.Empty.ToString();
        var exportData = new
        {
            version = 1,
            exportDate = DateTime.UtcNow.ToString("O"),
            tasks = new[]
            {
                new { id = emptyGuid, name = "Same Task", createdAt = createdAt.ToString("O") }
            },
            activities = Array.Empty<object>(),
            settings = new { pomodoroDuration = 25, shortBreakDuration = 5, longBreakDuration = 15 }
        };

        var json = JsonSerializer.Serialize(exportData);
        var service = new ImportService(mockActivityRepo.Object, mockTaskRepo.Object, mockSettingsRepo.Object, mockLogger.Object);

        var result = await service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.True(result.TasksSkipped > 0);
    }
}

[Trait("Category", "Service")]
public class CloudSyncServiceNullLastSyncedTests
{
    [Fact]
    public async Task SyncNowAsync_WhenLastSyncedAtNull_PullsFromRemote()
    {
        var mockGoogleDrive = new Mock<IGoogleDriveService>();
        var mockExport = new Mock<IExportService>();
        var mockImport = new Mock<IImportService>();
        var mockJs = new Mock<IJSRuntime>();
        var mockDb = new Mock<IIndexedDbService>();
        var mockLogger = new Mock<ILogger<CloudSyncService>>();
        var mockTask = new Mock<ITaskService>();
        var mockActivity = new Mock<IActivityService>();
        var mockTimer = new Mock<ITimerService>();

        mockDb.Setup(db => db.GetAsync<SyncStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SyncStateRecord { ClientId = "test-client", IsConnected = true, LastSyncedAt = null });

        var remoteEnvelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(remoteEnvelope);

        mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-123");
        mockGoogleDrive.Setup(g => g.ReadFileAsync("file-123")).ReturnsAsync(envelopeJson);
        mockJs.Setup(js => js.InvokeAsync<string>(Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        mockImport.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(1, 0, 1, 0, true));

        var service = new CloudSyncService(
            mockGoogleDrive.Object, mockExport.Object, mockImport.Object,
            mockJs.Object, mockDb.Object, mockLogger.Object,
            mockTask.Object, mockActivity.Object, mockTimer.Object);

        await service.InitializeAsync();
        var result = await service.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pulled, result.Action);
    }
}

[Trait("Category", "Component")]
public class TimeDistributionChartBreaksLabelTests : TestContext
{
    [Fact]
    public void CalculateSegments_WithBreaksLabel_UsesBreakColor()
    {
        var mockActivityService = new Mock<IActivityService>();
        var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

        mockActivityService
            .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
            .Returns(new Dictionary<string, int> { { Constants.Activity.BreaksLabel, 10 } });

        Services.AddSingleton(mockActivityService.Object);
        Services.AddSingleton(new TimeFormatter());
        Services.AddSingleton(mockLogger.Object);

        var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now));

        Assert.Single(cut.Instance.Segments);
        Assert.Equal(Constants.Activity.BreaksLabel, cut.Instance.Segments[0].Label);
        Assert.Equal("#1D9E75", cut.Instance.Segments[0].Color);
    }
}

[Trait("Category", "Component")]
public class WeeklyMiniChartKeyMismatchTests
{
    [Fact]
    public void DictionariesEqual_DifferentKeys_ReturnsFalse()
    {
        var a = new Dictionary<DateTime, int> { { new DateTime(2024, 1, 1), 10 } };
        var b = new Dictionary<DateTime, int> { { new DateTime(2024, 1, 2), 20 } };

        var method = typeof(WeeklyMiniChartBase).GetMethod("DictionariesEqual",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var generic = method.MakeGenericMethod(typeof(DateTime), typeof(int));
        var result = (bool)generic.Invoke(null, new object?[] { a, b })!;

        Assert.False(result);
    }
}
