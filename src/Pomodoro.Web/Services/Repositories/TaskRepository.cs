using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository implementation for task persistence using IndexedDB
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly IIndexedDbService _indexedDb;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(IIndexedDbService indexedDb, ILogger<TaskRepository> logger)
    {
        _indexedDb = indexedDb;
        _logger = logger;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        var all = await _indexedDb.GetAllAsync<TaskItem>(Constants.Storage.TasksStore);
        return all?.Where(t => !t.IsDeleted).ToList() ?? new List<TaskItem>();
    }

    public async Task<List<TaskItem>> GetAllIncludingDeletedAsync()
    {
        var all = await _indexedDb.GetAllAsync<TaskItem>(Constants.Storage.TasksStore);
        return all ?? new List<TaskItem>();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _indexedDb.GetAsync<TaskItem>(Constants.Storage.TasksStore, id.ToString());
    }

    public async Task<bool> SaveAsync(TaskItem task)
    {
        var success = await _indexedDb.PutAsync(Constants.Storage.TasksStore, task);
        if (!success)
        {
            _logger.LogWarning(Constants.Messages.LogFailedToSaveTask, task.Id);
        }
        return success;
    }

    public async Task<int> GetCountAsync()
    {
        var all = await GetAllAsync();
        return all.Count;
    }

    public async Task ClearAllAsync()
    {
        await _indexedDb.ClearAsync(Constants.Storage.TasksStore);
    }
}
