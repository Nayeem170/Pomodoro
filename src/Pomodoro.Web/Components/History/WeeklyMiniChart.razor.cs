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
    protected ChartService ChartService { get; set; } = default!;
    
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
        if (_isRendered)
        {
            await RenderChartAsync();
        }
    }
    
    private async Task RenderChartAsync()
    {
        var (labels, focusData, breakData) = ChartDataFormatter.PrepareWeeklyChartData(
            DailyFocusMinutes, BreakDailyMinutes, WeekStartDate);
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
