using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ExportService duplicate detection logic covering ActivityKey and TaskKey record structs
/// </summary>
public class ExportServiceTestsDuplicateDetection
{
    private readonly Mock<IActivityRepository> _mockActivityRepository;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<ILogger<ExportService>> _mockLogger;
    private readonly ExportService _exportService;

    public ExportServiceTestsDuplicateDetection()
    {
        _mockActivityRepository = new Mock<IActivityRepository>();
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockLogger = new Mock<ILogger<ExportService>>();

        _exportService = new ExportService(
            _mockActivityRepository.Object,
            _mockTaskRepository.Object,
            _mockSettingsRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ImportFromJsonAsync_UsesActivityKeyForDuplicateDetection()
    {
        // Arrange
        var completedAt = DateTime.UtcNow;
        var existingActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = completedAt,
            DurationMinutes = 25,
            TaskName = "Task 1"
        };

        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = completedAt,
                DurationMinutes = 25,
                TaskName = "Task 1"
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow.AddMinutes(5),
                DurationMinutes = 5,
                TaskName = null
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord> { existingActivity });
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(1, result.ActivitiesSkipped);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_UsesTaskKeyForDuplicateDetection()
    {
        // Arrange
        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Existing Task",
            CreatedAt = DateTime.UtcNow,
            PomodoroCount = 5
        };

        var importTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Existing Task",
                CreatedAt = existingTask.CreatedAt,
                PomodoroCount = 3
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "New Task",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 2
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = new List<ActivityRecord>(),
            tasks = importTasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem> { existingTask });

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TasksImported);
        Assert.Equal(1, result.TasksSkipped);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateActivitiesInImportFile_SkipsDuplicates()
    {
        // Arrange
        var completedAt = DateTime.UtcNow;
        
        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = completedAt,
                DurationMinutes = 25,
                TaskName = "Task 1"
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = completedAt,
                DurationMinutes = 25,
                TaskName = "Task 1"
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(1, result.ActivitiesSkipped);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateTasksInImportFile_SkipsDuplicates()
    {
        // Arrange
        var completedAt = DateTime.UtcNow;
        
        var importTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Same Task",
                CreatedAt = completedAt,
                PomodoroCount = 3
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Same Task",
                CreatedAt = completedAt,
                PomodoroCount = 5
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = new List<ActivityRecord>(),
            tasks = importTasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TasksImported);
        Assert.Equal(1, result.TasksSkipped);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }
}
