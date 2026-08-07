using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService completion operations.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region CompleteTaskAsync Tests

    [Fact]
    public async Task CompleteTaskAsync_MarksTaskAsCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.CompleteTaskAsync(taskId);

        // Assert
        Assert.True(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task CompleteTaskAsync_SavesToRepository()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.CompleteTaskAsync(taskId);

        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    #endregion

    #region UncompleteTaskAsync Tests

    [Fact]
    public async Task UncompleteTaskAsync_MarksTaskAsNotCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.UncompleteTaskAsync(taskId);

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task UncompleteTaskAsync_SavesToRepository()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.UncompleteTaskAsync(taskId);

        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task UncompleteTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.UncompleteTaskAsync(taskId);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task CompleteTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.CompleteTaskAsync(taskId);

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region Subtask Completion Guard Tests

    [Fact]
    public async Task CompleteTaskAsync_WithIncompleteSubtask_ThrowsInvalidOperationException()
    {
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, isCompleted: false);
        var child = CreateSampleTask(isCompleted: false);
        child.ParentTaskId = parentId;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { parent, child });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.CompleteTaskAsync(parentId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(Constants.Messages.CompleteSubtasksFirst);
    }

    [Fact]
    public async Task CompleteTaskAsync_WithAllSubtasksComplete_Succeeds()
    {
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, isCompleted: false);
        var child = CreateSampleTask(isCompleted: true);
        child.ParentTaskId = parentId;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { parent, child });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.CompleteTaskAsync(parentId);

        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithNoSubtasks_Succeeds()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.CompleteTaskAsync(taskId);

        service.AllTasks[0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithDeletedIncompleteSubtask_Succeeds()
    {
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, isCompleted: false);
        var child = CreateSampleTask(isCompleted: false);
        child.ParentTaskId = parentId;
        child.IsDeleted = true;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { parent, child });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.CompleteTaskAsync(parentId);

        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeTrue();
    }

    #endregion
}

