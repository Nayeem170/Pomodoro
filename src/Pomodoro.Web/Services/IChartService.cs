namespace Pomodoro.Web.Services;

public interface IChartService : IAsyncDisposable
{
    Task CreateBarChartAsync(string canvasId, string[] labels, int[] data, string label = "", int? highlightIndex = null);
    Task CreateGroupedBarChartAsync(string canvasId, string[] labels, int[] focusData, int[] breakData, int? highlightIndex = null);
    Task UpdateChartAsync(string canvasId, int[] data);
    Task DestroyChartAsync(string canvasId);
}
