using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for WeeklyMiniChart component
/// Displays a weekly bar chart using Chart.js
/// </summary>
public class WeeklyMiniChartBase : ComponentBase, IAsyncDisposable
{
    #region Services (Dependency Injection)

    [Inject]
    protected IChartService ChartService { get; set; } = default!;

    [Inject]
    protected ChartDataFormatter ChartDataFormatter { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter]
    public Dictionary<DateTime, int> DailyFocusMinutes { get; set; } = new();

    [Parameter]
    public Dictionary<DateTime, int> BreakDailyMinutes { get; set; } = new();

    [Parameter]
    public DateTime WeekStartDate { get; set; }

    #endregion

    #region Properties

    protected string CanvasId { get; } = $"{Constants.Charts.WeeklyChartPrefix}{Guid.NewGuid().ToString(Constants.DateFormats.GuidNoDashesFormat)}";

    #endregion

    private bool _isRendered;
    private Dictionary<DateTime, int>? _previousFocusMinutes;
    private Dictionary<DateTime, int>? _previousBreakMinutes;
    private DateTime _previousWeekStart;

    #region Lifecycle Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isRendered = true;
            await RenderChartAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_isRendered) return;

        if (_previousWeekStart == WeekStartDate &&
            _previousFocusMinutes != null && _previousBreakMinutes != null &&
            DictionariesEqual(_previousFocusMinutes, DailyFocusMinutes) &&
            DictionariesEqual(_previousBreakMinutes, BreakDailyMinutes))
        {
            return;
        }

        await RenderChartAsync();
    }

    private static bool DictionariesEqual<TKey, TValue>(
        Dictionary<TKey, TValue>? a, Dictionary<TKey, TValue>? b)
        where TKey : notnull
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.Count != b.Count) return false;
        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var value) || !EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                return false;
        }
        return true;
    }

    private async Task RenderChartAsync()
    {
        var (labels, focusData, breakData) = ChartDataFormatter.PrepareWeeklyChartData(
            DailyFocusMinutes, BreakDailyMinutes, WeekStartDate);
        _previousFocusMinutes = new Dictionary<DateTime, int>(DailyFocusMinutes);
        _previousBreakMinutes = new Dictionary<DateTime, int>(BreakDailyMinutes);
        _previousWeekStart = WeekStartDate;
        await ChartService.CreateGroupedBarChartAsync(CanvasId, labels, focusData, breakData, null);
    }

    #endregion

    #region IDisposable

    public async ValueTask DisposeAsync()
    {
        try
        {
            await ChartService.DestroyChartAsync(CanvasId);
        }
        catch
        {
            // Ignore disposal errors
        }
    }

    #endregion
}
