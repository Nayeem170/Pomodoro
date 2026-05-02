using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService.InitializeAsync operations.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_LoadsTasksFromRepository()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            CreateSampleTask(),
            CreateSampleTask()
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(tasks);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Equal(2, service.Tasks.Count);
    }

    [Fact]
    public async Task InitializeAsync_WithNoAppStateRecord_LoadsTasksOnly()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            CreateSampleTask(id: taskId)
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(tasks);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Single(service.Tasks);
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyRepository_LoadsEmptyList()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task InitializeAsync_WithNullTasksFromRepository_LoadsEmptyList()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync((List<TaskItem>?)null);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task InitializeAsync_WithNonExistentCurrentTaskId_ClearsCurrentTask()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AppStateRecord { CurrentTaskId = nonExistentTaskId });

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task InitializeAsync_FiresOnChangeEvent()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task InitializeAsync_WithDueRecurringTask_ActivatesTask()
    {
        var task = CreateSampleTask();
        task.IsCompleted = true;
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = false, LastCompletedDate = DateTime.UtcNow.Date.AddDays(-1) };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.False(task.IsCompleted);
    }

    [Fact]
    public async Task InitializeAsync_WithRecurringTaskNullLastCompleted_ComputesNextOccurrence()
    {
        var task = CreateSampleTask();
        task.IsCompleted = true;
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = false, LastCompletedDate = null };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.True(task.IsCompleted);
        Assert.NotNull(task.Repeat.NextOccurrence);
    }

    #endregion
}

