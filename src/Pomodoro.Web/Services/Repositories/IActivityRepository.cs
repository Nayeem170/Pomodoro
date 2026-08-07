using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

public interface IActivityRepository
{
    Task<List<ActivityRecord>> GetAllAsync();

    Task<List<ActivityRecord>> GetByDateRangeAsync(DateTime start, DateTime end);

    Task<List<ActivityRecord>> GetPagedAsync(DateTime start, DateTime end, int skip, int take);

    Task<ActivityRecord?> GetByIdAsync(Guid id);

    Task<bool> SaveAsync(ActivityRecord activity);

    Task<int> GetCountAsync(DateTime? start = null, DateTime? end = null);

    Task<bool> ClearAllAsync();
}
