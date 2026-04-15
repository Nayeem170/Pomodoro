using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ActivityRepositoryTests;

/// <summary>
/// CRUD operation tests for ActivityRepository.
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityRepositoryTests
{
    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoActivities_ReturnsEmptyList()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithNullResult_ReturnsEmptyList()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync((List<ActivityRecord>?)null!);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithActivities_ReturnsOrderedByCompletedAtDescending()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-1)),
            CreateSampleActivity(completedAt: DateTime.UtcNow),
            CreateSampleActivity(completedAt: DateTime.UtcNow.AddDays(-2)),
        };

        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].CompletedAt >= result[1].CompletedAt);
        Assert.True(result[1].CompletedAt >= result[2].CompletedAt);
    }

    [Fact]
    public async Task GetAllAsync_CallsIndexedDbWithCorrectStoreName()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetAllAsync();

        // Assert
        MockIndexedDb.Verify(
            x => x.GetAllAsync<ActivityRecord>(Constants.Storage.ActivitiesStore),
            Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsActivity()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var expectedActivity = CreateSampleActivity(id: activityId);

        MockIndexedDb
            .Setup(x => x.GetAsync<ActivityRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedActivity);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(activityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activityId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAsync<ActivityRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((ActivityRecord?)null);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_CallsIndexedDbWithCorrectParameters()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        MockIndexedDb
            .Setup(x => x.GetAsync<ActivityRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((ActivityRecord?)null);

        var repository = CreateRepository();

        // Act
        await repository.GetByIdAsync(activityId);

        // Assert
        MockIndexedDb.Verify(
            x => x.GetAsync<ActivityRecord>(Constants.Storage.ActivitiesStore, activityId.ToString()),
            Times.Once);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WithValidActivity_ReturnsTrue()
    {
        // Arrange
        var activity = CreateSampleActivity();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<ActivityRecord>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        var result = await repository.SaveAsync(activity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SaveAsync_WhenIndexedDbFails_ReturnsFalse()
    {
        // Arrange
        var activity = CreateSampleActivity();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<ActivityRecord>()))
            .ReturnsAsync(false);

        var repository = CreateRepository();

        // Act
        var result = await repository.SaveAsync(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SaveAsync_WhenIndexedDbFails_LogsWarning()
    {
        // Arrange
        var activity = CreateSampleActivity();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<ActivityRecord>()))
            .ReturnsAsync(false);

        var repository = CreateRepository();

        // Act
        await repository.SaveAsync(activity);

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(activity.Id.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveAsync_CallsIndexedDbWithCorrectStoreName()
    {
        // Arrange
        var activity = CreateSampleActivity();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<ActivityRecord>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.SaveAsync(activity);

        // Assert
        MockIndexedDb.Verify(
            x => x.PutAsync(Constants.Storage.ActivitiesStore, activity),
            Times.Once);
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_CallsIndexedDbClear()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.ClearAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        var result = await repository.ClearAllAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ClearAllAsync_CallsIndexedDbWithCorrectStoreName()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.ClearAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.ClearAllAsync();

        // Assert
        MockIndexedDb.Verify(
            x => x.ClearAsync(Constants.Storage.ActivitiesStore),
            Times.Once);
    }

    #endregion
}

