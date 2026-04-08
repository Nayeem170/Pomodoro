using Pomodoro.Web.Components.History;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for formatting StatCard component data.
/// Extracts formatting logic from component to enable testable code with coverage tracking.
/// </summary>
public class StatCardFormatter
{
    /// <summary>
    /// Gets the formatted value for display.
    /// Returns "0" if value is empty or null.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>Formatted value string</returns>
    public string GetFormattedValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "0" : value;
    }

    /// <summary>
    /// Gets the formatted label for display.
    /// Returns "N/A" if label is empty or null.
    /// </summary>
    /// <param name="label">The label to format</param>
    /// <returns>Formatted label string</returns>
    public string GetFormattedLabel(string? label)
    {
        return string.IsNullOrWhiteSpace(label) ? "N/A" : label;
    }

    /// <summary>
    /// Gets the formatted icon for display.
    /// Returns default icon "📊" if icon is empty or null.
    /// </summary>
    /// <param name="icon">The icon to format</param>
    /// <returns>Formatted icon string</returns>
    public string GetFormattedIcon(string? icon)
    {
        return string.IsNullOrWhiteSpace(icon) ? "📊" : icon;
    }

    /// <summary>
    /// Checks if the stat card has all required data.
    /// </summary>
    /// <param name="icon">The icon value</param>
    /// <param name="value">The value</param>
    /// <param name="label">The label</param>
    /// <returns>True if all required data is present</returns>
    public bool HasRequiredData(string? icon, string? value, string? label)
    {
        return !string.IsNullOrWhiteSpace(icon) && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(label);
    }
}
