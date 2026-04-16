namespace Pomodoro.Web;

/// <summary>
/// Session type, task, and session option constants
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Session type display names and styling
    /// </summary>
    public static class SessionTypes
    {
        // Display Names
        public const string PomodoroDisplayName = "Pomodoro";
        public const string ShortBreakDisplayName = "Short Break";
        public const string LongBreakDisplayName = "Long Break";

        // Uppercase Display
        public const string PomodoroUppercase = "POMODORO";
        public const string ShortBreakUppercase = "SHORT BREAK";
        public const string LongBreakUppercase = "LONG BREAK";

        // CSS Classes for sections (with gradient backgrounds)
        public const string PomodoroTheme = "pomodoro-theme";
        public const string ShortBreakTheme = "short-break-theme";
        public const string LongBreakTheme = "long-break-theme";

        // CSS Classes for timer display colors (without -theme suffix, matches PIP)
        public const string PomodoroClass = "pomodoro";
        public const string ShortBreakClass = "short-break";
        public const string LongBreakClass = "long-break";

        // CSS State Classes
        public const string PausedState = "paused";

        // Emojis
        public const string PomodoroEmoji = "🍅";
        public const string ShortBreakEmoji = "☕";
        public const string LongBreakEmoji = "🏖️";
        public const string TimerEmoji = "⏱️";

        // Activity Names
        public const string FocusTimeActivity = "Focus time";
        public const string ShortBreakActivity = "Short break";
        public const string LongBreakActivity = "Long break";
        public const string UnknownActivity = "Unknown";

        // Notification Actions
        public const string ActionShortBreak = "short-break";
        public const string ActionLongBreak = "long-break";
        public const string ActionStartPomodoro = "start-pomodoro";
        public const string ActionSkip = "skip";
    }

    /// <summary>
    /// Session option labels
    /// </summary>
    public static class SessionOptionLabels
    {
        public const string ShortBreak = "Short Break";
        public const string LongBreak = "Long Break";
        public const string AnotherPomodoro = "Another Pomodoro";
        public const string StartPomodoro = "Start Pomodoro";
    }

    /// <summary>
    /// Task-related constants
    /// </summary>
    public static class Tasks
    {
        public const string CompletedEmoji = "✅";
        public const string HasPomodorosEmoji = "📋";
        public const string DefaultEmoji = "📝";

        // CSS Classes
        public const string SelectedClass = "selected";
        public const string CompletedClass = "completed";

        // Initial Values
        public const int InitialFocusMinutes = 0;
        public const int InitialPomodoroCount = 0;
        public const int InitialCount = 0;
        public const int InsertAtBeginning = 0;
    }
}
