using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IImportService
{
    Task<ImportResult> ImportFromJsonAsync(string jsonData);
}
