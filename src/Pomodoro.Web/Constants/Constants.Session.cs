namespace Pomodoro.Web;

public static partial class Constants
{
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

    public static class SessionOptionLabels
    {
        public const string ShortBreak = "Short Break";
        public const string LongBreak = "Long Break";
        public const string AnotherPomodoro = "Another Pomodoro";
        public const string StartPomodoro = "Start Pomodoro";
        public const string ContinueShortBreak = "Continue Short Break";
        public const string ContinueLongBreak = "Continue Long Break";
    }

    public static class Tasks
    {
        public const string CompletedEmoji = "✅";
        public const string HasPomodorosEmoji = "📋";
        public const string DefaultEmoji = "📝";

        // CSS Classes
        public const string SelectedClass = "selected";
        public const string CompletedClass = "completed";
        public const string NewlyAddedClass = "task-row--new";

        // Initial Values
        public const int InitialFocusMinutes = 0;
        public const int InitialPomodoroCount = 0;
        public const int InitialCount = 0;
        public const int InsertAtBeginning = 0;
        public const int InsertAtEnd = -1;
        public const int MaxSubtaskDepth = 4;
        public const int SortGap = 1000;
        public const int InitialSortStep = 1000;
        public const int ScheduleWindowDays = 7;
        public const string ScheduleDayFormat = "ddd, MMM d";
    }

    public static class TaskLists
    {
        public const string LocalPomodoroListId = "__local_pomodoro__";
        public const string ScheduleListId = "__schedule__";
    }

    public static class Repeat
    {
        public const string RepeatIcon = "🔁";
        public const string ScheduleIcon = "📅";
        public const string PausedIcon = "⏸️";
        public const string RepeatCssClass = "task-repeat";
        public const string ScheduleCssClass = "task-scheduled";
        public const string PausedCssClass = "repeat-paused";
        public const int DefaultCustomDays = 1;
        public const int DefaultMonthlyDay = 1;
        public const int MaxCustomDays = 365;
        public const string LabelDaily = "Daily";
        public const string LabelWeekly = "Weekly";
        public const string LabelMonthly = "Monthly";
        public const string LabelQuarterly = "Quarterly";
        public const string LabelYearly = "Yearly";
        public const string LabelRepeat = "Repeat";
        public const string FollowParentChoice = "follow-parent";
        public const string MonthEndClampHint = "Runs on the last day of shorter months.";
        public static readonly string[] QuarterlyGroupLabels =
            ["Jan, Apr, Jul, Oct", "Feb, May, Aug, Nov", "Mar, Jun, Sep, Dec"];
        public static readonly string[] MonthNames =
            ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    }
}
