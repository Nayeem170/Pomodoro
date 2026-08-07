using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public class InfiniteScrollInterop : IInfiniteScrollInterop
{
    private readonly IJSRuntime _jsRuntime;

    public InfiniteScrollInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>(Constants.InfiniteScrollJsFunctions.IsSupported);
        }
        catch (JSException)
        {
            return false;
        }
    }

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
                Constants.InfiniteScrollJsFunctions.CreateObserver,
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

    public async Task DestroyObserverAsync(string sentinelId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.InfiniteScrollJsFunctions.DestroyObserver, sentinelId);
        }
        catch (JSException)
        {
            // Silently handle errors during cleanup
        }
    }

    public async Task DestroyAllObserversAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.InfiniteScrollJsFunctions.DestroyAllObservers);
        }
        catch (JSException)
        {
            // Silently handle errors during cleanup
        }
    }
}
