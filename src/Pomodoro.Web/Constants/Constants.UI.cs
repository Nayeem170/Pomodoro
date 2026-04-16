namespace Pomodoro.Web;

/// <summary>
/// UI-related constants for display, layout, and user interface elements
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Activity-related display strings
    /// </summary>
    public static class Activity
    {
        public const string FocusTimeLabel = "Focus time";
        public const string ShortBreaksLabel = "Short Breaks";
        public const string LongBreaksLabel = "Long Breaks";
    }

    /// <summary>
    /// UI-related constants
    /// </summary>
    public static class UI
    {
        /// <summary>Default consent modal countdown in seconds</summary>
        public const int DefaultConsentCountdownSeconds = 10;

        /// <summary>PiP window width</summary>
        public const int PipWindowWidth = 320;

        /// <summary>PiP window height</summary>
        public const int PipWindowHeight = 240;

        /// <summary>Maximum length for task names (references Validation.MaxTaskNameLength for single source of truth)</summary>
        public const int MaxTaskNameLength = Validation.MaxTaskNameLength;

        /// <summary>Toast notification display duration in milliseconds</summary>
        public const int ToastDurationMs = 2000;

        /// <summary>Percentage multiplier for progress calculations</summary>
        public const int PercentageMultiplier = 100;

        /// <summary>Percent symbol for display</summary>
        public const string PercentSymbol = "%";

        /// <summary>Delay for notification action check in milliseconds</summary>
        public const int NotificationCheckDelayMs = 500;

        /// <summary>History tab CSS class for daily tab</summary>
        public const string HistoryTabDailyClass = "daily";

        /// <summary>History tab CSS class for weekly tab</summary>
        public const string HistoryTabWeeklyClass = "weekly";

        /// <summary>History tab CSS class for active tab</summary>
        public const string HistoryTabActiveClass = "active";

        #region Timer Control Icons

        /// <summary>Play icon for start/resume button</summary>
        public const string PlayIcon = "▶";

        /// <summary>Pause icon for pause button</summary>
        public const string PauseIcon = "⏸";

        /// <summary>Hourglass icon for paused timer state</summary>
        public const string HourglassIcon = "⏱️";

        /// <summary>Reset/rewind icon for reset button</summary>
        public const string ResetIcon = "↺";

        #endregion

        #region Infinite Scroll

        /// <summary>Element ID for the infinite scroll sentinel element</summary>
        public const string InfiniteScrollSentinelId = "scroll-sentinel";

        /// <summary>Element ID for the timeline scroll container</summary>
        public const string TimelineScrollContainerId = "timeline-scroll-container";

        /// <summary>Default timeout for infinite scroll callback in milliseconds (5 seconds)</summary>
        public const int InfiniteScrollTimeoutMs = 5000;

        /// <summary>Default root margin for intersection observer</summary>
        public const string InfiniteScrollRootMargin = "100px";

        #endregion
    }

    /// <summary>
    /// History page tab labels
    /// </summary>
    public static class History
    {
        public const string DailyTabLabel = "Daily";
        public const string WeeklyTabLabel = "Weekly";
        public const string ThisWeekLabel = "This Week";
        public const string WeekRangeFormat = "{0:MMM dd} - {1:MMM dd}, {1:yyyy}";

        /// <summary>Tooltip for previous week navigation button</summary>
        public const string PreviousWeekTitle = "Previous week";

        /// <summary>Tooltip for next week navigation button</summary>
        public const string NextWeekTitle = "Next week";

        /// <summary>Left arrow icon for navigation</summary>
        public const string LeftArrowIcon = "◀";

        /// <summary>Right arrow icon for navigation</summary>
        public const string RightArrowIcon = "▶";
    }

    /// <summary>
    /// Notification-related constants
    /// </summary>
    public static class Notifications
    {
        /// <summary>Notification tag for timer notifications</summary>
        public const string TimerNotificationTag = "pomodoro-timer";

        /// <summary>Auto-close notification timeout in milliseconds</summary>
        public const int NotificationTimeoutMs = 60000;

        /// <summary>Countdown interval in seconds for consent modal</summary>
        public const int CountdownIntervalSeconds = 1;
    }

    /// <summary>
    /// Blazor framework-related constants
    /// </summary>
    public static class Blazor
    {
        /// <summary>Root component selector for the main app</summary>
        public const string AppRootSelector = "#app";

        /// <summary>Root component selector for head outlet</summary>
        public const string HeadOutletSelector = "head::after";
    }

    /// <summary>
    /// Layout related constants for MainLayout.razor
    /// </summary>
    public static class Layout
    {
        /// <summary>Application icon emoji</summary>
        public const string AppIcon = "🍅";

        /// <summary>Application title displayed in header</summary>
        public const string AppTitle = "Pomodoro";

        /// <summary>Application tagline</summary>
        public const string Tagline = "Focus. Work. Achieve.";

        /// <summary>Timer navigation link icon</summary>
        public const string TimerNavIcon = "⏱️";

        /// <summary>History navigation link icon</summary>
        public const string HistoryNavIcon = "📊";

        /// <summary>Settings navigation link icon</summary>
        public const string SettingsNavIcon = "⚙️";

        /// <summary>About navigation link icon</summary>
        public const string AboutNavIcon = "ℹ️";

        /// <summary>Timer navigation link title</summary>
        public const string TimerNavLinkTitle = "Timer";

        /// <summary>History navigation link title</summary>
        public const string HistoryNavLinkTitle = "History";

        /// <summary>Settings navigation link title</summary>
        public const string SettingsNavLinkTitle = "Settings";

        /// <summary>About navigation link title</summary>
        public const string AboutNavLinkTitle = "About Pomodoro";

        #region History Page Section Titles

        /// <summary>Weekly Trend section title</summary>
        public const string WeeklyTrendTitle = "Weekly Trend";

        /// <summary>Timeline section title</summary>
        public const string TimelineTitle = "Timeline";

        #endregion

        /// <summary>Footer made with text</summary>
        public const string FooterMadeWithText = "Made with ❤️ for productivity enthusiasts";

        /// <summary>Footer copyright text (without year)</summary>
        public const string FooterCopyrightOwner = "BitOps";
    }

    /// <summary>
    /// Page title constants
    /// </summary>
    public static class PageTitles
    {
        /// <summary>About page title</summary>
        public const string AboutPageTitle = "🍅 The Pomodoro Technique";
    }

    /// <summary>
    /// Task-related UI text constants
    /// </summary>
    public static class TaskUI
    {
        /// <summary>Current task label prefix with emoji</summary>
        public const string CurrentTaskLabel = "📌 Current Task:";

        /// <summary>Select task prompt with emoji</summary>
        public const string SelectTaskPrompt = "📌 Select a task to start";
    }

    /// <summary>
    /// Picture-in-Picture timer UI text constants
    /// </summary>
    public static class PipTimerUI
    {
        /// <summary>Title for close floating timer button</summary>
        public const string CloseFloatingTimerTitle = "Close floating timer";

        /// <summary>Title for pop out floating timer button</summary>
        public const string PopOutFloatingTimerTitle = "Pop out floating timer";
    }

    /// <summary>
    /// CSS file path constants
    /// </summary>
    public static class CssPaths
    {
        /// <summary>About page CSS file path</summary>
        public const string AboutCss = "css/about.css";
    }


    /// <summary>
    /// About page content text constants for translation/localization
    /// </summary>
    public static class AboutPageContent
    {
        // Header Section
        public const string Subtitle = "A time management method to boost your productivity";

        // What is it Section
        public const string WhatIsSectionTitle = "📖 What is Pomodoro Technique?";
        public const string WhatIsParagraph = "The Pomodoro Technique is a time management method developed by Francesco Cirillo in late 1980s. " +
            "The technique uses a timer to break work into intervals, traditionally {0} in length, " +
            "separated by short breaks. Each interval is known as a \"pomodoro,\" from the Italian word for tomato, " +
            "after the tomato-shaped kitchen timer Cirillo used as a university student.";

        // How It Works Section
        public const string HowItWorksSectionTitle = "⚙️ How It Works";
        public const string Step1Title = "Choose a Task";
        public const string Step1Description = "Select a task you want to work on. It can be anything from studying, writing, coding, or any project that needs your attention.";
        public const string Step2Title = "Set Timer";
        public const string Step2Description = "Set Pomodoro timer to {0} (or customize it to your preference). This is your focused work period.";
        public const string Step3Title = "Work with Focus";
        public const string Step3Description = "Work on the task until the timer rings. Avoid checking emails, social media, or any distractions during this time.";
        public const string Step4Title = "Take a Short Break";
        public const string Step4Description = "When the timer rings, take a {0} break. Stretch, grab a coffee, or just relax. This helps your brain recharge.";
        public const string Step5Title = "Repeat & Long Break";
        public const string Step5Description = "After {0}, take a longer break of {1}. This helps prevent mental fatigue and maintains productivity.";

        // Benefits Section
        public const string BenefitsSectionTitle = "✨ Benefits";
        public const string Benefit1Title = "Improved Focus";
        public const string Benefit1Description = "Working in short bursts helps maintain concentration and reduces the urge to procrastinate.";
        public const string Benefit2Title = "Increased Productivity";
        public const string Benefit2Description = "The time constraint creates urgency, helping you accomplish more in less time.";
        public const string Benefit3Title = "Better Mental Health";
        public const string Benefit3Description = "Regular breaks prevent burnout and keep your mind fresh throughout the day.";
        public const string Benefit4Title = "Track Progress";
        public const string Benefit4Description = "Counting pomodoros gives you a clear measure of how much time you've invested in tasks.";
        public const string Benefit5Title = "Time Awareness";
        public const string Benefit5Description = "Helps you understand how long tasks actually take, improving your planning skills.";
        public const string Benefit6Title = "Reduced Distractions";
        public const string Benefit6Description = "The timer creates a commitment to focus, making it easier to ignore interruptions.";

        // Tips Section
        public const string TipsSectionTitle = "💡 Tips for Success";
        public const string Tip1Title = "Start Small";
        public const string Tip1Description = "If {0} feels too long, start with {1} intervals and gradually increase.";
        public const string Tip2Title = "Protect Your Pomodoro";
        public const string Tip2Description = "Let others know when you're in a focus session to minimize interruptions.";
        public const string Tip3Title = "Complete Pomodoro";
        public const string Tip3Description = "If you finish a task early, use the remaining time to review or plan.";
        public const string Tip4Title = "Take Real Breaks";
        public const string Tip4Description = "Step away from your screen during breaks. Stretch, walk, or do something unrelated to work.";
        public const string Tip5Title = "Track Your Day";
        public const string Tip5Description = "Use this app's history feature to see your productivity patterns over time.";
        public const string Tip6Title = "Customize Your Times";
        public const string Tip6Description = "Adjust work and break durations to match your personal rhythm.";

        // Default Times Section
        public const string DefaultTimesSectionTitle = "⏱️ Default Timer Settings";
        public const string MinutesPomodoroLabel = "Minutes<br/>Pomodoro";
        public const string MinutesShortBreakLabel = "Minutes<br/>Short Break";
        public const string MinutesLongBreakLabel = "Minutes<br/>Long Break";
        public const string TimesNote = "You can customize these in the Settings page!";

        // Call to Action Section
        public const string CtaSectionTitle = "Ready to Boost Your Productivity?";
        public const string CtaDescription = "Start using the Pomodoro Technique today and experience the difference!";
        public const string CtaButtonText = "🍅 Start Focusing";
    }
}
