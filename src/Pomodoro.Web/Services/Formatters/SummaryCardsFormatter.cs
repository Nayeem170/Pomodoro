namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Formatter service for SummaryCards component
/// </summary>
public class SummaryCardsFormatter
{
    private readonly TimeFormatter _timeFormatter;

    /// <summary>
    /// Initializes a new instance of the SummaryCardsFormatter class
    /// </summary>
    /// <param name="timeFormatter">Time formatter service</param>
    public SummaryCardsFormatter(TimeFormatter timeFormatter)
    {
        _timeFormatter = timeFormatter;
    }

    /// <summary>
    /// Formats time in a simple format (e.g., "1h 30m")
    /// </summary>
    /// <param name="minutes">Minutes to format</param>
    /// <returns>Formatted time string</returns>
    public string FormatTime(int minutes)
    {
        return _timeFormatter.FormatSimpleTime(minutes);
    }

    /// <summary>
    /// Formats a count value (e.g., "5 sessions")
    /// </summary>
    /// <param name="count">Count to format</param>
    /// <returns>Formatted count string</returns>
    public string FormatCount(int count)
    {
        return $"{count} sessions";
    }
}
