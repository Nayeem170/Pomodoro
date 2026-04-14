using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.TaskRepositoryTests;

/// <summary>
/// Operation tests for TaskRepository.
/// </summary>
public partial class TaskRepositoryTests
{
    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoTasks_ReturnsEmptyList()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(new List<TaskItem>());

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
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync((List<TaskItem>?)null!);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_FiltersDeletedTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            CreateSampleTask(id: Guid.Parse("00000000-0000-0000-0000-000000000001"), name: "Active Task", isDeleted: false),
            CreateSampleTask(id: Guid.Parse("00000000-0000-0000-0000-000000000002"), name: "Deleted Task", isDeleted: true),
            CreateSampleTask(id: Guid.Parse("00000000-0000-0000-0000-000000000003"), name: "Another Active Task", isDeleted: false),
        };

        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(tasks);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.False(t.IsDeleted));
    }

    [Fact]
    public async Task GetAllAsync_CallsIndexedDbWithCorrectStoreName()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(new List<TaskItem>());

        var repository = CreateRepository();

        // Act
        await repository.GetAllAsync();

        // Assert
        MockIndexedDb.Verify(
            x => x.GetAllAsync<TaskItem>(Constants.Storage.TasksStore),
            Times.Once);
    }

    #endregion

    #region GetAllIncludingDeletedAsync Tests

    [Fact]
    public async Task GetAllIncludingDeletedAsync_WithNoTasks_ReturnsEmptyList()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(new List<TaskItem>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllIncludingDeletedAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllIncludingDeletedAsync_IncludesDeletedTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            CreateSampleTask(id: Guid.Parse("00000000-0000-0000-0000-000000000001"), name: "Active Task", isDeleted: false),
            CreateSampleTask(id: Guid.Parse("00000000-0000-0000-0000-000000000002"), name: "Deleted Task", isDeleted: true),
        };

        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(tasks);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllIncludingDeletedAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllIncludingDeletedAsync_WithNullResult_ReturnsEmptyList()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync((List<TaskItem>?)null!);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllIncludingDeletedAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedTask = CreateSampleTask(id: taskId);

        MockIndexedDb
            .Setup(x => x.GetAsync<TaskItem>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedTask);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAsync<TaskItem>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TaskItem?)null);

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
        var taskId = Guid.NewGuid();
        MockIndexedDb
            .Setup(x => x.GetAsync<TaskItem>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TaskItem?)null);

        var repository = CreateRepository();

        // Act
        await repository.GetByIdAsync(taskId);

        // Assert
        MockIndexedDb.Verify(
            x => x.GetAsync<TaskItem>(Constants.Storage.TasksStore, taskId.ToString()),
            Times.Once);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WithValidTask_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<TaskItem>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        var result = await repository.SaveAsync(task);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SaveAsync_WhenIndexedDbFails_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<TaskItem>()))
            .ReturnsAsync(false);

        var repository = CreateRepository();

        // Act
        var result = await repository.SaveAsync(task);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SaveAsync_WhenIndexedDbFails_LogsWarning()
    {
        // Arrange
        var task = CreateSampleTask();

        MockIndexedDb
            .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<TaskItem>()))
            .ReturnsAsync(false);

        var repository = CreateRepository();

        // Act
        await repository.SaveAsync(task);

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(task.Id.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion


    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_WithNoTasks_ReturnsZero()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(new List<TaskItem>());

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCountAsync_WithTasks_ReturnsNonDeletedCount()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            CreateSampleTask(isDeleted: false),
            CreateSampleTask(isDeleted: true),
            CreateSampleTask(isDeleted: false),
        };

        MockIndexedDb
            .Setup(x => x.GetAllAsync<TaskItem>(It.IsAny<string>()))
            .ReturnsAsync(tasks);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(2, result);
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
        await repository.ClearAllAsync();

        // Assert
        MockIndexedDb.Verify(
            x => x.ClearAsync(Constants.Storage.TasksStore),
            Times.Once);
    }

    #endregion
}
