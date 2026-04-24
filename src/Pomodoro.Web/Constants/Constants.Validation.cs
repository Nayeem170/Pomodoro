namespace Pomodoro.Web;

/// <summary>
/// Validation, error handling, and logging constants
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Validation-related constants
    /// </summary>
    public static class Validation
    {
        /// <summary>Maximum length for task names</summary>
        public const int MaxTaskNameLength = 500;

        /// <summary>Validation message for required task name</summary>
        public const string TaskNameRequiredMessage = "Task name is required";

        /// <summary>Validation message for max task name length</summary>
        public const string TaskNameMaxLengthMessage = "Task name cannot exceed 500 characters";

        /// <summary>Maximum import file size in bytes (10 MB)</summary>
        public const int MaxImportFileSizeBytes = 10 * 1024 * 1024;
    }

    /// <summary>
    /// Error display related constants
    /// </summary>
    public static class ErrorDisplay
    {
        /// <summary>Error icon emoji</summary>
        public const string ErrorIcon = "⚠️";

        /// <summary>Error page title</summary>
        public const string ErrorTitle = "Something went wrong";

        /// <summary>Default error message when no exception is provided</summary>
        public const string DefaultErrorMessage = "An unexpected error occurred.";

        /// <summary>Retry button text</summary>
        public const string RetryButtonText = "Try Again";

        /// <summary>Reload button text</summary>
        public const string ReloadButtonText = "Reload Page";
    }

    /// <summary>
    /// SafeTaskRunner operation names for logging
    /// </summary>
    public static class SafeTaskOperations
    {
        /// <summary>Default operation name when none provided</summary>
        public const string UnknownOperation = "Unknown operation";

        /// <summary>Check pending notification action operation</summary>
        public const string CheckPendingNotificationAction = "CheckPendingNotificationAction";

        /// <summary>PiP timer tick update operation</summary>
        public const string PipTimerTick = "PipTimer OnTimerTick";

        /// <summary>PiP timer state change operation</summary>
        public const string PipTimerStateChanged = "PipTimer OnTimerStateChanged";

        /// <summary>Timer complete operation</summary>
        public const string TimerComplete = "TimerService OnTimerComplete";

        /// <summary>Consent service timer complete handler operation</summary>
        public const string ConsentTimerComplete = "ConsentService HandleTimerComplete";

        /// <summary>Toast hide operation for settings page</summary>
        public const string ToastHide = "Settings ToastHide";

        /// <summary>Keyboard shortcut: play/pause toggle</summary>
        public const string KeyboardShortcutPlayPause = "KeyboardShortcut PlayPause";

        /// <summary>Keyboard shortcut: reset timer</summary>
        public const string KeyboardShortcutReset = "KeyboardShortcut Reset";

        /// <summary>Keyboard shortcut: start pomodoro</summary>
        public const string KeyboardShortcutPomodoro = "KeyboardShortcut Pomodoro";

        /// <summary>Keyboard shortcut: start short break</summary>
        public const string KeyboardShortcutShortBreak = "KeyboardShortcut ShortBreak";

        /// <summary>Keyboard shortcut: start long break</summary>
        public const string KeyboardShortcutLongBreak = "KeyboardShortcut LongBreak";

        public const string CloudSyncPush = "CloudSync Push";
        public const string CloudSyncPull = "CloudSync Pull";
        public const string CloudSyncAutoSync = "CloudSync BackgroundSync";
        public const string CloudSyncPeriodic = "CloudSync PeriodicSync";

        /// <summary>Log message format for errors in operations</summary>
        public const string ErrorInOperationLogFormat = "Error in {OperationName}";
    }

    /// <summary>
    /// Logging-related constants
    /// </summary>
    public static class Logging
    {
        /// <summary>Category filter for Microsoft framework logs</summary>
        public const string MicrosoftCategory = "Microsoft";

        /// <summary>Category filter for System logs</summary>
        public const string SystemCategory = "System";
    }
}
