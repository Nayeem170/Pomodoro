using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Implementation of infinite scroll JavaScript interop operations.
/// Wraps IJSRuntime calls to enable mocking in unit tests.
/// </summary>
public class InfiniteScrollInterop : IInfiniteScrollInterop
{
    private readonly IJSRuntime _jsRuntime;

    public InfiniteScrollInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Checks if the Intersection Observer API is supported in the browser.
    /// </summary>
    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("infiniteScroll.isSupported");
        }
        catch (JSException)
        {
            return false;
        }
    }

    /// <summary>
    /// Creates an Intersection Observer for the infinite scroll sentinel element.
    /// </summary>
    public async Task<bool> CreateObserverAsync(
        string sentinelId,
        DotNetObjectReference<object> dotNetRef,
        string containerId,
        string rootMargin,
        int timeoutMs)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>(
                "infiniteScroll.createObserver",
                sentinelId,
                dotNetRef,
                containerId,
                rootMargin,
                timeoutMs);
        }
        catch (JSException)
        {
            return false;
        }
    }

    /// <summary>
    /// Destroys the Intersection Observer for a specific sentinel element.
    /// </summary>
    public async Task DestroyObserverAsync(string sentinelId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("infiniteScroll.destroyObserver", sentinelId);
        }
        catch (JSException)
        {
            // Silently handle errors during cleanup
        }
    }

    /// <summary>
    /// Destroys all Intersection Observers (fallback cleanup).
    /// </summary>
    public async Task DestroyAllObserversAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("infiniteScroll.destroyAllObservers");
        }
        catch (JSException)
        {
            // Silently handle errors during cleanup
        }
    }
}
