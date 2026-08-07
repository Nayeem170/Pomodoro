using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

public interface ISettingsRepository
{
    Task<TimerSettings?> GetAsync();

    Task<bool> SaveAsync(TimerSettings settings);

    Task ResetToDefaultsAsync();
}
