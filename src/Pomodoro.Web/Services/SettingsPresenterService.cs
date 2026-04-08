using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
            && settings.AutoStartEnabled == defaults.AutoStartEnabled
            && settings.AutoStartDelaySeconds == defaults.AutoStartDelaySeconds;
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
    /// Validates settings values
    /// </summary>
    /// <param name="settings">Settings to validate</param>
    /// <returns>Validation result with any errors</returns>
    public SettingsValidationResult ValidateSettings(TimerSettings settings)
    {
        var errors = new List<string>();

        ValidatePomodoroDuration(settings.PomodoroMinutes, errors);
        ValidateShortBreakDuration(settings.ShortBreakMinutes, errors);
        ValidateLongBreakDuration(settings.LongBreakMinutes, errors);
        ValidateAutoStartDelay(settings.AutoStartDelaySeconds, errors);

        return new SettingsValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// Validates pomodoro duration
    /// </summary>
    /// <param name="pomodoroMinutes">Pomodoro duration in minutes</param>
    /// <param name="errors">Error list to add to</param>
    private void ValidatePomodoroDuration(int pomodoroMinutes, List<string> errors)
    {
        if (pomodoroMinutes < Constants.Timer.MinPomodoroMinutes || pomodoroMinutes > Constants.Timer.MaxPomodoroMinutes)
        {
            errors.Add($"Pomodoro duration must be between {Constants.Timer.MinPomodoroMinutes} and {Constants.Timer.MaxPomodoroMinutes} minutes");
        }
    }

    /// <summary>
    /// Validates short break duration
    /// </summary>
    /// <param name="shortBreakMinutes">Short break duration in minutes</param>
    /// <param name="errors">Error list to add to</param>
    private void ValidateShortBreakDuration(int shortBreakMinutes, List<string> errors)
    {
        if (shortBreakMinutes < Constants.Timer.MinBreakMinutes || shortBreakMinutes > Constants.Timer.MaxBreakMinutes)
        {
            errors.Add($"Short break duration must be between {Constants.Timer.MinBreakMinutes} and {Constants.Timer.MaxBreakMinutes} minutes");
        }
    }

    /// <summary>
    /// Validates long break duration
    /// </summary>
    /// <param name="longBreakMinutes">Long break duration in minutes</param>
    /// <param name="errors">Error list to add to</param>
    private void ValidateLongBreakDuration(int longBreakMinutes, List<string> errors)
    {
        if (longBreakMinutes < 1 || longBreakMinutes > 60)
        {
            errors.Add("Long break duration must be between 1 and 60 minutes");
        }
    }

    /// <summary>
    /// Validates auto-start delay
    /// </summary>
    /// <param name="autoStartDelaySeconds">Auto-start delay in seconds</param>
    /// <param name="errors">Error list to add to</param>
    private void ValidateAutoStartDelay(int autoStartDelaySeconds, List<string> errors)
    {
        if (autoStartDelaySeconds < Constants.Timer.MinAutoStartDelaySeconds || autoStartDelaySeconds > Constants.Timer.MaxAutoStartDelaySeconds)
        {
            errors.Add($"Auto-start delay must be between {Constants.Timer.MinAutoStartDelaySeconds} and {Constants.Timer.MaxAutoStartDelaySeconds} seconds");
        }
    }

    /// <summary>
    /// Determines if settings have changes
    /// </summary>
    /// <param name="current">Current settings</param>
    /// <param name="original">Original settings</param>
    /// <returns>True if settings have changes</returns>
    public bool HasChanges(TimerSettings current, TimerSettings original)
    {
        return !current.Equals(original);
    }

    /// <summary>
    /// Creates a toast message for settings save
    /// </summary>
    /// <param name="success">Whether save was successful</param>
    /// <param name="customMessage">Optional custom message</param>
    /// <returns>Toast message</returns>
    public string CreateToastMessage(bool success, string? customMessage = null)
    {
        if (!string.IsNullOrEmpty(customMessage))
        {
            return customMessage!;
        }

        return success ? "Settings saved successfully!" : "Failed to save settings";
    }

    /// <summary>
    /// Determines if auto-start delay should be shown
    /// </summary>
    /// <param name="autoStartEnabled">Whether auto-start is enabled</param>
    /// <returns>True if auto-start delay should be shown</returns>
    public bool ShouldShowAutoStartDelay(bool autoStartEnabled)
    {
        return autoStartEnabled;
    }

    /// <summary>
    /// Validates import file size
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <returns>Validation result</returns>
    public FileValidationResult ValidateImportFileSize(long fileSize)
    {
        if (fileSize == 0)
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = "File is empty"
            };
        }

        if (fileSize > Constants.Validation.MaxImportFileSizeBytes)
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = $"File too large. Maximum size is {Constants.Validation.MaxImportFileSizeBytes / (1024 * 1024)} MB."
            };
        }

        return new FileValidationResult
        {
            IsValid = true,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Creates export filename with current date
    /// </summary>
    /// <returns>Export filename</returns>
    public string CreateExportFilename()
    {
        return $"pomodoro-backup-{DateTime.Today:yyyy-MM-dd}.json";
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

    /// <summary>
    /// Determines if clear data confirmation should be shown
    /// </summary>
    /// <param name="isClearing">Current clearing state</param>
    /// <returns>True if confirmation should be shown</returns>
    public bool ShouldShowClearConfirmation(bool isClearing)
    {
        return !isClearing;
    }

    /// <summary>
    /// Creates toast hide action with delay
    /// </summary>
    /// <param name="showToast">Action to show toast</param>
    /// <param name="hideToast">Action to hide toast</param>
    /// <param name="message">Toast message</param>
    /// <returns>Action that hides toast after delay</returns>
    public Action CreateDelayedToastHideAction(Action<bool> showToast, Action hideToast, string message)
    {
        return () =>
        {
            showToast(true);
            SafeTaskRunner.RunAndForget(async () =>
            {
                await Task.Delay(Constants.UI.ToastDurationMs);
                hideToast();
            }, _logger, Constants.SafeTaskOperations.ToastHide);
        };
    }

    /// <summary>
    /// Determines if import button should be disabled
    /// </summary>
    /// <param name="isImporting">Current importing state</param>
    /// <returns>True if button should be disabled</returns>
    public bool ShouldDisableImportButton(bool isImporting)
    {
        return isImporting;
    }

    /// <summary>
    /// Determines if export button should be disabled
    /// </summary>
    /// <param name="isExporting">Current exporting state</param>
    /// <returns>True if button should be disabled</returns>
    public bool ShouldDisableExportButton(bool isExporting)
    {
        return isExporting;
    }

    /// <summary>
    /// Determines if clear button should be disabled
    /// </summary>
    /// <param name="isClearing">Current clearing state</param>
    /// <returns>True if button should be disabled</returns>
    public bool ShouldDisableClearButton(bool isClearing)
    {
        return isClearing;
    }

    /// <summary>
    /// Determines if save button should be disabled
    /// </summary>
    /// <param name="hasChanges">Whether settings have changes</param>
    /// <returns>True if button should be disabled</returns>
    public bool ShouldDisableSaveButton(bool hasChanges)
    {
        return !hasChanges;
    }

    /// <summary>
    /// Determines if reset button should be disabled
    /// </summary>
    /// <param name="isAtDefaults">Whether settings are at defaults</param>
    /// <returns>True if button should be disabled</returns>
    public bool ShouldDisableResetButton(bool isAtDefaults)
    {
        return isAtDefaults;
    }
}

/// <summary>
/// Settings validation result
/// </summary>
public class SettingsValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// File validation result
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
