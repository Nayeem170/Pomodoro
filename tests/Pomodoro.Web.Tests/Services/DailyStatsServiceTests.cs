using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class DailyStatsServiceTests
{
    private readonly Mock<IIndexedDbService> _mockIndexedDb;
    private readonly AppState _appState;
    private readonly Mock<ILogger<DailyStatsService>> _mockLogger;

    public DailyStatsServiceTests()
    {
        _mockIndexedDb = new Mock<IIndexedDbService>();
        _appState = new AppState();
        _mockLogger = new Mock<ILogger<DailyStatsService>>();
    }

    private DailyStatsService CreateService()
    {
        return new DailyStatsService(_mockIndexedDb.Object, _appState, _mockLogger.Object);
    }

    #region InitializeTodayStatsAsync

    [Fact]
    public async Task InitializeTodayStatsAsync_WhenStatsAreFromToday_RestoresAppState()
    {
        var todayKey = AppState.GetCurrentDayKey();
        var stats = new DailyStats
        {
            Date = todayKey,
            TotalFocusMinutes = 120,
            PomodoroCount = 4,
            TaskIdsWorkedOn = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        _mockIndexedDb
            .Setup(x => x.GetAsync<DailyStats>(Constants.Storage.DailyStatsStore, It.IsAny<string>()))
            .ReturnsAsync(stats);

        var service = CreateService();
        await service.InitializeTodayStatsAsync();

        Assert.Equal(120, _appState.TodayTotalFocusMinutes);
        Assert.Equal(4, _appState.TodayPomodoroCount);
        Assert.Equal(2, _appState.TodayTaskIdsWorkedOn.Count);
        Assert.Equal(todayKey, _appState.LastResetDate);
    }

    [Fact]
    public async Task InitializeTodayStatsAsync_WhenStatsAreFromPreviousDay_ResetsStats()
    {
        var stats = new DailyStats
        {
            Date = AppState.GetCurrentDayKey().AddDays(-1),
            TotalFocusMinutes = 120,
            PomodoroCount = 4
        };

        _mockIndexedDb
            .Setup(x => x.GetAsync<DailyStats>(Constants.Storage.DailyStatsStore, It.IsAny<string>()))
            .ReturnsAsync(stats);

        var service = CreateService();
        await service.InitializeTodayStatsAsync();

        Assert.Equal(0, _appState.TodayTotalFocusMinutes);
        Assert.Equal(0, _appState.TodayPomodoroCount);
    }

    [Fact]
    public async Task InitializeTodayStatsAsync_WhenNoStats_ResetsStats()
    {
        _mockIndexedDb
            .Setup(x => x.GetAsync<DailyStats>(Constants.Storage.DailyStatsStore, It.IsAny<string>()))
            .ReturnsAsync((DailyStats?)null);

        var service = CreateService();
        await service.InitializeTodayStatsAsync();

        Assert.Equal(0, _appState.TodayTotalFocusMinutes);
        Assert.Equal(0, _appState.TodayPomodoroCount);
    }

    [Fact]
    public async Task InitializeTodayStatsAsync_WhenTaskIdsIsNull_InitializesEmptyList()
    {
        var todayKey = AppState.GetCurrentDayKey();
        var stats = new DailyStats
        {
            Date = todayKey,
            TotalFocusMinutes = 60,
            PomodoroCount = 2,
            TaskIdsWorkedOn = null!
        };

        _mockIndexedDb
            .Setup(x => x.GetAsync<DailyStats>(Constants.Storage.DailyStatsStore, It.IsAny<string>()))
            .ReturnsAsync(stats);

        var service = CreateService();
        await service.InitializeTodayStatsAsync();

        Assert.NotNull(_appState.TodayTaskIdsWorkedOn);
        Assert.Empty(_appState.TodayTaskIdsWorkedOn);
    }

    #endregion

    #region CheckAndResetIfNeeded

    [Fact]
    public void CheckAndResetIfNeeded_WhenResetNeeded_ResetsStats()
    {
        _appState.LastResetDate = AppState.GetCurrentDayKey().AddDays(-1);
        _appState.TodayPomodoroCount = 5;

        var service = CreateService();
        service.CheckAndResetIfNeeded();

        Assert.Equal(0, _appState.TodayPomodoroCount);
        Assert.Equal(0, _appState.TodayTotalFocusMinutes);
    }

    [Fact]
    public void CheckAndResetIfNeeded_WhenNoResetNeeded_DoesNotReset()
    {
        _appState.LastResetDate = AppState.GetCurrentDayKey();
        _appState.TodayPomodoroCount = 5;
        _appState.TodayTotalFocusMinutes = 120;

        var service = CreateService();
        service.CheckAndResetIfNeeded();

        Assert.Equal(5, _appState.TodayPomodoroCount);
        Assert.Equal(120, _appState.TodayTotalFocusMinutes);
    }

    [Fact]
    public void CheckAndResetIfNeeded_WhenNeverReset_ResetsStats()
    {
        _appState.LastResetDate = null;
        _appState.TodayPomodoroCount = 3;

        var service = CreateService();
        service.CheckAndResetIfNeeded();

        Assert.Equal(0, _appState.TodayPomodoroCount);
    }

    #endregion

    #region RecordPomodoroCompletion

    [Fact]
    public void RecordPomodoroCompletion_IncrementsCountAndMinutes()
    {
        var taskId = Guid.NewGuid();
        var service = CreateService();

        service.RecordPomodoroCompletion(25, taskId);

        Assert.Equal(25, _appState.TodayTotalFocusMinutes);
        Assert.Equal(1, _appState.TodayPomodoroCount);
        Assert.Contains(taskId, _appState.TodayTaskIdsWorkedOn);
    }

    [Fact]
    public void RecordPomodoroCompletion_AccumulatesAcrossMultipleCompletions()
    {
        var task1 = Guid.NewGuid();
        var task2 = Guid.NewGuid();
        var service = CreateService();

        service.RecordPomodoroCompletion(25, task1);
        service.RecordPomodoroCompletion(25, task2);
        service.RecordPomodoroCompletion(5, task1);

        Assert.Equal(55, _appState.TodayTotalFocusMinutes);
        Assert.Equal(3, _appState.TodayPomodoroCount);
        Assert.Equal(2, _appState.TodayTaskIdsWorkedOn.Count);
    }

    [Fact]
    public void RecordPomodoroCompletion_DoesNotDuplicateTaskIds()
    {
        var taskId = Guid.NewGuid();
        var service = CreateService();

        service.RecordPomodoroCompletion(25, taskId);
        service.RecordPomodoroCompletion(25, taskId);

        Assert.Equal(2, _appState.TodayPomodoroCount);
        Assert.Single(_appState.TodayTaskIdsWorkedOn);
    }

    #endregion
}
