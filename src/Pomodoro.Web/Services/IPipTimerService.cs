using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IPipTimerService : IAsyncDisposable
{
    bool IsSupported { get; }

    bool IsOpen { get; }

    event Action? OnPipOpened;

    event Action? OnPipClosed;

    /// <returns>True if successfully opened</returns>
    Task<bool> OpenAsync();

    Task CloseAsync();

    Task UpdateTimerAsync();

    Task InitializeAsync();
}
