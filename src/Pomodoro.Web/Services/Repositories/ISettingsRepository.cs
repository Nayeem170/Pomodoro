using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository interface for settings persistence operations
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets the current timer settings
    /// </summary>
    Task<TimerSettings?> GetAsync();

    /// <summary>
    /// Saves timer settings
    /// </summary>
    Task<bool> SaveAsync(TimerSettings settings);

    /// <summary>
    /// Resets settings to defaults
    /// </summary>
    Task ResetToDefaultsAsync();
}
