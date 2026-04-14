using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for managing keyboard shortcuts in the application
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<KeyboardShortcutService> _logger;
    private readonly Dictionary<string, Action> _shortcuts = new();
    private readonly Dictionary<string, string> _descriptions = new();
    private DotNetObjectReference<KeyboardShortcutService>? _dotNetRef;

    public KeyboardShortcutService(IJSRuntime jsRuntime, ILogger<KeyboardShortcutService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public void RegisterShortcut(string key, Action action, string? description = null)
    {
        var normalizedKey = key.ToLowerInvariant();
        _shortcuts[normalizedKey] = action;
        if (description != null)
        {
            _descriptions[normalizedKey] = description;
        }
        _logger.LogDebug("Registered keyboard shortcut: {Key}", normalizedKey);
    }

    public void UnregisterShortcut(string key)
    {
        var normalizedKey = key.ToLowerInvariant();
        _shortcuts.Remove(normalizedKey);
        _descriptions.Remove(normalizedKey);
        _logger.LogDebug("Unregistered keyboard shortcut: {Key}", normalizedKey);
    }

    public Dictionary<string, string> GetRegisteredShortcuts()
    {
        return new Dictionary<string, string>(_descriptions);
    }

    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync(Constants.KeyboardShortcutJsFunctions.Initialize, _dotNetRef);
        _logger.LogInformation("Keyboard shortcut service initialized");
    }

    /// <summary>
    /// Called from JavaScript when a keyboard shortcut is triggered
    /// </summary>
    [JSInvokable]
    public void HandleShortcut(string key)
    {
        var normalizedKey = key.ToLowerInvariant();
        if (_shortcuts.TryGetValue(normalizedKey, out var action))
        {
            _logger.LogDebug("Keyboard shortcut triggered: {Key}", normalizedKey);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing keyboard shortcut action for key: {Key}", normalizedKey);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(Constants.KeyboardShortcutJsFunctions.Dispose);
                _dotNetRef.Dispose();
                _dotNetRef = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing keyboard shortcut service");
            }
        }
    }
}
