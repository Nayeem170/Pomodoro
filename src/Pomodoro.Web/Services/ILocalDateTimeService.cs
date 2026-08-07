using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public interface ILocalDateTimeService
{
    /// <returns>The client's local date</returns>
    Task<DateTime> GetLocalDateAsync();

    /// <returns>The client's local date and time with timezone offset</returns>
    Task<DateTimeOffset> GetLocalDateTimeOffsetAsync();

    /// <returns>The client's local date and time</returns>
    Task<DateTime> GetLocalDateTimeAsync();

    /// <returns>The timezone offset in minutes</returns>
    Task<int> GetTimezoneOffsetAsync();

    void ClearCache();
}
