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
    public async Task HandleTaskSelect_WhenPomodoroStarted_SwapsTaskWithoutRestart()
    {
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        TimerServiceMock.SetupGet(x => x.CurrentSession).Returns(new TimerSession
        {
            Type = SessionType.Pomodoro,
            WasStarted = true,
            IsRunning = true
        });

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskSelect(taskId);

        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.PauseAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        TimerServiceMock.Verify(x => x.ChangeCurrentTask(taskId), Times.Once);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenTimerNotRunning_DoesNotRecordPartial()
    {
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
        TimerServiceMock.SetupGet(x => x.CurrentSession).Returns((TimerSession?)null);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskSelect(taskId);

        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        TimerServiceMock.Verify(x => x.ChangeCurrentTask(It.IsAny<Guid>()), Times.Never);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenSettingOff_StillSwapsTask()
    {
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        TimerServiceMock.SetupGet(x => x.CurrentSession).Returns(new TimerSession
        {
            Type = SessionType.Pomodoro,
            WasStarted = true,
            IsRunning = true
        });
        var appState = Services.GetRequiredService<AppState>();
        appState.Settings.RecordPartialSessions = false;

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskSelect(taskId);

        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.ChangeCurrentTask(taskId), Times.Once);
        TaskServiceMock.Verify(x => x.SelectTaskAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task HandleTaskSelect_WhenBreakSession_DoesNotSwapTask()
    {
        var taskId = Guid.NewGuid();
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(true);
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.ShortBreak);
        TimerServiceMock.SetupGet(x => x.CurrentSession).Returns(new TimerSession
        {
            Type = SessionType.ShortBreak,
            WasStarted = true,
            IsRunning = true
        });

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskSelect(taskId);

        TimerServiceMock.Verify(x => x.TryRecordPartialSessionAsync(), Times.Never);
        TimerServiceMock.Verify(x => x.ChangeCurrentTask(It.IsAny<Guid>()), Times.Never);
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
    public async Task HandleTaskAdd_Quarterly_MapsDayZeroToNull()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Quarterly Task" };
        AppState.Tasks = new List<TaskItem> { task };
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var request = new NewTaskRequest("Quarterly Task", RepeatType.Quarterly, DateTime.Now, QuarterlyDay: 20);
        await cut.Instance.HandleTaskAdd(request);

        task.Repeat.Should().NotBeNull();
        task.Repeat!.QuarterlyDay.Should().Be(20);

        var unsetRequest = new NewTaskRequest("Quarterly Task", RepeatType.Quarterly, DateTime.Now);
        await cut.Instance.HandleTaskAdd(unsetRequest);

        task.Repeat.QuarterlyDay.Should().BeNull();
    }

    [Fact]
    public async Task HandleTaskAdd_Yearly_MapsZeroDayAndMonthToNull()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Yearly Task" };
        AppState.Tasks = new List<TaskItem> { task };
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns(taskId);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var request = new NewTaskRequest(
            "Yearly Task", RepeatType.Yearly, DateTime.Now, YearlyDay: 9, YearlyMonth: 11);
        await cut.Instance.HandleTaskAdd(request);

        task.Repeat.Should().NotBeNull();
        task.Repeat!.YearlyDay.Should().Be(9);
        task.Repeat.YearlyMonth.Should().Be(11);

        var unsetRequest = new NewTaskRequest("Yearly Task", RepeatType.Yearly, DateTime.Now);
        await cut.Instance.HandleTaskAdd(unsetRequest);

        task.Repeat.YearlyDay.Should().BeNull();
        task.Repeat.YearlyMonth.Should().BeNull();
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

    #region HandleToggleFollowParent

    [Fact]
    public async Task HandleToggleFollowParent_TogglesFlagAndCallsService()
    {
        var taskId = Guid.NewGuid();
        AppState.Tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Subtask", FollowsParentRepeat = true }
        };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleToggleFollowParent(taskId);

        TaskServiceMock.Verify(
            x => x.SetFollowsParentRepeatAsync(taskId, false),
            Times.Once);
    }

    [Fact]
    public async Task HandleToggleFollowParent_WhenTaskMissing_DoesNotCallService()
    {
        AppState.Tasks = new List<TaskItem>();

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleToggleFollowParent(Guid.NewGuid());

        TaskServiceMock.Verify(
            x => x.SetFollowsParentRepeatAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    #endregion

    #region HandleTaskReorder

    [Fact]
    public async Task HandleTaskReorder_CallsService_WhenCalled()
    {
        var taskId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(taskId, targetId, true))
            .ReturnsAsync(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTaskReorder(new ReorderRequest(taskId, targetId, InsertBefore: true)));

        TaskServiceMock.Verify(
            x => x.ReorderTaskAsync(taskId, targetId, true),
            Times.Once);
        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task HandleTaskReorder_SkipsStateUpdate_WhenReorderRejected()
    {
        var taskId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(taskId, targetId, false))
            .ReturnsAsync(false);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.InvokeAsync(() => cut.Instance.HandleTaskReorder(new ReorderRequest(taskId, targetId, InsertBefore: false)));

        TaskServiceMock.Verify(
            x => x.ReorderTaskAsync(taskId, targetId, false),
            Times.Once);
        cut.Instance.ErrorMessage.Should().BeNull();
        cut.Markup.Should().NotContain("Moved ",
            "a rejected move renders no announcement");
    }

    [Fact]
    public async Task HandleTaskReorder_Success_RendersLiveAnnouncement()
    {
        // Arrange - post-move state: B(0) moved above A(1000), C(3000)
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "A", SortOrder = 1000, CreatedAt = new DateTime(2026, 1, 1) };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "B", SortOrder = 0, CreatedAt = new DateTime(2026, 1, 2) };
        var c = new TaskItem { Id = Guid.NewGuid(), Name = "C", SortOrder = 3000, CreatedAt = new DateTime(2026, 1, 3) };
        var group = new List<TaskItem> { a, b, c };
        TaskServiceMock
            .Setup(x => x.GetTasksForListAsync(It.IsAny<string?>()))
            .ReturnsAsync(group);
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(b.Id, a.Id, true))
            .ReturnsAsync(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.HandleTaskReorder(new ReorderRequest(b.Id, a.Id, InsertBefore: true)));

        // Assert
        cut.Markup.Should().Contain("Moved B to position 1 of 3");
        cut.Find(".sr-only[aria-live='polite']").Should().NotBeNull(
            "the announcement renders in the visually-hidden polite region");
    }

    [Fact]
    public async Task HandleTaskReorder_ExceptionAfterSuccess_ClearsStaleAnnouncement()
    {
        // Arrange
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "A", SortOrder = 1000, CreatedAt = new DateTime(2026, 1, 1) };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "B", SortOrder = 0, CreatedAt = new DateTime(2026, 1, 2) };
        var group = new List<TaskItem> { a, b };
        TaskServiceMock
            .Setup(x => x.GetTasksForListAsync(It.IsAny<string?>()))
            .ReturnsAsync(group);
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(b.Id, a.Id, true))
            .ReturnsAsync(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.InvokeAsync(() => cut.Instance.HandleTaskReorder(new ReorderRequest(b.Id, a.Id, InsertBefore: true)));
        cut.Markup.Should().Contain("Moved B to position",
            "sanity: the first move announced");

        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(b.Id, a.Id, true))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await cut.Instance.HandleTaskReorder(new ReorderRequest(b.Id, a.Id, InsertBefore: true));

        // Assert
        cut.Markup.Should().NotContain("Moved B",
            "a failed move must clear the stale announcement, not leave the previous one");
    }

    [Fact]
    public async Task HandleTaskReorder_SetsErrorMessage_WhenExceptionThrown()
    {
        var taskId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(taskId, targetId, true))
            .ThrowsAsync(new Exception("Test exception"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        await cut.Instance.HandleTaskReorder(new ReorderRequest(taskId, targetId, InsertBefore: true));

        cut.Instance.ErrorMessage.Should().Be("Error updating task: Test exception");
    }

    #endregion

    #region HandleScheduleReorder

    [Fact]
    public async Task HandleScheduleReorder_CallsService_WhenCalled()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(taskId, targetId, false))
            .ReturnsAsync(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.HandleScheduleReorder(new ReorderRequest(taskId, targetId, InsertBefore: false)));

        // Assert
        TaskServiceMock.Verify(
            x => x.ReorderTaskAsync(taskId, targetId, false),
            Times.Once);
        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task HandleScheduleReorder_Success_RendersDayScopedAnnouncement()
    {
        // Arrange - post-move state: DayA(2000) moved below DayB(1000); both scheduled
        // tomorrow (same day group). Elsewhere is scheduled outside the 7-day window
        // and must NOT be counted: the announcement scope is the day group.
        var tomorrow = DateTime.Today.AddDays(1);
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "DayA", SortOrder = 2000, CreatedAt = new DateTime(2026, 1, 1), ScheduledDate = tomorrow };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "DayB", SortOrder = 1000, CreatedAt = new DateTime(2026, 1, 2), ScheduledDate = tomorrow };
        var elsewhere = new TaskItem { Id = Guid.NewGuid(), Name = "Elsewhere", SortOrder = 500, CreatedAt = new DateTime(2026, 1, 3), ScheduledDate = DateTime.Today.AddDays(20) };
        AppState.Tasks = new List<TaskItem> { a, b, elsewhere };
        TaskServiceMock
            .Setup(x => x.GetTasksForListAsync(It.IsAny<string?>()))
            .ReturnsAsync(new List<TaskItem> { a, b, elsewhere });
        TaskServiceMock
            .Setup(x => x.ReorderTaskAsync(a.Id, b.Id, false))
            .ReturnsAsync(true);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - move DayA below DayB within the day group.
        await cut.InvokeAsync(() => cut.Instance.HandleScheduleReorder(new ReorderRequest(a.Id, b.Id, InsertBefore: false)));

        // Assert - position is day-group scoped: 2 of 2, not 3.
        cut.Markup.Should().Contain("Moved DayA to position 2 of 2");
    }

    #endregion

    #region HandleTaskEdit + Undo Toast Lifecycle

    [Fact]
    public async Task HandleTaskDelete_HidesUndoToast_AfterDurationElapses()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Toast Task" };
        AppState.Tasks = new List<TaskItem> { task };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await cut.Instance.HandleTaskDelete(taskId);
        cut.Render();
        cut.Markup.Should().Contain("undo-toast");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (cut.Markup.Contains("undo-toast"))
        {
            sw.Elapsed.Should().BeLessThan(
                TimeSpan.FromMilliseconds(Constants.UI.UndoToastDurationMs + 4000),
                "the undo toast must auto-hide after the configured duration");
            await Task.Delay(250);
        }
    }

    [Fact]
    public async Task HandleTaskEdit_MovesTaskToNewList_WhenGoogleListChanges()
    {
        var listA = "list-a";
        var listB = "list-b";
        var taskId = Guid.NewGuid();
        var existing = new TaskItem { Id = taskId, Name = "Moved Task", GoogleListId = listA };
        AppState.Tasks = new List<TaskItem> { existing };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var edited = new TaskItem { Id = taskId, Name = "Moved Task", GoogleListId = listB };
        await cut.Instance.HandleTaskEdit(edited);

        TaskServiceMock.Verify(x => x.UpdateTaskAsync(edited), Times.Once);
        TaskServiceMock.Verify(x => x.MoveTaskToListAsync(taskId, listB), Times.Once);
    }

    [Fact]
    public async Task HandleTaskEdit_DoesNotMoveTask_WhenGoogleListUnchanged()
    {
        var listA = "list-a";
        var taskId = Guid.NewGuid();
        var existing = new TaskItem { Id = taskId, Name = "Stable Task", GoogleListId = listA };
        AppState.Tasks = new List<TaskItem> { existing };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var edited = new TaskItem { Id = taskId, Name = "Stable Task Renamed", GoogleListId = listA };
        await cut.Instance.HandleTaskEdit(edited);

        TaskServiceMock.Verify(x => x.UpdateTaskAsync(edited), Times.Once);
        TaskServiceMock.Verify(x => x.MoveTaskToListAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    #endregion
}

