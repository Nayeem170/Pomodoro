using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class ExportServiceExportToStringTests
{
    private readonly Mock<IActivityRepository> _mockActivityRepo;
    private readonly Mock<ITaskRepository> _mockTaskRepo;
    private readonly Mock<ISettingsRepository> _mockSettingsRepo;
    private readonly Mock<ILogger<ExportService>> _mockLogger;
    private readonly ExportService _service;

    public ExportServiceExportToStringTests()
    {
        _mockActivityRepo = new Mock<IActivityRepository>();
        _mockTaskRepo = new Mock<ITaskRepository>();
        _mockSettingsRepo = new Mock<ISettingsRepository>();
        _mockLogger = new Mock<ILogger<ExportService>>();

        _service = new ExportService(
            _mockActivityRepo.Object,
            _mockTaskRepo.Object,
            _mockSettingsRepo.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExportToJsonStringAsync_ReturnsCompactJson()
    {
        _mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());
        _mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());
        _mockSettingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings());

        var result = await _service.ExportToJsonStringAsync();

        Assert.NotNull(result);
        Assert.DoesNotContain("\n", result);
        var jsonDoc = JsonDocument.Parse(result);
        Assert.True(jsonDoc.RootElement.TryGetProperty("version", out var version));
        Assert.Equal(1, version.GetInt32());
    }

    [Fact]
    public async Task ExportToJsonStringAsync_WithData_ReturnsJsonWithAllData()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = DateTime.UtcNow, DurationMinutes = 25, TaskName = "Task 1" }
        };
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Task 1", CreatedAt = DateTime.UtcNow, PomodoroCount = 3 }
        };

        _mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
        _mockTaskRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mockSettingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(new TimerSettings { PomodoroMinutes = 30 });

        var result = await _service.ExportToJsonStringAsync();

        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        Assert.Equal(1, jsonDoc.RootElement.GetProperty("activities").GetArrayLength());
        Assert.Equal(1, jsonDoc.RootElement.GetProperty("tasks").GetArrayLength());
        Assert.Equal(30, jsonDoc.RootElement.GetProperty("settings").GetProperty("pomodoroMinutes").GetInt32());
    }

    [Fact]
    public async Task ExportToJsonStringAsync_WhenRepositoryThrows_PropagatesException()
    {
        _mockActivityRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ExportToJsonStringAsync());
    }

    [Fact]
    public async Task ExportToJsonStringAsync_WhenRepositoryThrows_LogsError()
    {
        _mockActivityRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Test error"));

        try { await _service.ExportToJsonStringAsync(); Assert.True(false); }
        catch { }

        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
