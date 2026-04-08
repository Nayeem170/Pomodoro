using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Displays a doughnut chart showing time distribution across tasks and breaks
/// </summary>
public partial class TimeDistributionChart : IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IActivityService ActivityService { get; set; } = default!;
    [Inject] private ILogger<TimeDistributionChart> Logger { get; set; } = default!;
    [Inject] private TimeFormatter TimeFormatter { get; set; } = default!;
    
    [Parameter]
    public DateTime SelectedDate { get; set; }
    
    /// <summary>
    /// Static canvas ID for the chart (single instance)
    /// </summary>
    private static readonly string CanvasId = Constants.Charts.TimeDistributionCanvasId;
    
    private DateTime _lastRenderedDate;

    /// <summary>
    /// Total minutes displayed in the chart
    /// </summary>
    public int TotalMinutes { get; private set; }

    /// <summary>
    /// Formatted total time for display (safe from null reference)
    /// </summary>
    public string FormattedTotalMinutes
    {
        get
        {
            try
            {
                return TimeFormatter?.FormatTime(TotalMinutes) ?? TotalMinutes.ToString();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error formatting total minutes in TimeDistributionChart");
                return TotalMinutes.ToString();
            }
        }
    }

    /// <summary>
    /// Whether there is data to display
    /// </summary>
    public bool HasData { get; private set; }
    
    private bool _isRendered;
    private bool _isDisposed;
    
    protected override void OnInitialized()
    {
        ActivityService.OnActivityChanged += OnActivityChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isRendered = true;
            await UpdateChartAsync();
        }
    }
    
    protected override async Task OnParametersSetAsync()
    {
        // Only update if the date has changed and we've already rendered
        if (_isRendered && _lastRenderedDate != SelectedDate.Date)
        {
            await UpdateChartAsync();
        }
    }
    
    private void OnActivityChanged()
    {
        if (_isRendered && !_isDisposed)
        {
            SafeTaskRunner.RunAndForget(async () =>
            {
                await InvokeAsync(async () =>
                {
                    await UpdateChartAsync();
                    StateHasChanged();
                });
            }, Logger, "OnActivityChanged");
        }
    }
    
    private async Task UpdateChartAsync()
    {
        if (_isDisposed) return;
        
        try
        {
            var distribution = ActivityService.GetTimeDistribution(SelectedDate);
            _lastRenderedDate = SelectedDate.Date;
            
            if (distribution.Count == 0)
            {
                TotalMinutes = 0;
                    HasData = false;
                    // Destroy existing chart when no data
                    await JS.InvokeVoidAsync(Constants.ChartJsFunctions.DestroyChart, CanvasId);
                    StateHasChanged();
                    return;
                }
                
                var labels = distribution.Keys.ToList();
                var data = distribution.Values.ToList();
                TotalMinutes = data.Sum();
                HasData = true;
                
                var centerText = TimeFormatter.FormatTime(TotalMinutes);
                
                await JS.InvokeVoidAsync(Constants.ChartJsFunctions.CreateDoughnutChart,
                    CanvasId,
                    labels,
                    data,
                    centerText);
                
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Constants.Messages.ErrorUpdatingTimeDistributionChart);
            }
        }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        ActivityService.OnActivityChanged -= OnActivityChanged;
        
        SafeTaskRunner.RunAndForget(
            async () => { await JS.InvokeVoidAsync(Constants.ChartJsFunctions.DestroyChart, CanvasId); },
            Logger,
            "DestroyChart");
    }
}
