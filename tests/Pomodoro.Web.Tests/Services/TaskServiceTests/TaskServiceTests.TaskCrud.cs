using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TaskService CRUD operations (Add, Update, Delete).
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    #region AddTaskAsync Tests

    [Fact]
    public async Task AddTaskAsync_AddsTaskToList()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("New Task");

        // Assert
        Assert.Single(service.Tasks);
        Assert.Equal("New Task", service.Tasks[0].Name);
    }

    [Fact]
    public async Task AddTaskAsync_SetsCurrentTaskId()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("New Task");

        // Assert
        Assert.NotNull(service.CurrentTaskId);
        Assert.Equal(service.Tasks[0].Id, service.CurrentTaskId);
    }

    [Fact]
    public async Task AddTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        await service.AddTaskAsync("New Task");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task AddTaskAsync_WithEmptyName_DoesNotAddTask()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("");

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task AddTaskAsync_WithWhitespaceOnly_DoesNotAddTask()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("   ");

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task AddTaskAsync_SavesToRepository()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("New Task");

        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTaskName()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        task.Name = "Updated Name";
        await service.UpdateTaskAsync(task);

        // Assert
        Assert.Equal("Updated Name", service.Tasks[0].Name);
    }

    [Fact]
    public async Task UpdateTaskAsync_SavesToRepository()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        task.Name = "Updated Name";
        await service.UpdateTaskAsync(task);

        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithEmptyName_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        task.Name = "";
        await service.UpdateTaskAsync(task);

        // Assert
        // Name should remain unchanged since empty name is invalid
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTaskAsync_WithNameExceedingMaxLength_DoesNotAddTask()
    {
        // Arrange
        var longName = new string('a', Constants.UI.MaxTaskNameLength + 1);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync(longName);

        // Assert
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNameExceedingMaxLength_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");
        var longName = new string('a', Constants.UI.MaxTaskNameLength + 1);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        task.Name = longName;
        await service.UpdateTaskAsync(task);

        // Assert
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTaskAsync_HtmlEncodesTaskName()
    {
        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem>());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.AddTaskAsync("<script>alert('xss')</script>");

        // Assert
        Assert.Single(service.Tasks);
        Assert.DoesNotContain("<script>", service.Tasks[0].Name);
        Assert.Contains("&lt;", service.Tasks[0].Name);
    }

    [Fact]
    public async Task UpdateTaskAsync_PreservesTaskNameAsIs()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        task.Name = "<b>Bold</b>";
        await service.UpdateTaskAsync(task);

        Assert.Equal("<b>Bold</b>", service.Tasks[0].Name);
    }

    [Fact]
    public async Task UpdateTaskAsync_FiresOnChangeEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var eventFired = false;
        service.OnChange += () => eventFired = true;

        // Act
        task.Name = "Updated Name";
        await service.UpdateTaskAsync(task);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNullNameProperty_DoesNotUpdate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, name: "Original Name");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act - Use reflection to set Name to null (simulating deserialization scenarios)
        var nameProperty = typeof(TaskItem).GetProperty("Name");
        nameProperty?.SetValue(task, null);
        await service.UpdateTaskAsync(task);

        // Assert
        // Name should remain unchanged since null name is invalid
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_MarksTaskAsDeleted()
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
        await service.DeleteTaskAsync(taskId);

        // Assert - Task should be soft deleted (not in Tasks but in AllTasks)
        Assert.Empty(service.Tasks);
        Assert.Single(service.AllTasks);
        Assert.True(service.AllTasks[0].IsDeleted);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenCurrentTask_ClearsCurrentTaskId()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AppStateRecord { CurrentTaskId = taskId });

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.DeleteTaskAsync(taskId);

        // Assert
        Assert.Null(service.CurrentTaskId);
    }

    [Fact]
    public async Task DeleteTaskAsync_FiresOnChangeEvent()
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
        await service.DeleteTaskAsync(taskId);

        // Assert
        Assert.True(eventFired);
    }

    #endregion
}

