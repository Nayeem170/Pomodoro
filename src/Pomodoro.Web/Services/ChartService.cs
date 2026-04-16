using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for Chart.js JavaScript interop
/// </summary>
public class ChartService : IChartService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _initialized;

    public ChartService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Creates a bar chart with the specified configuration
    /// </summary>
    public async Task CreateBarChartAsync(string canvasId, string[] labels, int[] data, string label = Constants.Charts.PomodorosLabel, int? highlightIndex = null)
    {
        await EnsureInitializedAsync();
        await _jsRuntime.InvokeVoidAsync(Constants.ChartJsFunctions.CreateBarChart, canvasId, labels, data, label, highlightIndex);
    }

    /// <summary>
    /// Creates a grouped bar chart with focus time and break data
    /// </summary>
    public async Task CreateGroupedBarChartAsync(string canvasId, string[] labels, int[] focusData, int[] breakData, int? highlightIndex = null)
    {
        await EnsureInitializedAsync();
        await _jsRuntime.InvokeVoidAsync(Constants.ChartJsFunctions.CreateGroupedBarChart, canvasId, labels, focusData, breakData, highlightIndex);
    }

    /// <summary>
    /// Updates an existing chart with new data
    /// </summary>
    public async Task UpdateChartAsync(string canvasId, int[] data)
    {
        await _jsRuntime.InvokeVoidAsync(Constants.ChartJsFunctions.UpdateChart, canvasId, data);
    }

    /// <summary>
    /// Destroys a chart to free resources
    /// </summary>
    public virtual async Task DestroyChartAsync(string canvasId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.ChartJsFunctions.DestroyChart, canvasId);
        }
        catch (Exception)
        {
            // Ignore errors during disposal - chart may not exist or already destroyed
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.ChartJsFunctions.EnsureInitialized);
            _initialized = true;
        }
        catch
        {
            // Chart.js may not be loaded yet, will be handled gracefully
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Charts will be cleaned up by the browser when the page unloads
        await Task.CompletedTask;
    }
}
