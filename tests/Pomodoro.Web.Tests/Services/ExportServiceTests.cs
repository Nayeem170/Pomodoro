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
[Trait("Category", "Service")]
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

