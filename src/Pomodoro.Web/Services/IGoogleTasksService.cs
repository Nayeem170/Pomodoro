using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IGoogleTasksService
{
    Task<IReadOnlyList<GoogleTaskList>> GetTaskListsAsync();
    Task<IReadOnlyList<GoogleTask>> GetTasksAsync(string listId, string? updatedMin = null);
    Task<bool> IsConnectedAsync();
}
