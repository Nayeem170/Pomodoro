namespace Pomodoro.Web;

/// <summary>
/// Navigation, routing, and keyboard shortcut constants
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Keyboard key constants
    /// </summary>
    public static class Keys
    {
        public const string Enter = "Enter";
        public const string Escape = "Escape";
        public const string ArrowLeft = "ArrowLeft";
        public const string ArrowRight = "ArrowRight";
    }
    
    /// <summary>
    /// Routing and navigation related constants
    /// </summary>
    public static class Routing
    {
        /// <summary>Page title for 404 Not Found page</summary>
        public const string NotFoundPageTitle = "Not found";
        
        /// <summary>Message displayed when page is not found</summary>
        public const string NotFoundMessage = "Sorry, there's nothing at this address.";
        
        /// <summary>Home page route</summary>
        public const string HomeRoute = "/";
        
        /// <summary>History page route</summary>
        public const string HistoryRoute = "/history";
        
        /// <summary>History page title with icon</summary>
        public const string HistoryPageTitle = "📊 History";
        
        /// <summary>Settings page route</summary>
        public const string SettingsRoute = "/settings";
        
        /// <summary>About page route</summary>
        public const string AboutRoute = "/about";
    }
    
    /// <summary>
    /// Keyboard shortcuts related constants
    /// </summary>
    public static class KeyboardShortcuts
    {
        /// <summary>Key for starting/pausing timer</summary>
        public const string PlayPauseKey = "Space";
        
        /// <summary>Key for resetting timer</summary>
        public const string ResetKey = "R";
        
        /// <summary>Key for switching to Pomodoro session</summary>
        public const string PomodoroKey = "P";
        
        /// <summary>Key for switching to Short Break session</summary>
        public const string ShortBreakKey = "S";
        
        /// <summary>Key for switching to Long Break session</summary>
        public const string LongBreakKey = "L";
        
        /// <summary>Key for showing keyboard shortcuts help</summary>
        public const string HelpKey = "?";
        
        /// <summary>Description for play/pause shortcut</summary>
        public const string PlayPauseDescription = "Start/Pause timer";
        
        /// <summary>Description for reset shortcut</summary>
        public const string ResetDescription = "Reset timer";
        
        /// <summary>Description for Pomodoro shortcut</summary>
        public const string PomodoroDescription = "Switch to Pomodoro";
        
        /// <summary>Description for Short Break shortcut</summary>
        public const string ShortBreakDescription = "Switch to Short Break";
        
        /// <summary>Description for Long Break shortcut</summary>
        public const string LongBreakDescription = "Switch to Long Break";
        
        /// <summary>Description for help shortcut</summary>
        public const string HelpDescription = "Show keyboard shortcuts";
    }
    
    /// <summary>
    /// Keyboard help modal UI text constants
    /// </summary>
    public static class KeyboardHelp
    {
        /// <summary>Modal title with icon</summary>
        public const string Title = "⌨️ Keyboard Shortcuts";
        
        /// <summary>Timer controls section title</summary>
        public const string TimerSection = "Timer Controls";
        
        /// <summary>Session switching section title</summary>
        public const string SessionSection = "Session Switching";
        
        /// <summary>Other shortcuts section title</summary>
        public const string OtherSection = "Other";
        
        /// <summary>Close button tooltip</summary>
        public const string CloseTooltip = "Close";
    }
}
