namespace Pomodoro.Web;

/// <summary>
/// Timer-related constants for duration settings, time calculations, and threading
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Timer duration and interval constants
    /// </summary>
    public static class Timer
    {
        /// <summary>Default pomodoro duration in minutes</summary>
        public const int DefaultPomodoroMinutes = 25;
        
        /// <summary>Default short break duration in minutes</summary>
        public const int DefaultShortBreakMinutes = 5;
        
        /// <summary>Default long break duration in minutes</summary>
        public const int DefaultLongBreakMinutes = 15;
        
        /// <summary>Timer interval in milliseconds</summary>
        public const int TimerIntervalMs = 1000;
        
        /// <summary>Minimum pomodoro duration in minutes</summary>
        public const int MinPomodoroMinutes = 1;
        
        /// <summary>Maximum pomodoro duration in minutes</summary>
        public const int MaxPomodoroMinutes = 120;
        
        /// <summary>Minimum break duration in minutes</summary>
        public const int MinBreakMinutes = 1;
        
        /// <summary>Maximum break duration in minutes</summary>
        public const int MaxBreakMinutes = 60;
        
        /// <summary>Default auto-start delay in seconds</summary>
        public const int DefaultAutoStartDelaySeconds = 10;
        
        /// <summary>Minimum auto-start delay in seconds</summary>
        public const int MinAutoStartDelaySeconds = 3;
        
        /// <summary>Maximum auto-start delay in seconds</summary>
        public const int MaxAutoStartDelaySeconds = 60;
    }
    
    /// <summary>
    /// Timer-related display text constants
    /// </summary>
    public static class TimerDisplay
    {
        /// <summary>Default pomodoro duration text for display</summary>
        public const string DefaultPomodoroMinutesText = "25 minutes";
        
        /// <summary>Default short break duration text for display</summary>
        public const string DefaultShortBreakMinutesText = "5-minute";
        
        /// <summary>Default long break duration range text for display</summary>
        public const string DefaultLongBreakMinutesRangeText = "15-30 minutes";
        
        /// <summary>Number of pomodoros before long break</summary>
        public const string PomodorosBeforeLongBreak = "4 pomodoros";
        
        /// <summary>Alternative short pomodoro duration for beginners</summary>
        public const string ShortPomodoroMinutesText = "15-minute";
    }
    
    /// <summary>
    /// Time conversion constants for duration calculations
    /// </summary>
    public static class TimeConversion
    {
        /// <summary>Seconds per minute</summary>
        public const int SecondsPerMinute = 60;
        
        /// <summary>Minutes per hour</summary>
        public const int MinutesPerHour = 60;
        
        /// <summary>Days in a week</summary>
        public const int DaysInWeek = 7;
        
        /// <summary>Days in a week for weekly chart lookback (6 days back + today = 7 days)</summary>
        public const int WeeklyLookbackDays = 6;
        
        /// <summary>Day offset for navigating to previous day</summary>
        public const int DayOffsetPrevious = -1;
        
        /// <summary>Day offset for navigating to next day</summary>
        public const int DayOffsetNext = 1;
        
        /// <summary>Day offset to convert from Sunday-based (Sun=0) to Saturday-based (Sat=0) week</summary>
        public const int SaturdayBasedWeekOffset = 1;
    }
    
    /// <summary>
    /// Time threshold constants for relative time calculations
    /// </summary>
    public static class TimeThresholds
    {
        /// <summary>One minute threshold</summary>
        public const int OneMinute = 1;
        
        /// <summary>One hour in minutes</summary>
        public const int OneHourInMinutes = 60;
        
        /// <summary>One day in hours</summary>
        public const int OneDayInHours = 24;
        
        /// <summary>One week in days</summary>
        public const int OneWeekInDays = 7;
    }
    
    /// <summary>
    /// Threading and concurrency constants
    /// </summary>
    public static class Threading
    {
        /// <summary>Initial count for semaphore</summary>
        public const int SemaphoreInitialCount = 1;
        
        /// <summary>Maximum count for semaphore</summary>
        public const int SemaphoreMaxCount = 1;
        
        /// <summary>Wait timeout in milliseconds (no wait)</summary>
        public const int WaitTimeoutMs = 0;
    }
}
