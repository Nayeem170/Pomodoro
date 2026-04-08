namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for managing keyboard shortcuts
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Registers a keyboard shortcut with an associated action
    /// </summary>
    /// <param name="key">The keyboard key (e.g., "Space", "r", "n")</param>
    /// <param name="action">The action to execute when the shortcut is triggered</param>
    /// <param name="description">Optional description of what the shortcut does</param>
    void RegisterShortcut(string key, Action action, string? description = null);
    
    /// <summary>
    /// Unregisters a previously registered keyboard shortcut
    /// </summary>
    /// <param name="key">The keyboard key to unregister</param>
    void UnregisterShortcut(string key);
    
    /// <summary>
    /// Gets a dictionary of registered shortcuts with their descriptions
    /// </summary>
    /// <returns>Dictionary mapping keys to descriptions</returns>
    Dictionary<string, string> GetRegisteredShortcuts();
    
    /// <summary>
    /// Initializes the keyboard shortcut service and sets up event listeners
    /// </summary>
    Task InitializeAsync();
}
