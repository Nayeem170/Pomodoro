using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class IndexPagePresenterServiceTests
{
    private readonly Mock<ILogger<IndexPagePresenterService>> _loggerMock;
    private readonly IndexPagePresenterService _service;

    public IndexPagePresenterServiceTests()
    {
        _loggerMock = new Mock<ILogger<IndexPagePresenterService>>();
        _service = new IndexPagePresenterService(_loggerMock.Object);
    }

    private static Mock<ITaskService> SetupTaskService(List<TaskItem>? tasks = null, Guid? currentTaskId = null, IReadOnlyList<TaskListRef>? taskLists = null)
    {
        var mock = new Mock<ITaskService>();
        mock.Setup(s => s.CurrentTaskId).Returns(currentTaskId);
        mock.Setup(s => s.CurrentTask).Returns((TaskItem?)null);
        mock.Setup(s => s.CurrentListId).Returns((string?)null);
        mock.Setup(s => s.TaskLists).Returns(taskLists ?? new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true)
        });
        mock.Setup(s => s.GetTasksForListAsync(It.IsAny<string>()))
            .ReturnsAsync(tasks ?? new List<TaskItem>());
        return mock;
    }

    private static Mock<ITimerService> SetupTimerService(TimeSpan remaining = default, SessionType session = SessionType.Pomodoro,
        bool running = false, bool paused = false, bool started = false)
    {
        var mock = new Mock<ITimerService>();
        mock.Setup(s => s.RemainingTime).Returns(remaining);
        mock.Setup(s => s.CurrentSessionType).Returns(session);
        mock.Setup(s => s.IsRunning).Returns(running);
        mock.Setup(s => s.IsPaused).Returns(paused);
        mock.Setup(s => s.IsStarted).Returns(started);
        return mock;
    }

    [Fact]
    public async Task UpdateStateAsync_WithValidServices_ShouldReturnCorrectState()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Task 2", CreatedAt = DateTime.UtcNow }
        };
        var currentTaskId = tasks[0].Id;
        var taskService = SetupTaskService(tasks, currentTaskId);
        var timerService = SetupTimerService(TimeSpan.FromMinutes(25), SessionType.Pomodoro, true, false, true);

        var result = await _service.UpdateStateAsync(taskService.Object, timerService.Object, null);

        Assert.Equal(2, result.Tasks.Count);
        Assert.Equal(currentTaskId, result.CurrentTaskId);
        Assert.Equal(TimeSpan.FromMinutes(25), result.RemainingTime);
        Assert.Equal(SessionType.Pomodoro, result.CurrentSessionType);
        Assert.True(result.IsTimerRunning);
        Assert.False(result.IsTimerPaused);
        Assert.True(result.IsTimerStarted);
        Assert.Equal(Constants.TaskLists.LocalPomodoroListId, result.CurrentListId);
    }

    [Fact]
    public async Task UpdateStateAsync_WithNullTasks_ShouldReturnEmptyList()
    {
        var taskService = SetupTaskService(null);
        var timerService = SetupTimerService(TimeSpan.FromMinutes(25));

        var result = await _service.UpdateStateAsync(taskService.Object, timerService.Object, null);

        Assert.Empty(result.Tasks);
        Assert.Null(result.CurrentTaskId);
    }

    [Fact]
    public async Task UpdateStateAsync_WithException_ShouldReturnDefaultState()
    {
        var taskService = new Mock<ITaskService>();
        taskService.Setup(s => s.GetTasksForListAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));
        var timerService = SetupTimerService();

        var result = await _service.UpdateStateAsync(taskService.Object, timerService.Object, null);

        Assert.Empty(result.Tasks);
        Assert.Equal(TimeSpan.FromMinutes(Constants.Timer.DefaultPomodoroMinutes), result.RemainingTime);
        Assert.Equal(SessionType.Pomodoro, result.CurrentSessionType);
        Assert.False(result.IsTimerStarted);
        Assert.Equal(Constants.TaskLists.LocalPomodoroListId, result.CurrentListId);
    }

    [Fact]
    public async Task UpdateStateAsync_WithAllTimerStates_ShouldReturnCorrectValues()
    {
        var taskService = SetupTaskService(new List<TaskItem>());
        var timerService = SetupTimerService(TimeSpan.FromMinutes(5), SessionType.ShortBreak, true, true, true);

        var result = await _service.UpdateStateAsync(taskService.Object, timerService.Object, null);

        Assert.Equal(TimeSpan.FromMinutes(5), result.RemainingTime);
        Assert.Equal(SessionType.ShortBreak, result.CurrentSessionType);
        Assert.True(result.IsTimerRunning);
        Assert.True(result.IsTimerPaused);
        Assert.True(result.IsTimerStarted);
    }

    [Fact]
    public async Task UpdateStateAsync_PassesListIdToTaskService()
    {
        var taskService = SetupTaskService();
        var timerService = SetupTimerService();

        await _service.UpdateStateAsync(taskService.Object, timerService.Object, "custom-list-id");

        taskService.Verify(s => s.GetTasksForListAsync("custom-list-id"), Times.Once);
    }

    [Fact]
    public async Task UpdateStateAsync_FallsBackToCurrentListId_WhenNull()
    {
        var taskService = SetupTaskService();
        taskService.Setup(s => s.CurrentListId).Returns("glist-1");
        var timerService = SetupTimerService();

        var result = await _service.UpdateStateAsync(taskService.Object, timerService.Object, null);

        taskService.Verify(s => s.GetTasksForListAsync("glist-1"), Times.Once);
        Assert.Equal("glist-1", result.CurrentListId);
    }

    [Fact]
    public void IndexPageState_DefaultValues_ShouldBeCorrect()
    {
        var state = new IndexPageState();

        Assert.Empty(state.Tasks);
        Assert.Null(state.CurrentTaskId);
        Assert.Null(state.CurrentListId);
        Assert.Empty(state.TaskLists);
        Assert.Equal(TimeSpan.Zero, state.RemainingTime);
        Assert.Equal(SessionType.Pomodoro, state.CurrentSessionType);
        Assert.False(state.IsTimerRunning);
        Assert.False(state.IsTimerPaused);
        Assert.False(state.IsTimerStarted);
    }
}
