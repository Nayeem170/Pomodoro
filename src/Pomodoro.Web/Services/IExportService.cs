namespace Pomodoro.Web.Services;

public interface IExportService
{
    Task<string> ExportToJsonAsync();
    Task ClearAllDataAsync();
}
