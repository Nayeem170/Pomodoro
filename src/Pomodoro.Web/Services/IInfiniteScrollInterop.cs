using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for infinite scroll JavaScript interop operations.
/// This interface enables mocking of JS interop calls in unit tests.
/// </summary>
public interface IInfiniteScrollInterop
{
    /// <summary>
    /// Checks if the Intersection Observer API is supported in the browser.
    /// </summary>
    /// <returns>True if Intersection Observer is supported, false otherwise.</returns>
    Task<bool> IsSupportedAsync();
    
    /// <summary>
    /// Creates an Intersection Observer for the infinite scroll sentinel element.
    /// </summary>
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
    
    /// <summary>
    /// Destroys the Intersection Observer for a specific sentinel element.
    /// </summary>
    /// <param name="sentinelId">ID of the sentinel element to destroy observer for.</param>
    Task DestroyObserverAsync(string sentinelId);
    
    /// <summary>
    /// Destroys all Intersection Observers (fallback cleanup).
    /// </summary>
    Task DestroyAllObserversAsync();
}
