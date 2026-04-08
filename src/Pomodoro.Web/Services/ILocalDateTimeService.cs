using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for the LocalDateTimeService
/// </summary>
public interface ILocalDateTimeService
{
    /// <summary>
    /// Gets the client's local date asynchronously
    /// </summary>
    /// <returns>The client's local date</returns>
    Task<DateTime> GetLocalDateAsync();

    /// <summary>
    /// Gets the client's local date and time with timezone offset asynchronously
    /// </summary>
    /// <returns>The client's local date and time with timezone offset</returns>
    Task<DateTimeOffset> GetLocalDateTimeOffsetAsync();

    /// <summary>
    /// Gets the client's local date and time asynchronously
    /// </summary>
    /// <returns>The client's local date and time</returns>
    Task<DateTime> GetLocalDateTimeAsync();

    /// <summary>
    /// Gets the client's timezone offset in minutes
    /// </summary>
    /// <returns>The timezone offset in minutes</returns>
    Task<int> GetTimezoneOffsetAsync();

    /// <summary>
    /// Clears the cached local date and time
    /// </summary>
    void ClearCache();
}