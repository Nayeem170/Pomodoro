using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for Picture-in-Picture timer service
/// Provides a floating, always-on-top timer window
/// </summary>
public interface IPipTimerService : IAsyncDisposable
{
    /// <summary>
    /// Gets whether the Document Picture-in-Picture API is supported
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets whether the PiP window is currently open
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Event raised when PiP window is opened
    /// </summary>
    event Action? OnPipOpened;

    /// <summary>
    /// Event raised when PiP window is closed
    /// </summary>
    event Action? OnPipClosed;

    /// <summary>
    /// Opens the PiP timer window
    /// </summary>
    /// <returns>True if successfully opened</returns>
    Task<bool> OpenAsync();

    /// <summary>
    /// Closes the PiP timer window
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// Updates the timer display in the PiP window
    /// </summary>
    Task UpdateTimerAsync();

    /// <summary>
    /// Initializes the service (called from component)
    /// </summary>
    Task InitializeAsync();
}
