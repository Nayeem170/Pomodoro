using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.Repositories;

[Trait("Category", "Service")]
public class PomodoroMetaRepositoryTests
{
    private readonly Mock<IIndexedDbService> _mockIndexedDb;
    private readonly Mock<ILogger<PomodoroMetaRepository>> _mockLogger;

    public PomodoroMetaRepositoryTests()
    {
        _mockIndexedDb = new Mock<IIndexedDbService>();
        _mockLogger = new Mock<ILogger<PomodoroMetaRepository>>();
    }

    private PomodoroMetaRepository CreateRepository()
        => new(_mockIndexedDb.Object, _mockLogger.Object);

    [Fact]
    public async Task GetAsync_ReturnsMeta()
    {
        var expected = new PomodoroMeta("gtask-1", 5, 125, Priority.High);
        _mockIndexedDb
            .Setup(x => x.GetAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore, "gtask-1"))
            .ReturnsAsync(expected);

        var repo = CreateRepository();
        var result = await repo.GetAsync("gtask-1");

        Assert.NotNull(result);
        Assert.Equal("gtask-1", result!.GoogleTaskId);
        Assert.Equal(5, result.PomodoroCount);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNotFound()
    {
        _mockIndexedDb
            .Setup(x => x.GetAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore, "missing"))
            .ReturnsAsync((PomodoroMeta?)null);

        var repo = CreateRepository();
        var result = await repo.GetAsync("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_InvokesPutAsync()
    {
        var meta = new PomodoroMeta("gtask-1", 1, 25, Priority.None);
        _mockIndexedDb
            .Setup(x => x.PutAsync(Constants.Storage.PomoMetaStore, meta))
            .ReturnsAsync(true);

        var repo = CreateRepository();
        await repo.SaveAsync(meta);

        _mockIndexedDb.Verify(x => x.PutAsync(Constants.Storage.PomoMetaStore, meta), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_InvokesDeleteAsync()
    {
        _mockIndexedDb
            .Setup(x => x.DeleteAsync(Constants.Storage.PomoMetaStore, "gtask-1"))
            .ReturnsAsync(true);

        var repo = CreateRepository();
        await repo.DeleteAsync("gtask-1");

        _mockIndexedDb.Verify(x => x.DeleteAsync(Constants.Storage.PomoMetaStore, "gtask-1"), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMeta()
    {
        var list = new List<PomodoroMeta>
        {
            new("gtask-1", 5, 125, Priority.High),
            new("gtask-2", 2, 50, Priority.Med)
        };
        _mockIndexedDb
            .Setup(x => x.GetAllAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore))
            .ReturnsAsync(list);

        var repo = CreateRepository();
        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("gtask-1", result[0].GoogleTaskId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNull()
    {
        _mockIndexedDb
            .Setup(x => x.GetAllAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore))
            .ReturnsAsync((List<PomodoroMeta>?)null);

        var repo = CreateRepository();
        var result = await repo.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ClearAllAsync_InvokesClearAsync()
    {
        _mockIndexedDb
            .Setup(x => x.ClearAsync(Constants.Storage.PomoMetaStore))
            .ReturnsAsync(true);

        var repo = CreateRepository();
        await repo.ClearAllAsync();

        _mockIndexedDb.Verify(x => x.ClearAsync(Constants.Storage.PomoMetaStore), Times.Once);
    }
}
