using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();

    Task<List<TaskItem>> GetAllIncludingDeletedAsync();

    Task<TaskItem?> GetByIdAsync(Guid id);

    Task<bool> SaveAsync(TaskItem task);

    Task<int> GetCountAsync();

    Task ClearAllAsync();

    Task<IReadOnlyList<TaskItem>> GetByGoogleListIdAsync(string listId);

    Task<TaskItem?> GetByGoogleTaskIdAsync(string googleTaskId);
}
