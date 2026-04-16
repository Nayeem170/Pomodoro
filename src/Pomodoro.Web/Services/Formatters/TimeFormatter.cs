using static Pomodoro.Web.Constants;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for formatting time values into human-readable strings
/// </summary>
public class TimeFormatter
{
    /// <summary>
    /// Formats minutes into a human-readable time string
    /// </summary>
    /// <param name="minutes">Total minutes to format</param>
    /// <returns>Formatted time string (e.g., "2h 30m", "45m", "1h")</returns>
    public virtual string FormatTime(int minutes)
    {
        if (minutes == 0) return string.Format(Constants.TimeFormats.MinutesFormat, 0);

        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;

        if (hours > 0 && mins > 0)
        {
            return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
        }
        else if (hours > 0)
        {
            return string.Format(Constants.TimeFormats.HoursFormat, hours);
        }
        else
        {
            return string.Format(Constants.TimeFormats.MinutesFormat, mins);
        }
    }

    /// <summary>
    /// Formats minutes into a simple time string (hours and minutes only)
    /// </summary>
    /// <param name="minutes">Total minutes to format</param>
    /// <returns>Formatted time string (e.g., "2h 30m", "45m")</returns>
    public string FormatSimpleTime(int minutes)
    {
        if (minutes < Constants.TimeConversion.MinutesPerHour)
            return string.Format(Constants.TimeFormats.MinutesFormat, minutes);

        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;
        return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
    }
}
