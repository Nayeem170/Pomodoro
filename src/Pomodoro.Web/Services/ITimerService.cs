using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for timer operations in the Pomodoro application.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides methods and events for managing Pomodoro timer sessions,
/// including work sessions (Pomodoros), short breaks, and long breaks.
/// </para>
/// <para>
/// Implementation should be registered as a scoped service and supports
/// JavaScript interop for browser-based timer functionality.
/// </para>
/// </remarks>
public interface ITimerService
{
    /// <summary>
    /// Event raised on each timer tick (typically every second).
    /// </summary>
    /// <remarks>
    /// Subscribe to this event to update UI elements that display the remaining time.
    /// </remarks>
    event Action? OnTick;
    
    /// <summary>
    /// Event raised when a timer session completes.
    /// </summary>
    /// <remarks>
    /// The <see cref="SessionType"/> parameter indicates which type of session completed.
    /// Use this to trigger notifications and consent modals.
    /// </remarks>
    event Action<SessionType>? OnTimerComplete;
    
    /// <summary>
    /// Event raised when the timer state changes (started, paused, resumed, reset).
    /// </summary>
    /// <remarks>
    /// Subscribe to this event to update UI state indicators and control buttons.
    /// </remarks>
    event Action? OnStateChanged;
    
    /// <summary>
    /// Gets the current timer session information, or null if no session is active.
    /// </summary>
    /// <value>
    /// A <see cref="TimerSession"/> containing session details, or <c>null</c>.
    /// </value>
    TimerSession? CurrentSession { get; }
    
    /// <summary>
    /// Gets the current timer settings.
    /// </summary>
    /// <value>
    /// A <see cref="TimerSettings"/> instance with duration and behavior settings.
    /// </value>
    TimerSettings Settings { get; }
    
    /// <summary>
    /// Gets a value indicating whether the timer is currently running.
    /// </summary>
    /// <value>
    /// <c>true</c> if the timer is actively counting down; otherwise, <c>false</c>.
    /// </value>
    bool IsRunning { get; }
    
    /// <summary>
    /// Gets a value indicating whether the timer is paused.
    /// </summary>
    /// <value>
    /// <c>true</c> if the timer is paused (started but not running); otherwise, <c>false</c>.
    /// </value>
    bool IsPaused { get; }
    
    /// <summary>
    /// Gets a value indicating whether the timer has been started.
    /// </summary>
    /// <value>
    /// <c>true</c> if the timer has been started at least once; otherwise, <c>false</c>.
    /// </value>
    bool IsStarted { get; }
    
    /// <summary>
    /// Gets the type of the current session.
    /// </summary>
    /// <value>
    /// A <see cref="SessionType"/> value indicating Pomodoro, ShortBreak, or LongBreak.
    /// </value>
    SessionType CurrentSessionType { get; }
    
    /// <summary>
    /// Gets the remaining time as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <value>
    /// The remaining time in the current session.
    /// </value>
    TimeSpan RemainingTime { get; }
    
    /// <summary>
    /// Gets the remaining time in seconds.
    /// </summary>
    /// <value>
    /// The number of seconds remaining in the current session.
    /// </value>
    int RemainingSeconds { get; }
    
    /// <summary>
    /// Initializes the timer service asynchronously.
    /// </summary>
    /// <returns>A task that completes when initialization is finished.</returns>
    /// <remarks>
    /// This method should be called during application startup to load settings
    /// and prepare the JavaScript interop for timer operations.
    /// </remarks>
    Task InitializeAsync();
    
    /// <summary>
    /// Starts a Pomodoro (work) session.
    /// </summary>
    /// <param name="taskId">
    /// Optional task ID to associate with this Pomodoro session.
    /// If provided, the session will be tracked under the specified task.
    /// </param>
    /// <returns>A task that completes when the session has started.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no task is selected and task tracking is required.
    /// </exception>
    Task StartPomodoroAsync(Guid? taskId = null);
    
    /// <summary>
    /// Starts a short break session.
    /// </summary>
    /// <returns>A task that completes when the session has started.</returns>
    Task StartShortBreakAsync();
    
    /// <summary>
    /// Starts a long break session.
    /// </summary>
    /// <returns>A task that completes when the session has started.</returns>
    Task StartLongBreakAsync();
    
    /// <summary>
    /// Switches to a different session type.
    /// </summary>
    /// <param name="sessionType">The type of session to switch to.</param>
    /// <returns>A task that completes when the session switch is complete.</returns>
    /// <remarks>
    /// This method will stop any running timer and start a new session of the specified type.
    /// </remarks>
    Task SwitchSessionTypeAsync(SessionType sessionType);
    
    /// <summary>
    /// Pauses the running timer.
    /// </summary>
    /// <returns>A task that completes when the timer is paused.</returns>
    /// <remarks>
    /// If the timer is not running, this method has no effect.
    /// </remarks>
    Task PauseAsync();
    
    /// <summary>
    /// Resumes a paused timer.
    /// </summary>
    /// <returns>A task that completes when the timer is resumed.</returns>
    /// <remarks>
    /// If the timer is not paused, this method has no effect.
    /// </remarks>
    Task ResumeAsync();
    
    /// <summary>
    /// Resets the timer to the initial state for the current session type.
    /// </summary>
    /// <returns>A task that completes when the timer is reset.</returns>
    /// <remarks>
    /// This stops the timer and resets the remaining time to the configured duration
    /// for the current session type.
    /// </remarks>
    Task ResetAsync();
    
    /// <summary>
    /// Updates the timer settings with new values.
    /// </summary>
    /// <param name="settings">The new settings to apply.</param>
    /// <returns>A task that completes when settings are updated.</returns>
    /// <remarks>
    /// Settings are applied immediately but not persisted until <see cref="SaveSettingsAsync"/> is called.
    /// </remarks>
    Task UpdateSettingsAsync(TimerSettings settings);
    
    /// <summary>
    /// Persists the current settings to storage.
    /// </summary>
    /// <returns>A task that completes when settings are saved.</returns>
    Task SaveSettingsAsync();
}
