using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public class IndexPagePresenterServiceTests
{
    private readonly Mock<ILogger<IndexPagePresenterService>> _loggerMock;
    private readonly IndexPagePresenterService _service;

    public IndexPagePresenterServiceTests()
    {
        _loggerMock = new Mock<ILogger<IndexPagePresenterService>>();
        _service = new IndexPagePresenterService(_loggerMock.Object);
    }

    [Fact]
    public void UpdateState_WithValidServices_ShouldReturnCorrectState()
    {
        // Arrange
        var taskServiceMock = new Mock<ITaskService>();
        var timerServiceMock = new Mock<ITimerService>();
        
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 1" },
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 2" }
        };
        var currentTaskId = tasks[0].Id;
        var remainingTime = TimeSpan.FromMinutes(25);
        var sessionType = SessionType.Pomodoro;
        
        taskServiceMock.Setup(s => s.Tasks).Returns(tasks);
        taskServiceMock.Setup(s => s.CurrentTaskId).Returns(currentTaskId);
        timerServiceMock.Setup(s => s.RemainingTime).Returns(remainingTime);
        timerServiceMock.Setup(s => s.CurrentSessionType).Returns(sessionType);
        timerServiceMock.Setup(s => s.IsRunning).Returns(true);
        timerServiceMock.Setup(s => s.IsPaused).Returns(false);
        timerServiceMock.Setup(s => s.IsStarted).Returns(true);

        // Act
        var result = _service.UpdateState(taskServiceMock.Object, timerServiceMock.Object);

        // Assert
        Assert.Equal(2, result.Tasks.Count);
        Assert.Equal(currentTaskId, result.CurrentTaskId);
        Assert.Equal(remainingTime, result.RemainingTime);
        Assert.Equal(sessionType, result.CurrentSessionType);
        Assert.True(result.IsTimerRunning);
        Assert.False(result.IsTimerPaused);
        Assert.True(result.IsTimerStarted);
    }

    [Fact]
    public void UpdateState_WithNullTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var taskServiceMock = new Mock<ITaskService>();
        var timerServiceMock = new Mock<ITimerService>();
        
        taskServiceMock.Setup(s => s.Tasks).Returns((List<TaskItem>)null!);
        timerServiceMock.Setup(s => s.RemainingTime).Returns(TimeSpan.FromMinutes(25));
        timerServiceMock.Setup(s => s.CurrentSessionType).Returns(SessionType.Pomodoro);
        timerServiceMock.Setup(s => s.IsRunning).Returns(false);
        timerServiceMock.Setup(s => s.IsPaused).Returns(false);
        timerServiceMock.Setup(s => s.IsStarted).Returns(false);

        // Act
        var result = _service.UpdateState(taskServiceMock.Object, timerServiceMock.Object);

        // Assert
        Assert.Empty(result.Tasks);
        Assert.Null(result.CurrentTaskId);
    }

    [Fact]
    public void UpdateState_WithException_ShouldReturnDefaultState()
    {
        // Arrange
        var taskServiceMock = new Mock<ITaskService>();
        var timerServiceMock = new Mock<ITimerService>();
        
        taskServiceMock.Setup(s => s.Tasks).Throws(new Exception("Test exception"));

        // Act
        var result = _service.UpdateState(taskServiceMock.Object, timerServiceMock.Object);

        // Assert
        Assert.Empty(result.Tasks);
        Assert.Equal(TimeSpan.FromMinutes(Constants.Timer.DefaultPomodoroMinutes), result.RemainingTime);
        Assert.Equal(SessionType.Pomodoro, result.CurrentSessionType);
        Assert.False(result.IsTimerStarted);
    }

    [Fact]
    public void UpdateState_WithAllTimerStates_ShouldReturnCorrectValues()
    {
        // Arrange
        var taskServiceMock = new Mock<ITaskService>();
        var timerServiceMock = new Mock<ITimerService>();
        
        taskServiceMock.Setup(s => s.Tasks).Returns(new List<TaskItem>());
        timerServiceMock.Setup(s => s.RemainingTime).Returns(TimeSpan.FromMinutes(5));
        timerServiceMock.Setup(s => s.CurrentSessionType).Returns(SessionType.ShortBreak);
        timerServiceMock.Setup(s => s.IsRunning).Returns(true);
        timerServiceMock.Setup(s => s.IsPaused).Returns(true);
        timerServiceMock.Setup(s => s.IsStarted).Returns(true);

        // Act
        var result = _service.UpdateState(taskServiceMock.Object, timerServiceMock.Object);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(5), result.RemainingTime);
        Assert.Equal(SessionType.ShortBreak, result.CurrentSessionType);
        Assert.True(result.IsTimerRunning);
        Assert.True(result.IsTimerPaused);
        Assert.True(result.IsTimerStarted);
    }

    [Fact]
    public void IndexPageState_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var state = new IndexPageState();

        // Assert
        Assert.Empty(state.Tasks);
        Assert.Null(state.CurrentTaskId);
        Assert.Equal(TimeSpan.Zero, state.RemainingTime);
        Assert.Equal(SessionType.Pomodoro, state.CurrentSessionType);
        Assert.False(state.IsTimerRunning);
        Assert.False(state.IsTimerPaused);
        Assert.False(state.IsTimerStarted);
    }
}
