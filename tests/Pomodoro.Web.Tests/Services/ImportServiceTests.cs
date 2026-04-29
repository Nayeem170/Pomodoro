using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class ImportServiceTests
{
    private readonly Mock<IActivityRepository> _mockActivityRepository;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<ILogger<ImportService>> _mockLogger;
    private readonly ImportService _service;

    public ImportServiceTests()
    {
        _mockActivityRepository = new Mock<IActivityRepository>();
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockLogger = new Mock<ILogger<ImportService>>();

        _service = new ImportService(
            _mockActivityRepository.Object,
            _mockTaskRepository.Object,
            _mockSettingsRepository.Object,
            _mockLogger.Object);
    }

    #region Validation

    [Fact]
    public async Task ImportFromJsonAsync_NullInput_ReturnsFailure()
    {
        var result = await _service.ImportFromJsonAsync(null!);

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorEmptyFile, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_EmptyString_ReturnsFailure()
    {
        var result = await _service.ImportFromJsonAsync("");

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorEmptyFile, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WhitespaceInput_ReturnsFailure()
    {
        var result = await _service.ImportFromJsonAsync("   ");

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorEmptyFile, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_InvalidJson_ReturnsFailure()
    {
        var result = await _service.ImportFromJsonAsync("not json at all");

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorInvalidJson, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_EmptyJsonObject_ReturnsFailure()
    {
        var result = await _service.ImportFromJsonAsync("{}");

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorInvalidVersion, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ZeroVersion_ReturnsFailure()
    {
        var json = JsonSerializer.Serialize(new { Version = 0 });
        var result = await _service.ImportFromJsonAsync(json);

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorInvalidVersion, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_NegativeVersion_ReturnsFailure()
    {
        var json = JsonSerializer.Serialize(new { Version = -1 });
        var result = await _service.ImportFromJsonAsync(json);

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorInvalidVersion, result.ErrorMessage);
    }

    #endregion

    #region ImportFromStringAsync

    [Fact]
    public async Task ImportFromStringAsync_DelegatesToImportFromJsonAsync()
    {
        var json = CreateValidJson();
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.ImportFromStringAsync(json);

        Assert.True(result.Success);
    }

    #endregion

    #region Successful Import

    [Fact]
    public async Task ImportFromJsonAsync_ValidEmptyData_ReturnsSuccess()
    {
        var json = CreateValidJson();
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.False(result.SettingsImported);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithSettings_ImportsSettings()
    {
        var settings = new TimerSettings { PomodoroMinutes = 30 };
        var json = CreateValidJson(settings: settings);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockSettingsRepository.Setup(r => r.SaveAsync(It.IsAny<TimerSettings>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.True(result.SettingsImported);
        _mockSettingsRepository.Verify(r => r.SaveAsync(It.Is<TimerSettings>(s => s.PomodoroMinutes == 30)), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithTasks_ImportsNewTasks()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            CreatedAt = DateTime.UtcNow,
            PomodoroCount = 3,
            TotalFocusMinutes = 75
        };
        var json = CreateValidJson(tasks: [task]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(1, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithActivities_ImportsNewActivities()
    {
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25
        };
        var json = CreateValidJson(activities: [activity]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithMixedData_ImportsAll()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25 };
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow };
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25
        };
        var json = CreateValidJson(settings, [activity], [task]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockSettingsRepository.Setup(r => r.SaveAsync(It.IsAny<TimerSettings>())).ReturnsAsync(true);
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);
        _mockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.True(result.SettingsImported);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(1, result.TasksImported);
    }

    #endregion

    #region Duplicate Detection

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateTask_SkipsTask()
    {
        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Existing Task",
            CreatedAt = DateTime.UtcNow
        };
        var importTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Existing Task",
            CreatedAt = existingTask.CreatedAt
        };
        var json = CreateValidJson(tasks: [importTask]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([existingTask]);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(1, result.TasksSkipped);
        _mockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateActivity_SkipsActivity()
    {
        var existingActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25
        };
        var importActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = existingActivity.CompletedAt,
            DurationMinutes = 25
        };
        var json = CreateValidJson(activities: [importActivity]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([existingActivity]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(1, result.ActivitiesSkipped);
        _mockActivityRepository.Verify(r => r.SaveAsync(It.IsAny<ActivityRecord>()), Times.Never);
    }

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateTask_MapsOldIdToExistingId()
    {
        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Shared Task",
            CreatedAt = DateTime.UtcNow
        };
        var importTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Shared Task",
            CreatedAt = existingTask.CreatedAt
        };
        var importActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            TaskId = importTask.Id
        };
        var json = CreateValidJson(activities: [importActivity], tasks: [importTask]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([existingTask]);
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(1, result.ActivitiesImported);
        _mockActivityRepository.Verify(r => r.SaveAsync(
            It.Is<ActivityRecord>(a => a.TaskId == existingTask.Id)), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ActivityWithExistingTaskId_KeepsOriginalId()
    {
        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Other Task",
            CreatedAt = DateTime.UtcNow
        };
        var importActivity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            TaskId = existingTask.Id
        };
        var json = CreateValidJson(activities: [importActivity]);
        _mockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _mockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([existingTask]);
        _mockActivityRepository.Setup(r => r.SaveAsync(It.IsAny<ActivityRecord>())).ReturnsAsync(true);

        var result = await _service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        _mockActivityRepository.Verify(r => r.SaveAsync(
            It.Is<ActivityRecord>(a => a.TaskId == existingTask.Id)), Times.Once);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ImportFromJsonAsync_RepositoryThrows_ReturnsFailure()
    {
        var json = CreateValidJson();
        _mockActivityRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("DB error"));

        var result = await _service.ImportFromJsonAsync(json);

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorFailed, result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJsonAsync_NullJson_ReturnsInvalidFormat()
    {
        var result = await _service.ImportFromJsonAsync("null");

        Assert.False(result.Success);
        Assert.Equal(Constants.Messages.ImportErrorInvalidFormat, result.ErrorMessage);
    }

    #endregion

    #region Helpers

    private static string CreateValidJson(
        TimerSettings? settings = null,
        List<ActivityRecord>? activities = null,
        List<TaskItem>? tasks = null)
    {
        var data = new
        {
            Version = 1,
            ExportDate = DateTime.UtcNow,
            Settings = settings,
            Activities = activities,
            Tasks = tasks
        };
        return JsonSerializer.Serialize(data);
    }

    #endregion
}
