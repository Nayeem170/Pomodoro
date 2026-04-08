using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService edge cases and error handling.
/// </summary>
public partial class TaskServiceTests
{
    #region Edge Case Tests - Non-Existent Task Operations

    [Fact]
    public async Task InitializeAsync_WithValidCurrentTaskId_SetsCurrentTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AppStateRecord { CurrentTaskId = taskId });
        
        var service = CreateService();
        
        // Act
        await service.InitializeAsync();
        
        // Assert - CurrentTaskId should be set because task exists in list
        Assert.Equal(taskId, service.CurrentTaskId);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNonExistentTask_DoesNotUpdate()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.UpdateTaskAsync(new TaskItem { Id = nonExistentTaskId, Name = "Non-existent Task" });
        
        // Assert - Should not save to repository since task doesn't exist
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithNonExistentTask_DoesNothing()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.DeleteTaskAsync(nonExistentTaskId);
        
        // Assert - Should not save to repository since task doesn't exist
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task CompleteTaskAsync_WithNonExistentTask_DoesNothing()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.CompleteTaskAsync(nonExistentTaskId);
        
        // Assert - Should not save to repository since task doesn't exist
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UncompleteTaskAsync_WithNonExistentTask_DoesNothing()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.UncompleteTaskAsync(nonExistentTaskId);
        
        // Assert - Should not save to repository since task doesn't exist
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_WithNonExistentTask_DoesNotUpdate()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var existingTask = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { existingTask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.AddTimeToTaskAsync(nonExistentTaskId, 25);
        
        // Assert - Should not save to repository since task doesn't exist
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNullName_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act - Pass task with empty name (simulating null input that gets sanitized)
        await service.UpdateTaskAsync(new TaskItem { Id = taskId, Name = string.Empty });
        
        // Assert - Should not save to repository since name is null (sanitized to empty)
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        Assert.Equal("Original Name", service.AllTasks[0].Name);
    }

    #endregion
}
