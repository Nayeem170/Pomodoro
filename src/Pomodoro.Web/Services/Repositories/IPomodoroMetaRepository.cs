using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

public interface IPomodoroMetaRepository
{
    Task<PomodoroMeta?> GetAsync(string googleTaskId);
    Task SaveAsync(PomodoroMeta meta);
    Task DeleteAsync(string googleTaskId);
    Task<IReadOnlyList<PomodoroMeta>> GetAllAsync();
    Task ClearAllAsync();
}
