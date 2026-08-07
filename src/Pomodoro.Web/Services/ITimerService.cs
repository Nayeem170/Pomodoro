using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

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
    /// <value>
    /// A <see cref="TimerSession"/> containing session details, or <c>null</c>.
    /// </value>
    TimerSession? CurrentSession { get; }

    /// <value>
    /// A <see cref="TimerSettings"/> instance with duration and behavior settings.
    /// </value>
    TimerSettings Settings { get; }

    /// <value>
    /// <c>true</c> if the timer is actively counting down; otherwise, <c>false</c>.
    /// </value>
    bool IsRunning { get; }

    /// <value>
    /// <c>true</c> if the timer is paused (started but not running); otherwise, <c>false</c>.
    /// </value>
    bool IsPaused { get; }

    /// <value>
    /// <c>true</c> if the timer has been started at least once; otherwise, <c>false</c>.
    /// </value>
    bool IsStarted { get; }

    /// <value>
    /// A <see cref="SessionType"/> value indicating Pomodoro, ShortBreak, or LongBreak.
    /// </value>
    SessionType CurrentSessionType { get; }

    /// <value>
    /// The remaining time in the current session.
    /// </value>
    TimeSpan RemainingTime { get; }

    /// <value>
    /// The number of seconds remaining in the current session.
    /// </value>
    int RemainingSeconds { get; }

    /// <returns>A task that completes when initialization is finished.</returns>
    /// <remarks>
    /// This method should be called during application startup to load settings
    /// and prepare the JavaScript interop for timer operations.
    /// </remarks>
    Task InitializeAsync();

    /// <param name="taskId">
    /// Optional task ID to associate with this Pomodoro session.
    /// If provided, the session will be tracked under the specified task.
    /// </param>
    /// <returns>A task that completes when the session has started.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no task is selected and task tracking is required.
    /// </exception>
    Task StartPomodoroAsync(Guid? taskId = null);

    /// <returns>A task that completes when the session has started.</returns>
    Task StartShortBreakAsync();

    /// <returns>A task that completes when the session has started.</returns>
    Task StartLongBreakAsync();

    /// <param name="sessionType">The type of session to switch to.</param>
    /// <returns>A task that completes when the session switch is complete.</returns>
    /// <remarks>
    /// This method will stop any running timer and start a new session of the specified type.
    /// </remarks>
    Task SwitchSessionTypeAsync(SessionType sessionType);

    /// <returns>A task that completes when the timer is paused.</returns>
    /// <remarks>
    /// If the timer is not running, this method has no effect.
    /// </remarks>
    Task PauseAsync();

    /// <returns>A task that completes when the timer is resumed.</returns>
    /// <remarks>
    /// If the timer is not paused, this method has no effect.
    /// </remarks>
    Task ResumeAsync();

    /// <returns>A task that completes when the timer is reset.</returns>
    /// <remarks>
    /// This stops the timer and resets the remaining time to the configured duration
    /// for the current session type.
    /// </remarks>
    Task ResetAsync();

    Task<bool> TryRecordPartialSessionAsync();

    /// <param name="settings">The new settings to apply.</param>
    /// <returns>A task that completes when settings are updated.</returns>
    /// <remarks>
    /// Settings are applied immediately but not persisted until <see cref="SaveSettingsAsync"/> is called.
    /// </remarks>
    Task UpdateSettingsAsync(TimerSettings settings);

    /// <returns>A task that completes when settings are saved.</returns>
    Task SaveSettingsAsync();

    TimerSession? InterruptedPomodoro { get; }

    /// <returns>A task that completes when the session has been resumed.</returns>
    Task ResumeInterruptedPomodoroAsync();
}
