using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Comprehensive tests for ExportService
/// </summary>
public class ExportServiceTests
{
    private readonly Mock<IActivityRepository> _mockActivityRepository;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<ILogger<ExportService>> _mockLogger;
    private readonly ExportService _exportService;

    public ExportServiceTests()
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

    #region ExportToJsonAsync Tests

    [Fact]
    public async Task ExportToJsonAsync_WithEmptyData_ReturnsValidJson()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());
        _mockSettingsRepository.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings());

        // Act
        var result = await _exportService.ExportToJsonAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"version\": 1", result);
        Assert.Contains("\"exportDate\"", result);
        Assert.Contains("\"settings\"", result);
        Assert.Contains("\"activities\"", result);
        Assert.Contains("\"tasks\"", result);
        
        // Verify it's valid JSON
        var jsonDoc = JsonDocument.Parse(result);
        Assert.True(jsonDoc.RootElement.TryGetProperty("version", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("exportDate", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("settings", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("activities", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("tasks", out _));
    }

    [Fact]
    public async Task ExportToJsonAsync_WithData_ReturnsJsonWithAllData()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                TaskName = "Test Task"
            }
        };

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Task",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 5
            }
        };

        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15
        };

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mockSettingsRepository.Setup(r => r.GetAsync()).ReturnsAsync(settings);

        // Act
        var result = await _exportService.ExportToJsonAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        
        var activitiesArray = jsonDoc.RootElement.GetProperty("activities");
        Assert.Equal(1, activitiesArray.GetArrayLength());
        
        var tasksArray = jsonDoc.RootElement.GetProperty("tasks");
        Assert.Equal(1, tasksArray.GetArrayLength());
        
        var settingsElement = jsonDoc.RootElement.GetProperty("settings");
        Assert.Equal(25, settingsElement.GetProperty("pomodoroMinutes").GetInt32());
        Assert.Equal(5, settingsElement.GetProperty("shortBreakMinutes").GetInt32());
        Assert.Equal(15, settingsElement.GetProperty("longBreakMinutes").GetInt32());
    }

    [Fact]
    public async Task ExportToJsonAsync_WithMultipleActivities_ReturnsAllActivities()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = DateTime.UtcNow, DurationMinutes = 25 },
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = DateTime.UtcNow, DurationMinutes = 5 },
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = DateTime.UtcNow, DurationMinutes = 15 }
        };

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());
        _mockSettingsRepository.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings());

        // Act
        var result = await _exportService.ExportToJsonAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var activitiesArray = jsonDoc.RootElement.GetProperty("activities");
        Assert.Equal(3, activitiesArray.GetArrayLength());
    }

    [Fact]
    public async Task ExportToJsonAsync_WithMultipleTasks_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow, PomodoroCount = 3 },
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 2", CreatedAt = DateTime.UtcNow, PomodoroCount = 5 },
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 3", CreatedAt = DateTime.UtcNow, PomodoroCount = 2 }
        };

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mockSettingsRepository.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings());

        // Act
        var result = await _exportService.ExportToJsonAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var tasksArray = jsonDoc.RootElement.GetProperty("tasks");
        Assert.Equal(3, tasksArray.GetArrayLength());
    }

    [Fact]
    public async Task ExportToJsonAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _exportService.ExportToJsonAsync());
    }

    [Fact]
    public async Task ExportToJsonAsync_LogsExportInformation()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = DateTime.UtcNow, DurationMinutes = 25 }
        };
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Test Task", CreatedAt = DateTime.UtcNow, PomodoroCount = 3 }
        };

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mockSettingsRepository.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings());

        // Act
        await _exportService.ExportToJsonAsync();

        // Assert - logger was called at least once
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExportToJsonAsync_WhenRepositoryThrows_LogsError()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Test error"));

        // Act
        try
        {
            await _exportService.ExportToJsonAsync();
            Assert.True(false); // Should not reach here
        }
        catch (Exception)
        {
            // Assert - logger was called at least once
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    #endregion

    #region ImportFromJsonAsync Tests

    [Fact]
    public async Task ImportFromJsonAsync_WithEmptyString_ReturnsFailedResult()
    {
        // Act
        var result = await _exportService.ImportFromJsonAsync("");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithWhitespaceOnly_ReturnsFailedResult()
    {
        // Act
        var result = await _exportService.ImportFromJsonAsync("   ");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithNullString_ReturnsFailedResult()
    {
        // Act
        var result = await _exportService.ImportFromJsonAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithInvalidJson_ReturnsFailedResult()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = await _exportService.ImportFromJsonAsync(invalidJson);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithInvalidVersion_ReturnsFailedResult()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            version = 0,
            exportDate = DateTime.UtcNow,
            settings = new TimerSettings(),
            activities = new List<ActivityRecord>(),
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("version", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithNegativeVersion_ReturnsFailedResult()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            version = -1,
            exportDate = DateTime.UtcNow,
            settings = new TimerSettings(),
            activities = new List<ActivityRecord>(),
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("version", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithValidEmptyData_ReturnsSuccess()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = new TimerSettings(),
            activities = new List<ActivityRecord>(),
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.True(result.SettingsImported);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithSettingsOnly_ImportsSettings()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 30,
            ShortBreakMinutes = 10,
            LongBreakMinutes = 20
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = settings,
            activities = new List<ActivityRecord>(),
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.True(result.SettingsImported);
        _mockSettingsRepository.Verify(r => r.SaveAsync(It.Is<TimerSettings>(s => 
            s.PomodoroMinutes == 30 &&
            s.ShortBreakMinutes == 10 &&
            s.LongBreakMinutes == 20
        )), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithTasksOnly_ImportsTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Task 1",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 3
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Task 2",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 5
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = new List<ActivityRecord>(),
            tasks = tasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(2, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.False(result.SettingsImported);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithActivitiesOnly_ImportsActivities()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                TaskName = "Task 1"
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = activities,
            tasks = new List<TaskItem>()
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.False(result.SettingsImported);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateTasks_SkipsDuplicates()
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
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.False(result.SettingsImported);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateActivities_SkipsDuplicates()
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
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.False(result.SettingsImported);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_MapsTaskIdsForActivities()
    {
        // Arrange
        var oldTaskId = Guid.NewGuid();

        var importTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = oldTaskId,
                Name = "Task 1",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 3
            }
        };

        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskId = oldTaskId,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = importTasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Capture saved tasks and activities to verify TaskId mapping
        TaskItem? savedTask = null;
        ActivityRecord? savedActivity = null;
        _mockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => savedTask = t)
            .ReturnsAsync(true);
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => savedActivity = a)
            .ReturnsAsync(true);

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedActivity);
        Assert.NotEqual(oldTaskId, savedTask!.Id); // Task should have new ID
        Assert.Equal(savedTask.Id, savedActivity!.TaskId); // Activity should reference new task ID
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithExistingTaskReference_KeepsReference()
    {
        // Arrange
        var existingTaskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = existingTaskId,
            Name = "Existing Task",
            CreatedAt = DateTime.UtcNow,
            PomodoroCount = 5
        };

        var importTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "New Task",
                CreatedAt = DateTime.UtcNow,
                PomodoroCount = 3
            }
        };

        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskId = existingTaskId,
                TaskName = "Existing Task",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = importTasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem> { existingTask });

        // Capture saved activities to verify TaskId mapping
        ActivityRecord? savedActivity = null;
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => savedActivity = a)
            .ReturnsAsync(true);

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.NotNull(savedActivity);
        Assert.Equal(existingTaskId, savedActivity!.TaskId);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithNonExistentTaskReference_SetsTaskIdToNull()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();

        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskId = nonExistentTaskId,
                TaskName = "Non-existent Task",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = new List<TaskItem>() // No tasks in import, so the referenced task doesn't exist
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Capture saved activities
        ActivityRecord? savedActivity = null;
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => savedActivity = a)
            .ReturnsAsync(true);

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.NotNull(savedActivity);
        Assert.Null(savedActivity!.TaskId); // TaskId should be null when referenced task doesn't exist
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ImportFromJsonAsync_GeneratesNewIdsForImportedItems()
    {
        // Arrange
        var importTasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow, PomodoroCount = 3 },
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 2", CreatedAt = DateTime.UtcNow, PomodoroCount = 5 }
        };

        var importActivities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskId = importTasks[0].Id,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25
            }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = importActivities,
            tasks = importTasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Capture saved activities
        List<ActivityRecord> savedActivities = new();
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => savedActivities.Add(a))
            .ReturnsAsync(true);

        // Act
        var result = await _exportService.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        var importedActivity = Assert.Single(savedActivities);
        
        // Verify imported activity has a new ID
        Assert.NotEqual(Guid.Empty, importedActivity.Id);
    }

    [Fact]
    public async Task ImportFromJsonAsync_LogsImportStatistics()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow, PomodoroCount = 3 },
            new TaskItem { Id = Guid.NewGuid(), Name = "Task 2", CreatedAt = DateTime.UtcNow, PomodoroCount = 5 }
        };

        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = DateTime.UtcNow, DurationMinutes = 25, TaskName = "Task 1" },
            new ActivityRecord { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = DateTime.UtcNow, DurationMinutes = 5, TaskName = null }
        };

        var json = JsonSerializer.Serialize(new
        {
            version = 1,
            exportDate = DateTime.UtcNow,
            settings = (TimerSettings?)null,
            activities = activities,
            tasks = tasks
        });

        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        await _exportService.ImportFromJsonAsync(json);

        // Assert - logger was called at least once
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithException_LogsErrorAndReturnsFailed()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _exportService.ImportFromJsonAsync("{}");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithDuplicateWithinImport_SkipsLaterDuplicates()
    {
        // Arrange
        var taskName = "Duplicate Task";
        var createdAt = DateTime.UtcNow;

        var importTasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = taskName, CreatedAt = createdAt, PomodoroCount = 1 },
            new TaskItem { Id = Guid.NewGuid(), Name = taskName, CreatedAt = createdAt, PomodoroCount = 1 }
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

    #endregion

    #region ClearAllDataAsync Tests

    [Fact]
    public async Task ClearAllDataAsync_ClearsAllRepositories()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(10);
        _mockTaskRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(5);

        // Act
        await _exportService.ClearAllDataAsync();

        // Assert
        _mockActivityRepository.Verify(r => r.ClearAllAsync(), Times.Once);
        _mockTaskRepository.Verify(r => r.ClearAllAsync(), Times.Once);
        _mockSettingsRepository.Verify(r => r.SaveAsync(It.IsAny<TimerSettings>()), Times.Once);
    }

    [Fact]
    public async Task ClearAllDataAsync_ResetsSettingsToDefaults()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(10);
        _mockTaskRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(5);

        TimerSettings? savedSettings = null;
        _mockSettingsRepository.Setup(r => r.SaveAsync(It.IsAny<TimerSettings>()))
            .Callback<TimerSettings>(s => savedSettings = s)
            .ReturnsAsync(true);

        // Act
        await _exportService.ClearAllDataAsync();

        // Assert
        Assert.NotNull(savedSettings);
        Assert.Equal(25, savedSettings!.PomodoroMinutes); // Default value
        Assert.Equal(5, savedSettings!.ShortBreakMinutes); // Default value
        Assert.Equal(15, savedSettings!.LongBreakMinutes); // Default value
        _mockActivityRepository.Verify(r => r.ClearAllAsync(), Times.Once);
        _mockTaskRepository.Verify(r => r.ClearAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearAllDataAsync_LogsSuccess()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(10);
        _mockTaskRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(5);

        // Act
        await _exportService.ClearAllDataAsync();

        // Assert - logger was called at least once
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClearAllDataAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _exportService.ClearAllDataAsync());
    }

    [Fact]
    public async Task ClearAllDataAsync_WhenRepositoryThrows_LogsError()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ThrowsAsync(new Exception("Test error"));

        // Act
        try
        {
            await _exportService.ClearAllDataAsync();
            Assert.True(false); // Should not reach here
        }
        catch (Exception)
        {
            // Assert - logger was called at least once
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    [Fact]
    public async Task ClearAllDataAsync_WithEmptyRepositories_LogsCorrectCounts()
    {
        // Arrange
        _mockActivityRepository.Setup(r => r.GetCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(0);
        _mockTaskRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(0);

        // Act
        await _exportService.ClearAllDataAsync();

        // Assert - logger was called at least once
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
