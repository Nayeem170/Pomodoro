using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public interface IInfiniteScrollInterop
{
    /// <returns>True if Intersection Observer is supported, false otherwise.</returns>
    Task<bool> IsSupportedAsync();

    /// <param name="sentinelId">ID of the sentinel element to observe.</param>
    /// <param name="dotNetRef">DotNet object reference for JS callbacks.</param>
    /// <param name="containerId">ID of the scroll container element.</param>
    /// <param name="rootMargin">Root margin for the Intersection Observer.</param>
    /// <param name="timeoutMs">Timeout in milliseconds for observer operations.</param>
    /// <returns>True if observer was created successfully, false otherwise.</returns>
    Task<bool> CreateObserverAsync(
        string sentinelId,
        DotNetObjectReference<object> dotNetRef,
        string containerId,
        string rootMargin,
        int timeoutMs);

    /// <param name="sentinelId">ID of the sentinel element to destroy observer for.</param>
    Task DestroyObserverAsync(string sentinelId);

    Task DestroyAllObserversAsync();
}
