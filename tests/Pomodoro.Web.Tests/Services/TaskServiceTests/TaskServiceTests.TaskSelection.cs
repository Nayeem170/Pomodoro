using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService selection operations.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region SelectTaskAsync Tests

    [Fact]
    public async Task SelectTaskAsync_SetsCurrentTaskId()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.SelectTaskAsync(taskId);

        // Assert
        Assert.Equal(taskId, service.CurrentTaskId);
    }

    [Fact]
    public async Task SelectTaskAsync_WithCompletedTask_DoesNotSelect()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.SelectTaskAsync(taskId);

        // Assert
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task SelectTaskAsync_WithNonExistentTask_DoesNotSelect()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.SelectTaskAsync(Guid.NewGuid());

        // Assert
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task SelectTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.SelectTaskAsync(taskId);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task SelectTaskAsync_SetsCurrentTaskProperty()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Test Task", isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.SelectTaskAsync(taskId);

        // Assert
        Assert.NotNull(service.CurrentTask);
        Assert.Equal("Test Task", service.CurrentTask!.Name);
    }

    #endregion
}

