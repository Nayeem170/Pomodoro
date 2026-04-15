using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests specifically designed to achieve line coverage for record struct definitions
/// at lines 310 and 315 in ExportService.cs
/// </summary>
[Trait("Category", "Service")]
public class ExportServiceTests_CoverageLine310And315 : TestBase
{
    [Fact]
    public async Task ImportFromJsonAsync_ActivityKeyExercisesLine310()
    {
        // Arrange
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ExportService>>();

        var existingActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.Parse("2024-01-01T10:00:00Z").ToUniversalTime(),
            DurationMinutes = 25,
            TaskName = "Existing Task"
        };

        mockActivityRepo
            .Setup(r => r.GetAllAsync())
            .Returns(Task.FromResult(new[] { existingActivity }.ToList()));

        mockTaskRepo
            .Setup(r => r.GetAllAsync())
            .Returns(Task.FromResult(Array.Empty<TaskItem>().ToList()));

        var service = new ExportService(
            mockActivityRepo.Object,
            mockTaskRepo.Object,
            mockSettingsRepo.Object,
            mockLogger.Object
        );

        var json = @"{
            ""version"": 1,
            ""exportDate"": ""2024-01-01T00:00:00Z"",
            ""settings"": {
                ""focusDuration"": 25,
                ""shortBreakDuration"": 5,
                ""longBreakDuration"": 15,
                ""sessionsUntilLongBreak"": 4
            },
            ""activities"": [
                {
                    ""type"": 0,
                    ""completedAt"": ""2024-01-01T10:00:00Z"",
                    ""durationMinutes"": 25,
                    ""taskName"": ""Existing Task""
                }
            ],
            ""tasks"": []
        }";

        // Act
        var result = await service.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(1, result.ActivitiesSkipped);
    }

    [Fact]
    public async Task ImportFromJsonAsync_TaskKeyExercisesLine315()
    {
        // Arrange
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ExportService>>();

        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Existing Task",
            CreatedAt = DateTime.Parse("2024-01-01T10:00:00Z").ToUniversalTime(),
            PomodoroCount = 5
        };

        mockActivityRepo
            .Setup(r => r.GetAllAsync())
            .Returns(Task.FromResult(Array.Empty<ActivityRecord>().ToList()));

        mockTaskRepo
            .Setup(r => r.GetAllAsync())
            .Returns(Task.FromResult(new[] { existingTask }.ToList()));

        var service = new ExportService(
            mockActivityRepo.Object,
            mockTaskRepo.Object,
            mockSettingsRepo.Object,
            mockLogger.Object
        );

        var json = @"{
            ""version"": 1,
            ""exportDate"": ""2024-01-01T00:00:00Z"",
            ""settings"": {
                ""focusDuration"": 25,
                ""shortBreakDuration"": 5,
                ""longBreakDuration"": 15,
                ""sessionsUntilLongBreak"": 4
            },
            ""activities"": [],
            ""tasks"": [
                {
                    ""id"": ""00000000-0000-0000-0000-000000000000"",
                    ""name"": ""Existing Task"",
                    ""createdAt"": ""2024-01-01T10:00:00Z"",
                    ""pomodoroCount"": 5,
                    ""totalFocusMinutes"": 125,
                    ""isCompleted"": false,
                    ""isDeleted"": false
                }
            ]
        }";

        // Act
        var result = await service.ImportFromJsonAsync(json);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(1, result.TasksSkipped);
    }

    [Fact]
    public void ActivityKey_PropertyAccess_ExercisePropertyGetters()
    {
        // Arrange
        var activityKey = new ActivityKey(
            SessionType.Pomodoro,
            DateTime.UtcNow,
            25,
            "Test Task"
        );

        // Act - Directly access all properties to exercise property getters
        var type = activityKey.Type;
        var completedAt = activityKey.CompletedAt;
        var durationMinutes = activityKey.DurationMinutes;
        var taskName = activityKey.TaskName;

        // Assert - Property values are as expected
        Assert.Equal(SessionType.Pomodoro, type);
        Assert.Equal(25, durationMinutes);
        Assert.Equal("Test Task", taskName);
    }

    [Fact]
    public void TaskKey_PropertyAccess_ExercisePropertyGetters()
    {
        // Arrange
        var taskKey = new TaskKey(
            "Test Task",
            DateTime.UtcNow
        );

        // Act - Directly access all properties to exercise property getters
        var name = taskKey.Name;
        var createdAt = taskKey.CreatedAt;

        // Assert - Property values are as expected
        Assert.Equal("Test Task", name);
        Assert.NotEqual(default, createdAt);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ExceptionHandling_CoverageLines260To263()
    {
        // Arrange
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ExportService>>();

        mockActivityRepo
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Simulated repository error"));

        var service = new ExportService(
            mockActivityRepo.Object,
            mockTaskRepo.Object,
            mockSettingsRepo.Object,
            mockLogger.Object
        );

        var json = @"{
            ""version"": 1,
            ""exportDate"": ""2024-01-01T00:00:00Z"",
            ""settings"": {
                ""focusDuration"": 25,
                ""shortBreakDuration"": 5,
                ""longBreakDuration"": 15,
                ""sessionsUntilLongBreak"": 4
            },
            ""activities"": [],
            ""tasks"": []
        }";

        // Act
        var result = await service.ImportFromJsonAsync(json);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_InvalidVersion_CoverageLines101To103()
    {
        // Arrange
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockTaskRepo = new Mock<ITaskRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        var mockLogger = new Mock<ILogger<ExportService>>();

        mockActivityRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Array.Empty<ActivityRecord>().ToList());

        mockTaskRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Array.Empty<TaskItem>().ToList());

        var service = new ExportService(
            mockActivityRepo.Object,
            mockTaskRepo.Object,
            mockSettingsRepo.Object,
            mockLogger.Object
        );

        // Use null JSON which will deserialize to null for ExportData
        var json = @"null";

        // Act
        var result = await service.ImportFromJsonAsync(json);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }
}

