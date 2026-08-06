namespace Pomodoro.Web;

public static partial class Constants
{
    public static class TimeFormats
    {
        public const string MinutesFormat = "{0}m";
        public const string HoursFormat = "{0}h";
        public const string HoursMinutesFormat = "{0}h {1}m";
        public const string TimerFormat = "{0:D2}:{1:D2}";

        // Date formats
        public const string TodayText = "Today";
        public const string YesterdayText = "Yesterday";
        public const string DateFormat = "MMM dd, yyyy";
        public const string ShortDateFormat = "MMM dd";
        public const string DayOfWeekShortFormat = "ddd";

        // Relative time
        public const string JustNowText = "Just now";
        public const string MinutesAgoFormat = "{0}m ago";
        public const string HoursAgoFormat = "{0}h ago";
        public const string DaysAgoFormat = "{0}d ago";
    }

    public static class DateFormats
    {
        public const string IsoFormat = "O";
        public const string DayOfWeekShortFormat = "ddd";
        public const string GuidNoDashesFormat = "N";
    }

    public static class DurationFormats
    {
        public const string MinutesFormat = "{0} min";
    }

    public static class Formatting
    {
        /// <summary>Separator between emoji and title text</summary>
        public const string EmojiTitleSeparator = " ";
    }
}
