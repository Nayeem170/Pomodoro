using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Service")]
public class SessionOptionsBranchTests
{
    [Fact]
    public void GetDefaultOption_PomodoroCountAtInterval_ReturnsLongBreak()
    {
        var appState = new AppState
        {
            Settings = new TimerSettings { LongBreakInterval = 4 },
            TodayPomodoroCount = 4
        };
        var service = new SessionOptionsService(appState);

        var result = service.GetDefaultOption(SessionType.Pomodoro);

        Assert.Equal(SessionType.LongBreak, result);
    }

    [Fact]
    public void GetDefaultOption_PomodoroCountNotAtInterval_ReturnsPomodoro()
    {
        var appState = new AppState
        {
            Settings = new TimerSettings { LongBreakInterval = 4 },
            TodayPomodoroCount = 3
        };
        var service = new SessionOptionsService(appState);

        var result = service.GetDefaultOption(SessionType.Pomodoro);

        Assert.Equal(SessionType.Pomodoro, result);
    }
}

[Trait("Category", "Service")]
public class ConsentServiceBranchTests
{
    [Fact]
    public async Task HandleTimerCompletedAsync_BreakCompletedWithAutoStartSession_ShowsConsentModal()
    {
        var timerServiceMock = new Mock<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sessionOptionsServiceMock = new Mock<ISessionOptionsService>();
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var appState = new AppState
        {
            Settings = new TimerSettings
            {
                AutoStartSession = true,
                AutoStartDelaySeconds = 5,
                SoundEnabled = false,
                NotificationsEnabled = false
            }
        };

        sessionOptionsServiceMock
            .Setup(x => x.GetOptionsForSessionType(It.IsAny<SessionType>()))
            .Returns(new List<ConsentOption>
            {
                new() { SessionType = SessionType.Pomodoro, Label = "Start Pomodoro", Duration = "25 min", IsDefault = true }
            });

        sessionOptionsServiceMock
            .Setup(x => x.GetDefaultOption(It.IsAny<SessionType>()))
            .Returns(SessionType.Pomodoro);

        var service = new ConsentService(
            timerServiceMock.Object,
            taskServiceMock.Object,
            notificationServiceMock.Object,
            appState,
            sessionOptionsServiceMock.Object,
            loggerMock.Object);

        var args = new TimerCompletedEventArgs(SessionType.ShortBreak, null, null, 5, true, DateTime.UtcNow);
        await service.HandleTimerCompletedAsync(args);

        Assert.True(service.IsModalVisible);
    }
}

[Trait("Category", "Component")]
public class DailyViewBranchTests : TestHelper
{
    [Fact]
    public void Render_WithNullActivities_RendersWithDefaults()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters.Add(p => p.Activities, (List<ActivityRecord>?)null));

        Assert.Contains("Time distribution", cut.Markup);
    }
}

[Trait("Category", "Component")]
public class DataManagementSettingsBranchTests : TestHelper
{
    [Fact]
    public void Render_WithIsExportingTrue_ShowsExportingText()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Settings.DataManagementSettings>(
            parameters => parameters.Add(p => p.IsExporting, true));

        Assert.Contains("Exporting...", cut.Markup);
    }

    [Fact]
    public void Render_WithImportResult_ShowsImportResultText()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.Settings.DataManagementSettings>(
            parameters => parameters.Add(p => p.ImportResult, "Import successful"));

        Assert.Contains("Import successful", cut.Markup);
    }
}

[Trait("Category", "Component")]
public class ClearConfirmationModalBranchTests : TestHelper
{
    [Fact]
    public void Render_WithCloudSyncConnected_ShowsAndGoogleDriveText()
    {
        Services.Remove(Services.First(d => d.ServiceType == typeof(ICloudSyncService)));
        var cloudSyncMock = new Mock<ICloudSyncService>();
        cloudSyncMock.SetupGet(x => x.IsConnected).Returns(true);
        Services.AddSingleton(cloudSyncMock.Object);

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.ClearConfirmationModal>(
            parameters => parameters.Add(p => p.IsVisible, true));

        Assert.Contains("and Google Drive", cut.Markup);
    }
}

[Trait("Category", "Component")]
public class SettingsPageBranchTests : TestHelper
{
    [Fact]
    public void Render_WithCloudConnected_ShowsAndCloudBackupText()
    {
        Services.Remove(Services.First(d => d.ServiceType == typeof(ICloudSyncService)));
        var cloudSyncMock = new Mock<ICloudSyncService>();
        cloudSyncMock.SetupGet(x => x.IsConnected).Returns(true);
        Services.AddSingleton(cloudSyncMock.Object);

        var cut = RenderComponent<Pomodoro.Web.Pages.Settings>();

        Assert.Contains("and cloud backup", cut.Markup);
    }
}

[Trait("Category", "Component")]
public class TimeDistributionChartBranchTests : TestContext
{
    [Fact]
    public void Segments_WhenDistributionIsNull_HandlesGracefully()
    {
        var mockActivityService = new Mock<IActivityService>();
        var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

        mockActivityService
            .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
            .Returns((Dictionary<string, int>?)null);

        Services.AddSingleton(mockActivityService.Object);
        Services.AddSingleton(new TimeFormatter());
        Services.AddSingleton(mockLogger.Object);

        var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now));

        Assert.Empty(cut.Instance.Segments);
        Assert.Equal(0, cut.Instance.TotalMinutes);
    }

    [Fact]
    public void Segments_WhenAllValuesZero_ZeroPercentages()
    {
        var mockActivityService = new Mock<IActivityService>();
        var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

        mockActivityService
            .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
            .Returns(new Dictionary<string, int> { { "Task A", 0 }, { "Task B", 0 } });

        Services.AddSingleton(mockActivityService.Object);
        Services.AddSingleton(new TimeFormatter());
        Services.AddSingleton(mockLogger.Object);

        var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now));

        Assert.Equal(0, cut.Instance.TotalMinutes);
        Assert.All(cut.Instance.Segments, s => Assert.Equal(0, s.Percentage));
    }
}

[Trait("Category", "Component")]
public class WeeklyTimeDistributionBranchTests : TestContext
{
    [Fact]
    public void CalculateSegments_WhenActivitiesHaveZeroDuration_ZeroPercentages()
    {
        var mockActivityService = new Mock<IActivityService>();
        var mockLogger = new Mock<ILogger<WeeklyTimeDistributionBase>>();

        var weekStart = new DateTime(2024, 1, 1);
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart, DurationMinutes = 0, TaskName = "Task A" }
        };

        mockActivityService.Setup(x => x.GetAllActivities()).Returns(activities);

        Services.AddSingleton(mockActivityService.Object);
        Services.AddSingleton(new TimeFormatter());
        Services.AddSingleton(mockLogger.Object);

        var cut = RenderComponent<WeeklyTimeDistribution>(parameters => parameters
            .Add(p => p.WeekStart, weekStart));

        Assert.Equal(0, cut.Instance.TotalMinutes);
        Assert.All(cut.Instance.Segments, s => Assert.Equal(0, s.Percentage));
    }

    [Fact]
    public void FormattedTotal_WhenTimeFormatterIsNull_ReturnsRawValue()
    {
        var mockActivityService = new Mock<IActivityService>();
        var mockLogger = new Mock<ILogger<WeeklyTimeDistributionBase>>();

        mockActivityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>());

        Services.AddSingleton(mockActivityService.Object);
        Services.AddSingleton(new TimeFormatter());
        Services.AddSingleton(mockLogger.Object);

        var cut = RenderComponent<WeeklyTimeDistribution>(parameters => parameters
            .Add(p => p.WeekStart, new DateTime(2024, 1, 1)));

        var tfField = typeof(WeeklyTimeDistributionBase).GetProperty("TimeFormatter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        tfField!.SetValue(cut.Instance, null);

        Assert.Equal("0", cut.Instance.FormattedTotal);
    }
}

[Trait("Category", "Component")]
public class CloudSyncSettingsBranchTests : TestContext
{
    private readonly Mock<ICloudSyncService> _cloudSyncServiceMock;
    private readonly Mock<ILogger<Pomodoro.Web.Components.Settings.CloudSyncSettings>> _loggerMock;

    public CloudSyncSettingsBranchTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _cloudSyncServiceMock = new Mock<ICloudSyncService>();
        _loggerMock = new Mock<ILogger<Pomodoro.Web.Components.Settings.CloudSyncSettings>>();
        Services.AddSingleton(_cloudSyncServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
    }

    [Fact]
    public async Task Sync_Pulled_ShowsPullSuccessMessage()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Pulled(3, 1, 2, 0, true));

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        await cut.InvokeAsync(() => (Task)cut.Instance.GetType()
            .GetMethod("SyncNow", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(cut.Instance, null)!);

        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Once);
    }

    [Fact]
    public async Task Sync_Pushed_ShowsPushSuccessMessage()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Pushed());

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        await cut.InvokeAsync(() => (Task)cut.Instance.GetType()
            .GetMethod("SyncNow", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(cut.Instance, null)!);

        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Once);
    }

    [Fact]
    public async Task Sync_FailedWithNullErrorMessage_ShowsDefaultSyncFailed()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(null));

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        await cut.InvokeAsync(() => (Task)cut.Instance.GetType()
            .GetMethod("SyncNow", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(cut.Instance, null)!);

        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Once);
    }

    [Fact]
    public async Task Sync_ReconnectRequired_PulledAfterReconnect_ShowsPullMessage()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.SetupSequence(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(Constants.SyncMessages.ReconnectRequired))
            .ReturnsAsync(SyncResult.Pulled(1, 0, 1, 0, false));
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        await cut.InvokeAsync(() => (Task)cut.Instance.GetType()
            .GetMethod("SyncNow", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(cut.Instance, null)!);

        _cloudSyncServiceMock.Verify(x => x.ConnectAsync(It.IsAny<string>()), Times.Once);
        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Sync_ReconnectRequired_SecondSyncFailsWithNullError_ShowsSyncFailed()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.SetupSequence(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(Constants.SyncMessages.ReconnectRequired))
            .ReturnsAsync(SyncResult.Failed(null));
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        await cut.InvokeAsync(() => (Task)cut.Instance.GetType()
            .GetMethod("SyncNow", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(cut.Instance, null)!);

        _cloudSyncServiceMock.Verify(x => x.ConnectAsync(It.IsAny<string>()), Times.Once);
        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Exactly(2));
    }

    [Fact]
    public void OnSyncStatusChanged_WhenDisconnecting_SuppressesStateHasChanged()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);

        var cut = RenderComponent<Pomodoro.Web.Components.Settings.CloudSyncSettings>();

        var isDisconnectingField = typeof(Pomodoro.Web.Components.Settings.CloudSyncSettings)
            .GetField("_isDisconnecting", BindingFlags.Instance | BindingFlags.NonPublic)!;
        isDisconnectingField.SetValue(cut.Instance, true);

        var ex = Record.Exception(() =>
            _cloudSyncServiceMock.Raise(x => x.OnSyncStatusChanged += null));

        Assert.Null(ex);
    }
}
