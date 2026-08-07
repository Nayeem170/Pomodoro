using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
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
[Trait("Category", "Page")]
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
        await cut.Instance.HandleTaskAdd(new NewTaskRequest(taskName));

        // Assert
        TaskServiceMock.Verify(
            x => x.AddTaskAsync(taskName, It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleTaskAdd_SetsErrorMessage_WhenExceptionThrown()
    {
        // Arrange
        var taskName = "Test Task";
        TaskServiceMock
            .Setup(x => x.AddTaskAsync(taskName, It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Test exception"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskAdd(new NewTaskRequest(taskName));

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

    [Fact]
    public async Task HandleTaskComplete_ShowsErrorToast_WhenSubtasksIncomplete()
    {
        var taskId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.CompleteTaskAsync(taskId))
            .ThrowsAsync(new InvalidOperationException(Constants.Messages.CompleteSubtasksFirst));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTaskComplete(taskId));
        cut.Render();

        cut.Instance.ErrorMessage.Should().Be(Constants.Messages.CompleteSubtasksFirst);
        cut.Markup.Should().Contain("error-toast");
        cut.Markup.Should().Contain(Constants.Messages.CompleteSubtasksFirst);
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
            .Setup(x => x.AddTaskAsync(taskName, It.IsAny<string?>()))
            .ThrowsAsync(new ArgumentException("Task name cannot be empty", nameof(taskName)));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskAdd(new NewTaskRequest(taskName));

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
    public async Task HandleTaskSelect_WhenTimerRunningAndSettingOn_RecordsPartialAndRestarts()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        var appState = Services.GetRequiredService<AppState>();
        appState.Settings.RecordPartialSessions = true;
        TimerServiceMock.Setup(x => x.TryRecordPartialSessionAsync()).ReturnsAsync(true);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Once);
        TimerServiceMock.Verify(x => x.PauseAsync(), Times.Once);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenTimerNotRunning_DoesNotRecordPartial()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
        var appState = Services.GetRequiredService<AppState>();
        appState.Settings.RecordPartialSessions = true;

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenSettingOff_DoesNotRecordPartial()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        var appState = Services.GetRequiredService<AppState>();
        appState.Settings.RecordPartialSessions = false;

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenBreakSession_DoesNotRecordPartial()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.ShortBreak);
        var appState = Services.GetRequiredService<AppState>();
        appState.Settings.RecordPartialSessions = true;

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleTaskSelect(taskId);

        // Assert
        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
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

    #region HandleTaskAdd With Repeat/Schedule

    [Fact]
    public async Task HandleTaskAdd_SetsRepeatRule_WhenRepeatTypeSpecified()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "New Task" };
        AppState.Tasks = new List<TaskItem> { task };
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var request = new NewTaskRequest("New Task", RepeatType.Daily, DateTime.Now);
        await cut.Instance.HandleTaskAdd(request);

        task.Repeat.Should().NotBeNull();
        task.Repeat!.Type.Should().Be(RepeatType.Daily);
        TaskServiceMock.Verify(x => x.UpdateTaskAsync(task), Times.Once);
    }

    [Fact]
    public async Task HandleTaskAdd_SetsScheduledDate_WhenScheduledDateSpecified()
    {
        var taskId = Guid.NewGuid();
        var scheduledDate = new DateTime(2026, 1, 15);
        var task = new TaskItem { Id = taskId, Name = "Scheduled Task" };
        AppState.Tasks = new List<TaskItem> { task };
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var request = new NewTaskRequest("Scheduled Task", ScheduledDate: scheduledDate);
        await cut.Instance.HandleTaskAdd(request);

        task.ScheduledDate.Should().Be(scheduledDate);
        TaskServiceMock.Verify(x => x.UpdateTaskAsync(task), Times.Once);
    }

    [Fact]
    public async Task HandleTaskAdd_SetsErrorMessage_WhenUnauthorizedAccessException()
    {
        TaskServiceMock
            .Setup(x => x.AddTaskAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new UnauthorizedAccessException("Auth failed"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.Instance.HandleTaskAdd(new NewTaskRequest("Test"));

        cut.Instance.ErrorMessage.Should().Be(Constants.Messages.GoogleReconnectNeeded);
    }

    #endregion

    #region HandleTabChange

    [Fact]
    public async Task HandleTabChange_CallsTaskServiceSelectListAsync_WhenInvoked()
    {
        const string listId = Constants.TaskLists.LocalPomodoroListId;
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        var method = typeof(IndexBase).GetMethod(
            "HandleTabChange",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var task = (Task)method!.Invoke(cut.Instance, new object[] { listId })!;
        await task;

        TaskServiceMock.Verify(x => x.SelectListAsync(listId), Times.Once);
    }

    #endregion

    #region HandleUndoDelete

    [Fact]
    public async Task HandleUndoDelete_RestoresTask_WhenCalledAfterDelete()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Deleted Task" };
        AppState.Tasks = new List<TaskItem> { task };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.Instance.HandleTaskDelete(taskId);
        await cut.Instance.HandleUndoDelete();

        TaskServiceMock.Verify(x => x.RestoreTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskDelete_RendersUndoToast_WhenSuccessful()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "My Task" };
        AppState.Tasks = new List<TaskItem> { task };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var loadingProp = typeof(IndexBase).GetProperty("IsLoading",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        loadingProp!.SetValue(cut.Instance, false);
        cut.Render();

        await cut.Instance.HandleTaskDelete(taskId);
        cut.Render();

        cut.Markup.Should().Contain("undo-toast");
        cut.Markup.Should().Contain("My Task");
    }

    [Fact]
    public async Task HandleUndoDelete_DoesNothing_WhenNoDeletePending()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.Instance.HandleUndoDelete();

        TaskServiceMock.Verify(x => x.RestoreTaskAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleUndoDelete_SetsErrorMessage_WhenRestoreThrows()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Deleted Task" };
        AppState.Tasks = new List<TaskItem> { task };
        TaskServiceMock
            .Setup(x => x.RestoreTaskAsync(taskId))
            .ThrowsAsync(new InvalidOperationException("Restore failed"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.Instance.HandleTaskDelete(taskId);
        await cut.Instance.HandleUndoDelete();

        cut.Instance.ErrorMessage.Should().Contain("Restore failed");
    }

    #endregion

    #region Session Log Rendering

    [Fact]
    public void RendersSessionLog_WhenActivitiesExist()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, CompletedAt = DateTime.Today.AddHours(10) },
            new() { Type = SessionType.Pomodoro, CompletedAt = DateTime.Today.AddHours(9) },
            new() { Type = SessionType.ShortBreak, CompletedAt = DateTime.Today.AddHours(11) }
        };
        ActivityServiceMock.Setup(x => x.GetTodayActivities()).Returns(activities);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var loadingProp = typeof(IndexBase).GetProperty("IsLoading",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        loadingProp!.SetValue(cut.Instance, false);
        cut.Render();

        cut.Markup.Should().Contain("session-log");
    }

    #endregion
}

