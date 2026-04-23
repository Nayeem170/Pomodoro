namespace Pomodoro.Web.Services;

public interface IExportService
{
    Task<string> ExportToJsonAsync();
    Task<string> ExportToJsonStringAsync();
    Task ClearAllDataAsync();
}
