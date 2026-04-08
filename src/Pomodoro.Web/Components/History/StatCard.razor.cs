using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for StatCard component
/// Displays a single statistic with icon, value and label
/// </summary>
public class StatCardBase : ComponentBase
{
    [Inject]
    private StatCardFormatter Formatter { get; set; } = null!;

    #region Parameters

    [Parameter]
    public string Icon { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    #endregion

    #region Methods for Testing

    /// <summary>
    /// Gets the formatted display value
    /// </summary>
    public string GetFormattedValue()
    {
        return Formatter.GetFormattedValue(Value);
    }

    /// <summary>
    /// Gets the formatted display label
    /// </summary>
    public string GetFormattedLabel()
    {
        return Formatter.GetFormattedLabel(Label);
    }

    /// <summary>
    /// Gets the formatted display icon
    /// </summary>
    public string GetFormattedIcon()
    {
        return Formatter.GetFormattedIcon(Icon);
    }

    /// <summary>
    /// Checks if the card has all required data
    /// </summary>
    public bool HasRequiredData()
    {
        return Formatter.HasRequiredData(Icon, Value, Label);
    }

    #endregion
}
