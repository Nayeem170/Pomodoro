using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ActivityRepositoryTests;

/// <summary>
/// Query operation tests for ActivityRepository.
/// </summary>
public partial class ActivityRepositoryTests
{
    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_WithNoActivities_ReturnsEmptyList()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByDateRangeAsync(start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithNullResult_ReturnsEmptyList()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((List<ActivityRecord>?)null!);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByDateRangeAsync(start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithActivities_ReturnsOrderedByCompletedAtDescending()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var baseDate = DateTime.UtcNow;

        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: baseDate.AddDays(-1)),
            CreateSampleActivity(completedAt: baseDate),
            CreateSampleActivity(completedAt: baseDate.AddDays(-2)),
        };

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByDateRangeAsync(start, end);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].CompletedAt >= result[1].CompletedAt);
        Assert.True(result[1].CompletedAt >= result[2].CompletedAt);
    }

    [Fact]
    public async Task GetByDateRangeAsync_CallsIndexedDbWithCorrectParameters()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetByDateRangeAsync(start, end);

        // Assert
        MockIndexedDb.Verify(
            x => x.QueryByDateRangeAsync<ActivityRecord>(
                Constants.Storage.ActivitiesStore,
                Constants.Storage.CompletedAtIndex,
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByDateRangeAsync_FormatsDatesInIsoFormat()
    {
        // Arrange
        var start = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var end = new DateTime(2024, 1, 20, 15, 45, 0, DateTimeKind.Utc);
        string? actualStart = null;
        string? actualEnd = null;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string>((_, _, s, e) =>
            {
                actualStart = s;
                actualEnd = e;
            })
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetByDateRangeAsync(start, end);

        // Assert
        Assert.NotNull(actualStart);
        Assert.NotNull(actualEnd);
        Assert.StartsWith("2024-01-15", actualStart);
        Assert.StartsWith("2024-01-20", actualEnd);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithLocalKind_ConvertsToUtcBeforeQuery()
    {
        // Arrange
        var start = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Local);
        var end = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Local);
        string? actualStart = null;
        string? actualEnd = null;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string>((_, _, s, e) =>
            {
                actualStart = s;
                actualEnd = e;
            })
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetByDateRangeAsync(start, end);

        // Assert
        Assert.NotNull(actualStart);
        Assert.NotNull(actualEnd);
        var expectedStart = start.ToUniversalTime();
        var expectedEnd = end.ToUniversalTime();
        Assert.Equal(expectedStart.ToString("O"), actualStart);
        Assert.Equal(expectedEnd.ToString("O"), actualEnd);
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_WithNoActivities_ReturnsEmptyList()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetPagedAsync(start, end, 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPagedAsync_WithActivities_ReturnsPagedResults()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var baseDate = DateTime.UtcNow;

        var activities = new List<ActivityRecord>();
        for (int i = 0; i < 25; i++)
        {
            activities.Add(CreateSampleActivity(completedAt: baseDate.AddDays(-i)));
        }

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetPagedAsync(start, end, 5, 10);

        // Assert
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task GetPagedAsync_WithSkipZero_ReturnsFirstPage()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var baseDate = DateTime.UtcNow;

        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(id: Guid.Parse("00000000-0000-0000-0000-000000000001"), completedAt: baseDate),
            CreateSampleActivity(id: Guid.Parse("00000000-0000-0000-0000-000000000002"), completedAt: baseDate.AddDays(-1)),
            CreateSampleActivity(id: Guid.Parse("00000000-0000-0000-0000-000000000003"), completedAt: baseDate.AddDays(-2)),
        };

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetPagedAsync(start, end, 0, 2);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetPagedAsync_WithTakeLargerThanAvailable_ReturnsAllRemaining()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var baseDate = DateTime.UtcNow;

        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: baseDate),
            CreateSampleActivity(completedAt: baseDate.AddDays(-1)),
            CreateSampleActivity(completedAt: baseDate.AddDays(-2)),
        };

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetPagedAsync(start, end, 2, 10);

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_WithNoActivities_ReturnsZero()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCountAsync_WithoutDateRange_ReturnsTotalCount()
    {
        // Arrange
        var activities = CreateSampleActivities(5);

        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetCountAsync_WithDateRange_ReturnsFilteredCount()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var activities = CreateSampleActivities(3);

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync(start, end);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetCountAsync_WithDateRange_CallsQueryByDateRange()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        MockIndexedDb
            .Setup(x => x.QueryByDateRangeAsync<ActivityRecord>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetCountAsync(start, end);

        // Assert
        MockIndexedDb.Verify(
            x => x.QueryByDateRangeAsync<ActivityRecord>(
                Constants.Storage.ActivitiesStore,
                Constants.Storage.CompletedAtIndex,
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_WithoutDateRange_CallsGetAll()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(new List<ActivityRecord>());

        var repository = CreateRepository();

        // Act
        await repository.GetCountAsync();

        // Assert
        MockIndexedDb.Verify(
            x => x.GetAllAsync<ActivityRecord>(Constants.Storage.ActivitiesStore),
            Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_WithOnlyStartDate_ReturnsTotalCount()
    {
        // Arrange
        var activities = CreateSampleActivities(4);

        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync(DateTime.UtcNow.AddDays(-7), null);

        // Assert
        Assert.Equal(4, result);
    }

    [Fact]
    public async Task GetCountAsync_WithOnlyEndDate_ReturnsTotalCount()
    {
        // Arrange
        var activities = CreateSampleActivities(4);

        MockIndexedDb
            .Setup(x => x.GetAllAsync<ActivityRecord>(It.IsAny<string>()))
            .ReturnsAsync(activities);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync(null, DateTime.UtcNow);

        // Assert
        Assert.Equal(4, result);
    }

    #endregion
}
