using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for JavaScript interop operations to enable mocking
/// </summary>
public interface IJSInteropService
{
    /// <summary>
    /// Invokes a JavaScript function that doesn't return a value
    /// </summary>
    /// <param name="identifier">The identifier of the function to invoke</param>
    /// <param name="args">Arguments to pass to the function</param>
    Task InvokeVoidAsync(string identifier, params object[] args);

    /// <summary>
    /// Invokes a JavaScript function that returns a value
    /// </summary>
    /// <typeparam name="TValue">The return type</typeparam>
    /// <param name="identifier">The identifier of the function to invoke</param>
    /// <param name="args">Arguments to pass to the function</param>
    Task<TValue> InvokeAsync<TValue>(string identifier, params object[] args);
}

/// <summary>
/// Default implementation of IJSInteropService that delegates to IJSRuntime
/// </summary>
public class JSInteropService : IJSInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task InvokeVoidAsync(string identifier, params object[] args)
    {
        return _jsRuntime.InvokeVoidAsync(identifier, args).AsTask();
    }

    public Task<TValue> InvokeAsync<TValue>(string identifier, params object[] args)
    {
        return _jsRuntime.InvokeAsync<TValue>(identifier, args).AsTask();
    }
}