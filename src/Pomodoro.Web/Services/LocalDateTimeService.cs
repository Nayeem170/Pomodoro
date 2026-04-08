using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for getting the client's local date and time
/// </summary>
public class LocalDateTimeService : ILocalDateTimeService
{
    private readonly IJSRuntime _jsRuntime;
    private DateTime? _cachedLocalDate;
    private DateTimeOffset? _cachedLocalDateTimeOffset;

    /// <summary>
    /// Initializes a new instance of the LocalDateTimeService
    /// </summary>
    /// <param name="jsRuntime">The JS runtime instance</param>
    public LocalDateTimeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Gets the client's local date asynchronously
    /// </summary>
    /// <returns>The client's local date</returns>
    public virtual async Task<DateTime> GetLocalDateAsync()
    {
        // Return cached value if available
        if (_cachedLocalDate.HasValue)
        {
            return _cachedLocalDate.Value;
        }

        try
        {
            // Get the client's local date via JavaScript interop
            var localDate = await _jsRuntime.InvokeAsync<DateTime>("localDateTime.getLocalDate");
            _cachedLocalDate = localDate;
            return localDate;
        }
        catch (Exception)
        {
            // Fallback to server date if JavaScript interop fails
            return DateTime.Now.Date;
        }
    }

    /// <summary>
    /// Gets the client's local date and time with timezone offset asynchronously
    /// </summary>
    /// <returns>The client's local date and time with timezone offset</returns>
    public virtual async Task<DateTimeOffset> GetLocalDateTimeOffsetAsync()
    {
        // Return cached value if available
        if (_cachedLocalDateTimeOffset.HasValue)
        {
            return _cachedLocalDateTimeOffset.Value;
        }

        try
        {
            // Get the client's local date and time via JavaScript interop
            var localDateTime = await _jsRuntime.InvokeAsync<DateTime>("localDateTime.getLocalDateTime");
            var offset = await _jsRuntime.InvokeAsync<int>("localDateTime.getTimezoneOffset");
            
            // Create a DateTimeOffset with the local time and timezone offset
            var localDateTimeOffset = new DateTimeOffset(
                localDateTime.Year,
                localDateTime.Month,
                localDateTime.Day,
                localDateTime.Hour,
                localDateTime.Minute,
                localDateTime.Second,
                TimeSpan.FromMinutes(-offset)); // JavaScript returns offset in minutes with opposite sign
            
            _cachedLocalDateTimeOffset = localDateTimeOffset;
            return localDateTimeOffset;
        }
        catch (Exception)
        {
            // Fallback to server date if JavaScript interop fails
            return DateTimeOffset.Now;
        }
    }

    /// <summary>
    /// Clears the cached local date and time
    /// </summary>
    public void ClearCache()
    {
        _cachedLocalDate = null;
        _cachedLocalDateTimeOffset = null;
    }

    /// <summary>
    /// Gets the client's local date and time asynchronously
    /// </summary>
    /// <returns>The client's local date and time</returns>
    public virtual async Task<DateTime> GetLocalDateTimeAsync()
    {
        try
        {
            // Get the client's local date and time via JavaScript interop
            var localDateTime = await _jsRuntime.InvokeAsync<DateTime>("localDateTime.getLocalDateTime");
            return localDateTime;
        }
        catch (Exception)
        {
            // Fallback to server date if JavaScript interop fails
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Gets the client's timezone offset in minutes
    /// </summary>
    /// <returns>The timezone offset in minutes</returns>
    public virtual async Task<int> GetTimezoneOffsetAsync()
    {
        try
        {
            // Get the client's timezone offset via JavaScript interop
            var offset = await _jsRuntime.InvokeAsync<int>("localDateTime.getTimezoneOffset");
            return offset;
        }
        catch (Exception)
        {
            // Fallback to server timezone offset if JavaScript interop fails
            return (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
        }
    }
}