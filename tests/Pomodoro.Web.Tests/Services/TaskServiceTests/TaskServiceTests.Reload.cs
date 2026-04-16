using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService reload operations.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region ReloadAsync Tests

    [Fact]
    public async Task ReloadAsync_ReloadsTasksFromRepository()
    {
        // Arrange
        var initialTasks = new List<TaskItem> { CreateSampleTask() };
        var reloadedTasks = new List<TaskItem>
        {
            CreateSampleTask(),
            CreateSampleTask(),
            CreateSampleTask()
        };

        MockTaskRepository.SetupSequence(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(initialTasks)
            .ReturnsAsync(reloadedTasks);

        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ReloadAsync();

        // Assert
        Assert.Equal(3, service.Tasks.Count);
    }

    [Fact]
    public async Task ReloadAsync_ClearsCurrentTaskIfNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var initialTasks = new List<TaskItem> { CreateSampleTask(id: taskId) };
        var reloadedTasks = new List<TaskItem> { CreateSampleTask() }; // Different task

        MockTaskRepository.SetupSequence(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(initialTasks)
            .ReturnsAsync(reloadedTasks);

        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AppStateRecord { CurrentTaskId = taskId });

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ReloadAsync();

        // Assert
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task ReloadAsync_WithNullTasks_LoadsEmptyList()
    {
        // Arrange
        var initialTasks = new List<TaskItem> { CreateSampleTask() };

        MockTaskRepository.SetupSequence(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(initialTasks)
            .ReturnsAsync((List<TaskItem>?)null);

        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.ReloadAsync();

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task ReloadAsync_FiresOnChangeEvent()
    {
        // Arrange
        var initialTasks = new List<TaskItem> { CreateSampleTask() };
        var reloadedTasks = new List<TaskItem> { CreateSampleTask(), CreateSampleTask() };

        MockTaskRepository.SetupSequence(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(initialTasks)
            .ReturnsAsync(reloadedTasks);

        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.ReloadAsync();

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_SavesAllTasksToIndexedDb()
    {
        // Arrange
        var task1 = CreateSampleTask();
        var task2 = CreateSampleTask();

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task1, task2 });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>())).ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.SaveAsync();

        // Assert
        MockIndexedDb.Verify(d => d.PutAllAsync(Constants.Storage.TasksStore, It.IsAny<List<TaskItem>>()), Times.Once);
    }

    #endregion
}

