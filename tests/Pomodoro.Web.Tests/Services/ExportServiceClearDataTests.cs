using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class ExportServiceClearDataTests
{
    private readonly Mock<IActivityRepository> _mockActivityRepo;
    private readonly Mock<ITaskRepository> _mockTaskRepo;
    private readonly Mock<ISettingsRepository> _mockSettingsRepo;
    private readonly Mock<ILogger<ExportService>> _mockLogger;
    private readonly ExportService _service;

    public ExportServiceClearDataTests()
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
    public async Task ClearAllDataAsync_ClearsActivitiesAndTasksAndResetsSettings()
    {
        _mockActivityRepo.Setup(r => r.GetCountAsync(null, null)).ReturnsAsync(5);
        _mockTaskRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(3);
        _mockSettingsRepo.Setup(r => r.SaveAsync(It.IsAny<TimerSettings>())).ReturnsAsync(true);

        await _service.ClearAllDataAsync();

        _mockActivityRepo.Verify(r => r.ClearAllAsync(), Times.Once);
        _mockTaskRepo.Verify(r => r.ClearAllAsync(), Times.Once);
        _mockSettingsRepo.Verify(r => r.SaveAsync(It.IsAny<TimerSettings>()), Times.Once);
    }

    [Fact]
    public async Task ClearAllDataAsync_ThrowsOnActivityFailure()
    {
        _mockActivityRepo.Setup(r => r.GetCountAsync(null, null)).ReturnsAsync(1);
        _mockActivityRepo.Setup(r => r.ClearAllAsync()).ThrowsAsync(new Exception("DB error"));

        var ex = await Record.ExceptionAsync(() => _service.ClearAllDataAsync());
        Assert.NotNull(ex);
        Assert.Equal("DB error", ex.Message);
    }

    [Fact]
    public async Task ClearAllDataAsync_ThrowsOnSettingsFailure()
    {
        _mockActivityRepo.Setup(r => r.GetCountAsync(null, null)).ReturnsAsync(1);
        _mockTaskRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(1);
        _mockSettingsRepo.Setup(r => r.SaveAsync(It.IsAny<TimerSettings>())).ThrowsAsync(new Exception("DB error"));

        var ex = await Record.ExceptionAsync(() => _service.ClearAllDataAsync());
        Assert.NotNull(ex);
        Assert.Equal("DB error", ex.Message);
    }
}
