using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService CRUD operations.
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    #region AddActivityAsync Tests

    [Fact]
    public async Task AddActivityAsync_AddsToCache()
    {
        // Arrange
        var activities = new List<ActivityRecord>();
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var newActivity = CreateSampleActivity();

        // Act
        await service.AddActivityAsync(newActivity);

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Single(allActivities);
        Assert.Equal(newActivity.Id, allActivities[0].Id);
    }

    [Fact]
    public async Task AddActivityAsync_SavesToRepository()
    {
        // Arrange
        var activities = new List<ActivityRecord>();
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var newActivity = CreateSampleActivity();

        // Act
        await service.AddActivityAsync(newActivity);

        // Assert
        MockActivityRepository.Verify(r => r.SaveAsync(newActivity), Times.Once);
    }

    [Fact]
    public async Task AddActivityAsync_FiresOnActivityChangedEvent()
    {
        // Arrange
        var activities = new List<ActivityRecord>();
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnActivityChanged += () => eventFired = true;

        var newActivity = CreateSampleActivity();

        // Act
        await service.AddActivityAsync(newActivity);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task AddActivityAsync_TrimsCacheWhenExceedsMaxSize()
    {
        // Arrange - MaxActivityCacheSize is 500
        var existingActivities = Enumerable.Range(0, 500)
            .Select(i => CreateSampleActivity(completedAt: DateTime.UtcNow.AddMinutes(-i)))
            .ToList();

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingActivities);
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var newActivity = CreateSampleActivity(completedAt: DateTime.UtcNow);

        // Act
        await service.AddActivityAsync(newActivity);

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Equal(500, allActivities.Count); // Max cache size
        Assert.Equal(newActivity.Id, allActivities[0].Id); // Newest is first
    }

    [Fact]
    public async Task AddActivityAsync_AddsAtBeginningOfCache()
    {
        // Arrange
        var existingActivity = CreateSampleActivity(completedAt: DateTime.UtcNow.AddMinutes(-1));
        var activities = new List<ActivityRecord> { existingActivity };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var newActivity = CreateSampleActivity(completedAt: DateTime.UtcNow);

        // Act
        await service.AddActivityAsync(newActivity);

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Equal(2, allActivities.Count);
        Assert.Equal(newActivity.Id, allActivities[0].Id); // New activity is first
        Assert.Equal(existingActivity.Id, allActivities[1].Id); // Old activity is second
    }

    #endregion

    #region ClearAllActivitiesAsync Tests

    [Fact]
    public async Task ClearAllActivitiesAsync_ClearsCache()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(),
            CreateSampleActivity(),
            CreateSampleActivity()
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        MockActivityRepository.Setup(r => r.ClearAllAsync()).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ClearAllActivitiesAsync();

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Empty(allActivities);
    }

    [Fact]
    public async Task ClearAllActivitiesAsync_ClearsRepository()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        MockActivityRepository.Setup(r => r.ClearAllAsync()).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ClearAllActivitiesAsync();

        // Assert
        MockActivityRepository.Verify(r => r.ClearAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearAllActivitiesAsync_FiresOnActivityChangedEvent()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        MockActivityRepository.Setup(r => r.ClearAllAsync()).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnActivityChanged += () => eventFired = true;

        // Act
        await service.ClearAllActivitiesAsync();

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_LoadsCacheFromRepository()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(),
            CreateSampleActivity()
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Equal(2, allActivities.Count);
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledTwice_OnlyLoadsOnce()
    {
        // Arrange
        var activities = new List<ActivityRecord> { CreateSampleActivity() };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();

        // Act
        await service.InitializeAsync();
        await service.InitializeAsync();

        // Assert
        MockActivityRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyRepository_LoadsEmptyCache()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Empty(allActivities);
    }

    #endregion

    #region ReloadAsync Tests

    [Fact]
    public async Task ReloadAsync_ReloadsCacheFromRepository()
    {
        // Arrange
        var initialActivities = new List<ActivityRecord>
        {
            CreateSampleActivity()
        };

        var reloadedActivities = new List<ActivityRecord>
        {
            CreateSampleActivity(),
            CreateSampleActivity(),
            CreateSampleActivity()
        };

        MockActivityRepository.SetupSequence(r => r.GetAllAsync())
            .ReturnsAsync(initialActivities)
            .ReturnsAsync(reloadedActivities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ReloadAsync();

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Equal(3, allActivities.Count);
    }

    [Fact]
    public async Task ReloadAsync_FiresOnActivityChangedEvent()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnActivityChanged += () => eventFired = true;

        // Act
        await service.ReloadAsync();

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region HandleTimerCompletedAsync Tests

    [Fact]
    public async Task HandleTimerCompletedAsync_CreatesActivityRecord()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            Guid.NewGuid(),
            "Test Task",
            25,
            true,
            DateTime.UtcNow
        );

        // Act
        await service.HandleTimerCompletedAsync(args);

        // Assert
        var allActivities = service.GetAllActivities();
        Assert.Single(allActivities);

        var activity = allActivities[0];
        Assert.Equal(SessionType.Pomodoro, activity.Type);
        Assert.Equal(args.TaskId, activity.TaskId);
        Assert.Equal(args.TaskName, activity.TaskName);
        Assert.Equal(args.DurationMinutes, activity.DurationMinutes);
        Assert.True(activity.WasCompleted);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_SavesToRepository()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        MockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var args = new TimerCompletedEventArgs(
            SessionType.ShortBreak,
            null,
            null,
            5,
            true,
            DateTime.UtcNow
        );

        // Act
        await service.HandleTimerCompletedAsync(args);

        // Assert
        MockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    #endregion

    #region GetActivitiesByDateRangeAsync Tests

    [Fact]
    public async Task GetActivitiesByDateRangeAsync_ReturnsFromRepository()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var expectedActivities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-3))
        };

        MockActivityRepository.Setup(r => r.GetByDateRangeAsync(from, to))
            .ReturnsAsync(expectedActivities);

        var service = CreateService();

        // Act
        var result = await service.GetActivitiesByDateRangeAsync(from, to);

        // Assert
        Assert.Single(result);
        MockActivityRepository.Verify(r => r.GetByDateRangeAsync(from, to), Times.Once);
    }

    #endregion

    #region GetActivitiesPagedAsync Tests

    [Fact]
    public async Task GetActivitiesPagedAsync_ReturnsPagedResults()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var expectedActivities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-1)),
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-2))
        };

        MockActivityRepository.Setup(r => r.GetPagedAsync(from, to, 0, 20))
            .ReturnsAsync(expectedActivities);

        var service = CreateService();

        // Act
        var result = await service.GetActivitiesPagedAsync(from, to, 0, 20);

        // Assert
        Assert.Equal(2, result.Count);
        MockActivityRepository.Verify(r => r.GetPagedAsync(from, to, 0, 20), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesPagedAsync_WithCustomPaging_ReturnsCorrectPage()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var expectedActivities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-3))
        };

        MockActivityRepository.Setup(r => r.GetPagedAsync(from, to, 10, 5))
            .ReturnsAsync(expectedActivities);

        var service = CreateService();

        // Act
        var result = await service.GetActivitiesPagedAsync(from, to, 10, 5);

        // Assert
        MockActivityRepository.Verify(r => r.GetPagedAsync(from, to, 10, 5), Times.Once);
    }

    #endregion

    #region GetActivityCountAsync Tests

    [Fact]
    public async Task GetActivityCountAsync_ReturnsCountFromRepository()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetCountAsync(null, null))
            .ReturnsAsync(42);

        var service = CreateService();

        // Act
        var count = await service.GetActivityCountAsync();

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public async Task GetActivityCountAsync_WithDateRange_PassesDatesToRepository()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        MockActivityRepository.Setup(r => r.GetCountAsync(from, to))
            .ReturnsAsync(10);

        var service = CreateService();

        // Act
        var count = await service.GetActivityCountAsync(from, to);

        // Assert
        Assert.Equal(10, count);
        MockActivityRepository.Verify(r => r.GetCountAsync(from, to), Times.Once);
    }

    #endregion
}

