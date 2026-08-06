namespace Pomodoro.Web.Services;

public interface IKeyboardShortcutService
{
    /// <param name="key">The keyboard key (e.g., "Space", "r", "n")</param>
    /// <param name="action">The action to execute when the shortcut is triggered</param>
    /// <param name="description">Optional description of what the shortcut does</param>
    void RegisterShortcut(string key, Action action, string? description = null);

    /// <param name="key">The keyboard key to unregister</param>
    void UnregisterShortcut(string key);

    /// <returns>Dictionary mapping keys to descriptions</returns>
    Dictionary<string, string> GetRegisteredShortcuts();

    Task InitializeAsync();
}
