using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

public class PomodoroMetaRepository : IPomodoroMetaRepository
{
    private readonly IIndexedDbService _indexedDb;
    private readonly ILogger<PomodoroMetaRepository> _logger;

    public PomodoroMetaRepository(IIndexedDbService indexedDb, ILogger<PomodoroMetaRepository> logger)
    {
        _indexedDb = indexedDb;
        _logger = logger;
    }

    public async Task<PomodoroMeta?> GetAsync(string googleTaskId)
    {
        return await _indexedDb.GetAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore, googleTaskId);
    }

    public async Task SaveAsync(PomodoroMeta meta)
    {
        await _indexedDb.PutAsync(Constants.Storage.PomoMetaStore, meta);
    }

    public async Task DeleteAsync(string googleTaskId)
    {
        await _indexedDb.DeleteAsync(Constants.Storage.PomoMetaStore, googleTaskId);
    }

    public async Task<IReadOnlyList<PomodoroMeta>> GetAllAsync()
    {
        var all = await _indexedDb.GetAllAsync<PomodoroMeta>(Constants.Storage.PomoMetaStore);
        return all ?? new List<PomodoroMeta>();
    }

    public async Task ClearAllAsync()
    {
        await _indexedDb.ClearAsync(Constants.Storage.PomoMetaStore);
    }
}
