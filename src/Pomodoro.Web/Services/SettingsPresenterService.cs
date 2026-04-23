using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for settings page presentation logic
/// </summary>
public class SettingsPresenterService
{
    private readonly ILogger<SettingsPresenterService> _logger;

    public SettingsPresenterService(ILogger<SettingsPresenterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks if given settings match default values
    /// </summary>
    /// <param name="settings">Settings to check</param>
    /// <returns>True if settings match defaults, false otherwise</returns>
    public virtual bool IsAtDefaults(TimerSettings settings)
    {
        var defaults = new TimerSettings();
        return settings.PomodoroMinutes == defaults.PomodoroMinutes
            && settings.ShortBreakMinutes == defaults.ShortBreakMinutes
            && settings.LongBreakMinutes == defaults.LongBreakMinutes
            && settings.SoundEnabled == defaults.SoundEnabled
            && settings.NotificationsEnabled == defaults.NotificationsEnabled
            && settings.AutoStartPomodoros == defaults.AutoStartPomodoros
            && settings.AutoStartBreaks == defaults.AutoStartBreaks
            && settings.AutoStartDelaySeconds == defaults.AutoStartDelaySeconds
            && settings.LongBreakInterval == defaults.LongBreakInterval
            && settings.DailyGoal == defaults.DailyGoal;
    }

    /// <summary>
    /// Builds a success message for import operation
    /// </summary>
    /// <param name="totalImported">Total number of records imported</param>
    /// <param name="totalSkipped">Total number of records skipped</param>
    /// <returns>Formatted success message</returns>
    public virtual string BuildImportSuccessMessage(int totalImported, int totalSkipped)
    {
        var messageParts = new List<string>();

        if (totalImported > 0)
        {
            messageParts.Add($"imported {totalImported} records");
        }

        if (totalSkipped > 0)
        {
            messageParts.Add($"skipped {totalSkipped} duplicates");
        }

        return messageParts.Count > 0
            ? $"Import complete: {string.Join(", ", messageParts)}."
            : "Import complete: no new records to import.";
    }

    /// <summary>
    /// Downloads file using JavaScript interop
    /// </summary>
    /// <param name="jsInteropService">JS interop service instance</param>
    /// <param name="filename">Filename to download</param>
    /// <param name="content">File content</param>
    /// <param name="mimeType">MIME type</param>
    public virtual async Task DownloadFileAsync(IJSInteropService jsInteropService, string filename, string content, string mimeType)
    {
        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var base64 = Convert.ToBase64String(bytes);

            await jsInteropService.InvokeVoidAsync("fileInterop.downloadFile", filename, base64, mimeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {Filename}", filename);
            throw;
        }
    }

}
