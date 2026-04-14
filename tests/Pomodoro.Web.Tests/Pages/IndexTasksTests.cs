using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Xunit;
using Moq;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for Index page task actions in Index.razor.Tasks.cs
/// Tests all task-related event handlers
/// </summary>
public class IndexTasksTests : TestHelper
{
    public IndexTasksTests()
    {
        // Set up TaskService with default settings
        var defaultSettings = new TimerSettings();
        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(new List<TaskItem>());
        TaskServiceMock
            .SetupGet(x => x.AllTasks)
            .Returns(new List<TaskItem>());
        TaskServiceMock
            .SetupGet(x => x.CurrentTaskId)
            .Returns((Guid?)null);
        TaskServiceMock
            .SetupGet(x => x.CurrentTask)
            .Returns((TaskItem?)null);
    }

    #region HandleTaskAdd

    [Fact]
    public async Task HandleTaskAdd_CallsTaskServiceAddTaskAsync_WhenCalled()
    {
        // Arrange
        var taskName = "Test Task";
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskAdd(taskName);

        // Assert
        TaskServiceMock.Verify(
            x => x.AddTaskAsync(taskName),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskAdd_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskName = "Test Task";
        TaskServiceMock
            .Setup(x => x.AddTaskAsync(taskName))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskAdd(taskName);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error adding task: Test exception");
    }

    #endregion

    #region HandleTaskSelect

    [Fact]
    public async Task HandleTaskSelect_CallsTaskServiceSelectTaskAsync_WhenCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        TaskServiceMock.Verify(
            x => x.SelectTaskAsync(taskId),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.SelectTaskAsync(taskId))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error selecting task: Test exception");
    }

    #endregion

    #region HandleTaskComplete

    [Fact]
    public async Task HandleTaskComplete_CallsTaskServiceCompleteTaskAsync_WhenCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskComplete(taskId);

        // Assert
        TaskServiceMock.Verify(
            x => x.CompleteTaskAsync(taskId),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskComplete_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.CompleteTaskAsync(taskId))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskComplete(taskId);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error completing task: Test exception");
    }

    #endregion

    #region HandleTaskDelete

    [Fact]
    public async Task HandleTaskDelete_CallsTaskServiceDeleteTaskAsync_WhenCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskDelete(taskId);

        // Assert
        TaskServiceMock.Verify(
            x => x.DeleteTaskAsync(taskId),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskDelete_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.DeleteTaskAsync(taskId))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskDelete(taskId);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error deleting task: Test exception");
    }

    #endregion

    #region HandleTaskUncomplete

    [Fact]
    public async Task HandleTaskUncomplete_CallsTaskServiceUncompleteTaskAsync_WhenCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskUncomplete(taskId);

        // Assert
        TaskServiceMock.Verify(
            x => x.UncompleteTaskAsync(taskId),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskUncomplete_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.UncompleteTaskAsync(taskId))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskUncomplete(taskId);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error uncompleting task: Test exception");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task HandleTaskAdd_SetsErrorMessage_WhenEmptyTaskName()
    {
        // Arrange
        string taskName = string.Empty;
        TaskServiceMock
            .Setup(x => x.AddTaskAsync(taskName))
            .ThrowsAsync(new ArgumentException("Task name cannot be empty", nameof(taskName)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskAdd(taskName);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error adding task: Task name cannot be empty (Parameter 'taskName')");
    }

    [Fact]
    public async Task HandleTaskSelect_SetsErrorMessage_WhenNullTaskId()
    {
        // Arrange
        Guid taskId = Guid.Empty;
        TaskServiceMock
            .Setup(x => x.SelectTaskAsync(taskId))
            .ThrowsAsync(new ArgumentException("Task ID cannot be empty", nameof(taskId)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(Guid.Empty);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error selecting task: Task ID cannot be empty (Parameter 'taskId')");
    }

    [Fact]
    public async Task HandleTaskComplete_SetsErrorMessage_WhenNullTaskId()
    {
        // Arrange
        Guid taskId = Guid.Empty;
        TaskServiceMock
            .Setup(x => x.CompleteTaskAsync(taskId))
            .ThrowsAsync(new ArgumentException("Task ID cannot be empty", nameof(taskId)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskComplete(Guid.Empty);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error completing task: Task ID cannot be empty (Parameter 'taskId')");
    }

    [Fact]
    public async Task HandleTaskDelete_SetsErrorMessage_WhenNullTaskId()
    {
        // Arrange
        Guid taskId = Guid.Empty;
        TaskServiceMock
            .Setup(x => x.DeleteTaskAsync(taskId))
            .ThrowsAsync(new ArgumentException("Task ID cannot be empty", nameof(taskId)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskDelete(Guid.Empty);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error deleting task: Task ID cannot be empty (Parameter 'taskId')");
    }

    [Fact]
    public async Task HandleTaskUncomplete_SetsErrorMessage_WhenNullTaskId()
    {
        // Arrange
        Guid taskId = Guid.Empty;
        TaskServiceMock
            .Setup(x => x.UncompleteTaskAsync(taskId))
            .ThrowsAsync(new ArgumentException("Task ID cannot be empty", nameof(taskId)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskUncomplete(Guid.Empty);

        // Assert
        cut.Instance.ErrorMessage.Should().Be($"Error uncompleting task: Task ID cannot be empty (Parameter 'taskId')");
    }

    #endregion
}
