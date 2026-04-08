using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for exporting and importing application data
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all data to JSON format
    /// </summary>
    /// <returns>JSON formatted string</returns>
    Task<string> ExportToJsonAsync();
    
    /// <summary>
    /// Imports data from JSON format with duplicate detection
    /// </summary>
    /// <param name="jsonData">JSON string containing exported data</param>
    /// <returns>ImportResult with detailed statistics about the import operation</returns>
    Task<ImportResult> ImportFromJsonAsync(string jsonData);
    
    /// <summary>
    /// Clears all application data
    /// </summary>
    /// <returns>Task indicating completion</returns>
    Task ClearAllDataAsync();
}
