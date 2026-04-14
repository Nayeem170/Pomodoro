using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository implementation for activity record persistence using IndexedDB
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly IIndexedDbService _indexedDb;
    private readonly ILogger<ActivityRepository> _logger;

    public ActivityRepository(IIndexedDbService indexedDb, ILogger<ActivityRepository> logger)
    {
        _indexedDb = indexedDb;
        _logger = logger;
    }

    public async Task<List<ActivityRecord>> GetAllAsync()
    {
        var all = await _indexedDb.GetAllAsync<ActivityRecord>(Constants.Storage.ActivitiesStore);
        return all?.OrderByDescending(a => a.CompletedAt).ToList() ?? new List<ActivityRecord>();
    }

    public async Task<List<ActivityRecord>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        var fromUtc = start.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime() : start;
        var toUtc = end.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(end, DateTimeKind.Local).ToUniversalTime() : end;
        var fromDate = fromUtc.ToString(Constants.DateFormats.IsoFormat);
        var toDate = toUtc.ToString(Constants.DateFormats.IsoFormat);
        
        var activities = await _indexedDb.QueryByDateRangeAsync<ActivityRecord>(
            Constants.Storage.ActivitiesStore,
            Constants.Storage.CompletedAtIndex,
            fromDate,
            toDate);
        
        return activities?.OrderByDescending(a => a.CompletedAt).ToList() ?? new List<ActivityRecord>();
    }

    public async Task<List<ActivityRecord>> GetPagedAsync(DateTime start, DateTime end, int skip, int take)
    {
        var all = await GetByDateRangeAsync(start, end);
        return all.Skip(skip).Take(take).ToList();
    }

    public async Task<ActivityRecord?> GetByIdAsync(Guid id)
    {
        return await _indexedDb.GetAsync<ActivityRecord>(Constants.Storage.ActivitiesStore, id.ToString());
    }

    public async Task<bool> SaveAsync(ActivityRecord activity)
    {
        var success = await _indexedDb.PutAsync(Constants.Storage.ActivitiesStore, activity);
        if (!success)
        {
            _logger.LogWarning(Constants.Messages.LogFailedToSaveActivity, activity.Id);
        }
        return success;
    }

    public async Task<int> GetCountAsync(DateTime? start = null, DateTime? end = null)
    {
        if (start.HasValue && end.HasValue)
        {
            var activities = await GetByDateRangeAsync(start.Value, end.Value);
            return activities.Count;
        }
        
        var all = await GetAllAsync();
        return all.Count;
    }

    public async Task<bool> ClearAllAsync()
    {
        await _indexedDb.ClearAsync(Constants.Storage.ActivitiesStore);
        return true;
    }
}
