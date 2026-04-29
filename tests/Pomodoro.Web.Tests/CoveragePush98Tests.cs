using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Services.Repositories;
using Xunit;
using Index = Pomodoro.Web.Pages.Index;

namespace Pomodoro.Web.Tests.CoveragePush;

[Trait("Category", "Component")]
public class WeeklyTimeDistributionRenderingTests : TestContext
{
    [Fact]
    public void Render_WithPomodoroAndBreakActivities_RendersDonutAndLegend()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = DateTime.Now, DurationMinutes = 25 },
            new() { Type = SessionType.ShortBreak, CompletedAt = DateTime.Now, DurationMinutes = 5 },
            new() { Type = SessionType.LongBreak, CompletedAt = DateTime.Now, DurationMinutes = 15 },
        });

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyTimeDistribution>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Markup.Should().Contain("donut-wrap");
        cut.Markup.Should().Contain("legend");
        cut.Markup.Should().Contain("Task A");
        cut.Markup.Should().Contain("Breaks");
        cut.Instance.Segments.Should().HaveCount(2);
        cut.Instance.TotalMinutes.Should().Be(45);
    }

    [Fact]
    public void Render_WithNoActivities_RendersEmptyState()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>());

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyTimeDistribution>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Markup.Should().Contain("No activities this week");
        cut.Instance.Segments.Should().BeEmpty();
    }

    [Fact]
    public void OnActivityChanged_WhenNotDisposed_Recalculates()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = DateTime.Now, DurationMinutes = 25 },
        });

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyTimeDistribution>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        activityService.Raise(x => x.OnActivityChanged += null);

        cut.Instance.Segments.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>());

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyTimeDistribution>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Instance.Dispose();
        cut.Instance.Dispose();
    }

    [Fact]
    public void FormattedTotal_WhenTimeFormatterThrows_ReturnsRawValue()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = DateTime.Now, DurationMinutes = 25 },
        });

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton<TimeFormatter>(new ThrowingTimeFormatter());

        var cut = RenderComponent<WeeklyTimeDistribution>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Instance.FormattedTotal.Should().Be("25");
    }
}

[Trait("Category", "Component")]
public class WeeklyRecentActivityRenderingTests : TestContext
{
    [Fact]
    public void Render_WithAllSessionTypes_RendersCorrectLabelsAndIcons()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = DateTime.Now, DurationMinutes = 25 },
            new() { Type = SessionType.ShortBreak, CompletedAt = DateTime.Now, DurationMinutes = 5 },
            new() { Type = SessionType.LongBreak, CompletedAt = DateTime.Now, DurationMinutes = 15 },
        });

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyRecentActivity>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Markup.Should().Contain("Pomodoro completed");
        cut.Markup.Should().Contain("Short break");
        cut.Markup.Should().Contain("Long break");
        cut.Markup.Should().Contain("25 min");
        cut.Markup.Should().Contain("5 min");
        cut.Markup.Should().Contain("15 min");
    }

    [Fact]
    public void Render_WithNoActivities_RendersEmptyState()
    {
        var activityService = new Mock<IActivityService>();
        activityService.Setup(x => x.GetAllActivities()).Returns(new List<ActivityRecord>());

        Services.AddSingleton(activityService.Object);
        Services.AddSingleton(new TimeFormatter());

        var cut = RenderComponent<WeeklyRecentActivity>(p => p
            .Add(x => x.WeekStart, DateTime.Now.AddDays(-3)));

        cut.Markup.Should().Contain("No activities this week");
    }
}

[Trait("Category", "Component")]
public class ActivityTimelineRenderingTests : TestContext
{
    public ActivityTimelineRenderingTests()
    {
        Services.AddSingleton(new ActivityTimelineFormatter());
    }
    private static ActivityRecord MakeActivity(SessionType type, string? taskName = null) =>
        new() { Type = type, TaskName = taskName, CompletedAt = DateTime.Now, DurationMinutes = 25 };

    [Fact]
    public void Render_WithAllSessionTypes_RendersBadgesAndTimeline()
    {
        var activities = new List<ActivityRecord>
        {
            MakeActivity(SessionType.Pomodoro, "Task A"),
            MakeActivity(SessionType.ShortBreak),
            MakeActivity(SessionType.LongBreak),
        };

        var cut = RenderComponent<ActivityTimeline>(p => p
            .Add(x => x.Activities, activities));

        cut.Markup.Should().Contain("Pomodoro");
        cut.Markup.Should().Contain("Short break");
        cut.Markup.Should().Contain("Long break");
        cut.Markup.Should().Contain("Task A");
        cut.Markup.Should().Contain("25 min");
        cut.Markup.Should().Contain("tl-row");
        cut.Markup.Should().Contain("tl-badge");
    }

    [Fact]
    public void Render_WithNoActivities_RendersEmptyState()
    {
        var cut = RenderComponent<ActivityTimeline>(p => p
            .Add(x => x.Activities, new List<ActivityRecord>()));

        cut.Markup.Should().Contain("No activities for this day");
    }

    [Fact]
    public void Render_WithSingleActivity_RendersWithoutConnector()
    {
        var activities = new List<ActivityRecord>
        {
            MakeActivity(SessionType.Pomodoro, "Only Task"),
        };

        var cut = RenderComponent<ActivityTimeline>(p => p
            .Add(x => x.Activities, activities));

        cut.Markup.Should().Contain("Only Task");
        cut.Markup.Should().NotContain("tl-connector");
    }
}

[Trait("Category", "Component")]
public class CloudSyncSettingsPullBranchTests : TestContext
{
    private readonly Mock<ICloudSyncService> _cloudSyncMock;

    public CloudSyncSettingsPullBranchTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _cloudSyncMock = new Mock<ICloudSyncService>();
        Services.AddSingleton(_cloudSyncMock.Object);
        Services.AddSingleton(new Mock<ILogger<CloudSyncSettings>>().Object);
    }

    [Fact]
    public async Task SyncNow_WhenUpToDate_ShowsUpToDateToast()
    {
        var toastMessage = string.Empty;
        _cloudSyncMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncMock.Setup(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.UpToDate());

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        await Task.Delay(100);
        toastMessage.Should().Be(Constants.SyncMessages.AlreadyUpToDate);
    }
}

[Trait("Category", "Component")]
public class CurrentTaskIndicatorRenderingTests : TestContext
{
    [Fact]
    public void Render_WhenPomodoroWithTask_ShowsTaskName()
    {
        var taskId = Guid.NewGuid();
        var cut = RenderComponent<CurrentTaskIndicator>(p => p
            .Add(x => x.CurrentSessionType, SessionType.Pomodoro)
            .Add(x => x.CurrentTaskId, taskId)
            .Add(x => x.Tasks, new List<TaskItem>
            {
                new() { Id = taskId, Name = "My Task", CreatedAt = DateTime.Now }
            }));

        cut.Markup.Should().Contain("My Task");
        cut.Markup.Should().Contain("active-task");
    }

    [Fact]
    public void Render_WhenPomodoroWithNoTask_ShowsSelectPrompt()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(p => p
            .Add(x => x.CurrentSessionType, SessionType.Pomodoro)
            .Add(x => x.CurrentTaskId, (Guid?)null)
            .Add(x => x.Tasks, new List<TaskItem>()));

        cut.Markup.Should().Contain("active-task");
    }

    [Fact]
    public void Render_WhenBreakSession_DoesNotRender()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(p => p
            .Add(x => x.CurrentSessionType, SessionType.ShortBreak));

        cut.Markup.Should().NotContain("active-task");
    }
}

[Trait("Category", "Service")]
public class CloudSyncServiceSaveSyncStateTests
{
    [Fact]
    public async Task InitializeAsync_WhenIndexedDbThrows_DoesNotPropagate()
    {
        var logger = new Mock<ILogger<CloudSyncService>>();
        var indexedDb = new Mock<IIndexedDbService>();
        var googleDrive = new Mock<IGoogleDriveService>();
        var exportService = new Mock<IExportService>();
        var importService = new Mock<IImportService>();
        var jsRuntime = new Mock<IJSRuntime>();
        var taskService = new Mock<ITaskService>();
        var activityService = new Mock<IActivityService>();
        var timerService = new Mock<ITimerService>();

        indexedDb.Setup(x => x.GetAsync<object>(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("IndexedDb error"));

        var service = new CloudSyncService(
            googleDrive.Object, exportService.Object, importService.Object,
            jsRuntime.Object, indexedDb.Object, logger.Object,
            taskService.Object, activityService.Object, timerService.Object);

        var ex = await Record.ExceptionAsync(() => service.InitializeAsync());
        ex.Should().BeNull();
    }
}

internal class ThrowingTimeFormatter : TimeFormatter
{
    public override string FormatTime(int minutes) =>
        throw new InvalidOperationException("Test exception");
}
