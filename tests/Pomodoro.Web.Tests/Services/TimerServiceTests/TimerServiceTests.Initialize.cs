using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService initialization.
/// </summary>
public partial class TimerServiceTests
{
    [Fact]
    public async Task InitializeAsync_LoadsSettingsFromRepository()
    {
        // Arrange
        var settings = new TimerSettings { PomodoroMinutes = 30, ShortBreakMinutes = 10, LongBreakMinutes = 20 };
        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync(settings);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((DailyStats?)null);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        MockSettingsRepository.Verify(x => x.GetAsync(), Times.Once);
        Assert.Equal(30, AppState.Settings.PomodoroMinutes);
    }

    [Fact]
    public async Task InitializeAsync_CreatesDefaultSession_WhenNoSessionExists()
    {
        // Arrange
        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync((TimerSettings?)null);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((DailyStats?)null);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.NotNull(AppState.CurrentSession);
        Assert.Equal(SessionType.Pomodoro, AppState.CurrentSession.Type);
        Assert.False(AppState.CurrentSession.IsRunning);
    }

    [Fact]
    public async Task InitializeAsync_RestoresTodayStats_WhenStatsAreFromToday()
    {
        // Arrange
        var todayKey = AppState.GetCurrentDayKey();
        var dailyStats = new DailyStats
        {
            Date = todayKey,
            TotalFocusMinutes = 45,
            PomodoroCount = 3,
            TaskIdsWorkedOn = new List<Guid> { Guid.NewGuid() }
        };

        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync((TimerSettings?)null);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dailyStats);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Equal(45, AppState.TodayTotalFocusMinutes);
        Assert.Equal(3, AppState.TodayPomodoroCount);
        Assert.Single(AppState.TodayTaskIdsWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_ResetsStats_WhenStatsAreFromPreviousDay()
    {
        // Arrange
        var previousDayKey = AppState.GetCurrentDayKey().AddDays(-1);
        var dailyStats = new DailyStats
        {
            Date = previousDayKey,
            TotalFocusMinutes = 120,
            PomodoroCount = 8,
            TaskIdsWorkedOn = new List<Guid> { Guid.NewGuid() }
        };

        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync((TimerSettings?)null);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dailyStats);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Equal(0, AppState.TodayTotalFocusMinutes);
        Assert.Equal(0, AppState.TodayPomodoroCount);
        Assert.Empty(AppState.TodayTaskIdsWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_InitializesJsConstants()
    {
        // Arrange
        var settings = new TimerSettings { PomodoroMinutes = 25, ShortBreakMinutes = 5, LongBreakMinutes = 15 };
        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync(settings);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((DailyStats?)null);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        MockIndexedDb.Verify(x => x.InitializeJsConstantsAsync(25, 5, 15), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_UsesDefaultSettings_WhenRepositoryReturnsNull()
    {
        // Arrange
        MockSettingsRepository.Setup(x => x.GetAsync()).ReturnsAsync((TimerSettings?)null);
        MockIndexedDb.Setup(x => x.GetAsync<DailyStats>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((DailyStats?)null);
        MockIndexedDb.Setup(x => x.InitializeJsConstantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert - Should use AppState's default settings
        Assert.Equal(25, AppState.Settings.PomodoroMinutes); // Default value
    }
}
