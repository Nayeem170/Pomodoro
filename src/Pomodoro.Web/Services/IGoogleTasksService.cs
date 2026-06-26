using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IGoogleTasksService
{
    Task<IReadOnlyList<GoogleTaskList>> GetTaskListsAsync();
    Task<IReadOnlyList<GoogleTask>> GetTasksAsync(string listId, string? updatedMin = null);
    Task<bool> IsConnectedAsync();
    Task<GoogleTask> InsertTaskAsync(string listId, GoogleTask task);
    Task<GoogleTask?> PatchTaskAsync(string listId, string taskId, GoogleTaskPatch updates, string? etag = null);
    Task DeleteTaskAsync(string listId, string taskId);
}
