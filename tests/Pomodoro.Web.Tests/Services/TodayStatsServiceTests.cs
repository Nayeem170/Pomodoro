using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TodayStatsService
/// </summary>
public class TodayStatsServiceTests
{
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ILogger<TodayStatsService>> _mockLogger;
    private readonly TodayStatsService _service;

    public TodayStatsServiceTests()
    {
        _mockActivityService = new Mock<IActivityService>();
        _mockLogger = new Mock<ILogger<TodayStatsService>>();
        _service = new TodayStatsService(_mockActivityService.Object);
    }

    [Fact]
    public void GetTodayTotalFocusMinutes_ReturnsZero_WhenNoActivities()
    {
        // Arrange
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(new List<ActivityRecord>());

        // Act
        var result = _service.GetTodayTotalFocusMinutes();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTodayTotalFocusMinutes_ReturnsSumOfPomodoroDurations()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, DurationMinutes = 15 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTotalFocusMinutes();

        // Assert
        Assert.Equal(50, result); // Only Pomodoro sessions
    }

    [Fact]
    public void GetTodayTotalFocusMinutes_ExcludesBreakSessions()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, DurationMinutes = 15 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTotalFocusMinutes();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTodayPomodoroCount_ReturnsZero_WhenNoActivities()
    {
        // Arrange
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(new List<ActivityRecord>());

        // Act
        var result = _service.GetTodayPomodoroCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTodayPomodoroCount_ReturnsCountOfPomodoroSessions()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, DurationMinutes = 15 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayPomodoroCount();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetTodayPomodoroCount_ExcludesBreakSessions()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, DurationMinutes = 15 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayPomodoroCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTodayTasksWorkedOn_ReturnsZero_WhenNoActivities()
    {
        // Arrange
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(new List<ActivityRecord>());

        // Act
        var result = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTodayTasksWorkedOn_ReturnsUniqueTaskCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task B", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 }, // Duplicate
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task C", DurationMinutes = 25 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(3, result); // Task A, Task B, Task C
    }

    [Fact]
    public void GetTodayTasksWorkedOn_ExcludesEmptyTaskNames()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = null, DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task B", DurationMinutes = 25 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(2, result); // Only Task A and Task B
    }

    [Fact]
    public void GetTodayTasksWorkedOn_ExcludesWhitespaceTaskNames()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "   ", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "\t", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task B", DurationMinutes = 25 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(2, result); // Only Task A and Task B
    }

    [Fact]
    public void GetTodayTasksWorkedOn_ExcludesBreakSessions()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.ShortBreak, TaskName = "Task B", DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, TaskName = "Task C", DurationMinutes = 15 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var result = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(1, result); // Only Task A from Pomodoro session
    }

    [Fact]
    public void AllMethods_UseSameActivityServiceCall()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task A", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.Pomodoro, TaskName = "Task B", DurationMinutes = 25 },
            new ActivityRecord { Type = SessionType.ShortBreak, TaskName = "", DurationMinutes = 5 }
        };
        _mockActivityService.Setup(x => x.GetTodayActivities())
            .Returns(activities);

        // Act
        var totalFocusMinutes = _service.GetTodayTotalFocusMinutes();
        var pomodoroCount = _service.GetTodayPomodoroCount();
        var tasksWorkedOn = _service.GetTodayTasksWorkedOn();

        // Assert
        Assert.Equal(50, totalFocusMinutes);
        Assert.Equal(2, pomodoroCount);
        Assert.Equal(2, tasksWorkedOn);
        _mockActivityService.Verify(x => x.GetTodayActivities(), Times.Exactly(3));
    }
}
