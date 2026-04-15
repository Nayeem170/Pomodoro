using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService time tracking operations.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region AddTimeToTaskAsync Tests

    [Fact]
    public async Task AddTimeToTaskAsync_AddsTimeToTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.AddTimeToTaskAsync(taskId, 25);
        
        // Assert
        Assert.Equal(25, service.AllTasks[0].TotalFocusMinutes);
        Assert.Equal(1, service.AllTasks[0].PomodoroCount);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_WithZeroMinutes_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.AddTimeToTaskAsync(taskId, 0);
        
        // Assert
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_WithNegativeMinutes_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.AddTimeToTaskAsync(taskId, -10);
        
        // Assert
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_UpdatesLastWorkedOn()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        var beforeTime = DateTime.UtcNow;
        
        // Act
        await service.AddTimeToTaskAsync(taskId, 25);
        
        // Assert
        Assert.NotNull(service.AllTasks[0].LastWorkedOn);
        Assert.True(service.AllTasks[0].LastWorkedOn >= beforeTime);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        var eventFired = false;
        service.OnChange += () => eventFired = true;
        
        // Act
        await service.AddTimeToTaskAsync(taskId, 25);
        
        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_SavesToRepository()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        // Act
        await service.AddTimeToTaskAsync(taskId, 25);
        
        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    #endregion

    #region HandleTimerCompletedAsync Tests

    [Fact]
    public async Task HandleTimerCompletedAsync_WithPomodoro_AddsTimeToTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            taskId,
            "Test Task",
            25,
            true,
            DateTime.UtcNow
        );
        
        // Act
        await service.HandleTimerCompletedAsync(args);
        
        // Assert
        Assert.Equal(25, service.AllTasks[0].TotalFocusMinutes);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WithBreak_DoesNotAddTime()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        var args = new TimerCompletedEventArgs(
            SessionType.ShortBreak,
            taskId,
            null,
            5,
            true,
            DateTime.UtcNow
        );
        
        // Act
        await service.HandleTimerCompletedAsync(args);
        
        // Assert
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WithoutTaskId_DoesNotAddTime()
    {
        // Arrange
        var task = CreateSampleTask();
        
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        
        var service = CreateService();
        await service.InitializeAsync();
        
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            null,
            null,
            25,
            true,
            DateTime.UtcNow
        );
        
        // Act
        await service.HandleTimerCompletedAsync(args);
        
        // Assert
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
    }

    #endregion
}

