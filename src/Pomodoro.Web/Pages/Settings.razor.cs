using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Code-behind for Settings page
/// </summary>
public class SettingsPageBase : ComponentBase
{
    #region Services (Dependency Injection)

    [Inject]
    protected ITimerService TimerService { get; set; } = default!;

    [Inject]
    protected IExportService ExportService { get; set; } = default!;

    [Inject]
    protected IImportService ImportService { get; set; } = default!;
    [Inject]
    protected ITaskService TaskService { get; set; } = default!;

    [Inject]
    protected IActivityService ActivityService { get; set; } = default!;

    [Inject]
    protected IJSInteropService JSInteropService { get; set; } = default!;

    [Inject]
    protected ILogger<SettingsPageBase> Logger { get; set; } = default!;

    [Inject]
    protected SettingsPresenterService SettingsPresenterService { get; set; } = null!;

    #endregion

    #region State

    protected TimerSettings Settings { get; set; } = new TimerSettings();

    protected TimerSettings OriginalSettings { get; set; } = new TimerSettings();

    protected bool ShowToast { get; set; }

    protected string? ToastMessage { get; set; }

    // Export/Import state
    protected bool IsExporting { get; set; }
    protected bool IsImporting { get; set; }
    protected bool IsClearing { get; set; }
    protected bool ShowClearConfirmation { get; set; }
    protected string? ImportResult { get; set; }

    /// <summary>
    /// Indicates whether current settings differ from the original saved settings
    /// </summary>
    protected bool HasChanges => !Settings.Equals(OriginalSettings);

    protected void MarkDirty() => StateHasChanged();

    protected async Task ShowTemporaryToastAsync(string message)
    {
        ToastMessage = message;
        ShowToast = true;
        StateHasChanged();

        SafeTaskRunner.RunAndForget(
            async () =>
            {
                await Task.Delay(Constants.UI.ToastDurationMs);
                ShowToast = false;
                ToastMessage = null;
                await InvokeAsync(StateHasChanged);
            },
            Logger,
            Constants.SafeTaskOperations.ToastHide
        );
    }

    /// <summary>
    /// Indicates whether current settings differ from default values
    /// </summary>
    protected bool IsAtDefaults => SettingsPresenterService.IsAtDefaults(Settings);

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        // Clone settings from TimerService for local editing
        Settings = TimerService.Settings.Clone();
        // Store original settings for comparison
        OriginalSettings = Settings.Clone();
    }

    #endregion

    #region Actions

    public async Task HandleSave()
    {
        await TimerService.UpdateSettingsAsync(Settings);

        // Update original settings after save using Clone() for consistency
        OriginalSettings = Settings.Clone();

        await ShowTemporaryToastAsync("Settings saved successfully!");
    }

    public void ResetToDefaults()
    {
        // Use model's default values - single source of truth
        Settings = new TimerSettings();
    }

    #endregion

    #region Export/Import Actions

    public async Task ExportJson()
    {
        IsExporting = true;
        StateHasChanged();

        try
        {
            var json = await ExportService.ExportToJsonAsync();
            var filename = $"pomodoro-backup-{DateTime.Today:yyyy-MM-dd}.json";
            await SettingsPresenterService.DownloadFileAsync(JSInteropService, filename, json, "application/json");

            await ShowTemporaryToastAsync("JSON backup exported successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to export JSON");
            ToastMessage = "Failed to export JSON backup. Please try again.";
            ShowToast = true;
            StateHasChanged();
        }
        finally
        {
            IsExporting = false;
            StateHasChanged();
        }
    }

    public async Task HandleImport(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null || file.Size == 0)
        {
            ImportResult = "No file selected";
            return;
        }

        // Validate file size to prevent memory issues
        if (file.Size > Constants.Validation.MaxImportFileSizeBytes)
        {
            ImportResult = $"File too large. Maximum size is {Constants.Validation.MaxImportFileSizeBytes / (1024 * 1024)} MB.";
            return;
        }

        IsImporting = true;
        ImportResult = null;
        StateHasChanged();

        try
        {
            // Use explicit max size to ensure it matches our validation
            using var stream = file.OpenReadStream(Constants.Validation.MaxImportFileSizeBytes);
            using var reader = new StreamReader(stream);
            var jsonData = await reader.ReadToEndAsync();

            var result = await ImportService.ImportFromJsonAsync(jsonData);

            if (result.Success)
            {
                // Build success message with detailed statistics
                var message = SettingsPresenterService.BuildImportSuccessMessage(result.TotalImported, result.TotalSkipped);

                // Reload all services to reflect imported data without page reload
                await TaskService.ReloadAsync();
                await ActivityService.ReloadAsync();

                // Update settings from the newly imported data
                Settings = TimerService.Settings.Clone();
                OriginalSettings = Settings.Clone();

                IsImporting = false;
                ImportResult = null;
                await ShowTemporaryToastAsync(message);
            }
            else
            {
                // Show error message without reloading
                ImportResult = result.ErrorMessage ?? "Import failed. Please check the file format.";
                IsImporting = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import JSON");
            ImportResult = "Import failed. Please check the file format.";
            IsImporting = false;
            StateHasChanged();
        }
    }

    public void ConfirmClearData()
    {
        ShowClearConfirmation = true;
    }

    public async Task ClearData()
    {
        ShowClearConfirmation = false;
        IsClearing = true;
        StateHasChanged();

        try
        {
            await ExportService.ClearAllDataAsync();

            // Reset settings to defaults
            Settings = new TimerSettings();
            await TimerService.UpdateSettingsAsync(Settings);

            OriginalSettings = new TimerSettings();

            // Reload all services to reflect cleared data without page reload
            await TaskService.ReloadAsync();
            await ActivityService.ReloadAsync();

            IsClearing = false;
            await ShowTemporaryToastAsync("All data cleared successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to clear data");
            IsClearing = false;
            StateHasChanged();
        }
    }

    #endregion
}
