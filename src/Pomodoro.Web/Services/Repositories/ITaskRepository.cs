using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository interface for task persistence operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets all tasks (excluding soft-deleted)
    /// </summary>
    Task<List<TaskItem>> GetAllAsync();

    /// <summary>
    /// Gets all tasks including soft-deleted
    /// </summary>
    Task<List<TaskItem>> GetAllIncludingDeletedAsync();

    /// <summary>
    /// Gets a task by ID
    /// </summary>
    Task<TaskItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// Saves a task (insert or update)
    /// </summary>
    Task<bool> SaveAsync(TaskItem task);

    /// <summary>
    /// Gets the count of all tasks (excluding soft-deleted)
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Clears all tasks from the store
    /// </summary>
    Task ClearAllAsync();
}
