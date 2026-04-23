using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public interface ICloudSyncService
{
    bool IsConnected { get; }
    bool IsInitialized { get; }
    string? ClientId { get; }
    DateTime? LastSyncedAt { get; }
    string DeviceId { get; }
    event Action? OnSyncStatusChanged;
    Task InitializeAsync();
    Task<bool> ConnectAsync(string clientId);
    Task DisconnectAsync();
    Task<SyncResult> SyncNowAsync();
    Task AutoSyncOnStartAsync();
    Task MarkDirtyAsync();
}

public class SyncResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ActivitiesImported { get; set; }
    public int ActivitiesSkipped { get; set; }
    public int TasksImported { get; set; }
    public int TasksSkipped { get; set; }
    public bool SettingsImported { get; set; }
    public SyncAction Action { get; set; }

    public static SyncResult Pushed() => new() { Success = true, Action = SyncAction.Pushed };
    public static SyncResult Pulled(int activitiesImported, int activitiesSkipped, int tasksImported, int tasksSkipped, bool settingsImported) => new()
    {
        Success = true,
        Action = SyncAction.Pulled,
        ActivitiesImported = activitiesImported,
        ActivitiesSkipped = activitiesSkipped,
        TasksImported = tasksImported,
        TasksSkipped = tasksSkipped,
        SettingsImported = settingsImported
    };
    public static SyncResult UpToDate() => new() { Success = true, Action = SyncAction.UpToDate };
    public static SyncResult Failed(string error) => new() { Success = false, ErrorMessage = error, Action = SyncAction.Failed };
}

public enum SyncAction
{
    Pushed,
    Pulled,
    UpToDate,
    Failed
}

public class CloudSyncService : ICloudSyncService, IDisposable
{
    private readonly IGoogleDriveService _googleDriveService;
    private readonly IExportService _exportService;
    private readonly IImportService _importService;
    private readonly IJSRuntime _jsRuntime;
    private readonly IIndexedDbService _indexedDb;
    private readonly ILogger<CloudSyncService> _logger;
    private readonly ITaskService _taskService;
    private readonly IActivityService _activityService;
    private readonly ITimerService _timerService;

    private CancellationTokenSource? _debounceCts;
    private Timer? _periodicSyncTimer;
    private bool _isInitialized;
    private bool _isDisposed;

    public bool IsConnected => _googleDriveService.IsConnected;
    public bool IsInitialized => _isInitialized;
    public DateTime? LastSyncedAt { get; private set; }
    private string _deviceId;
    public string DeviceId => _deviceId;
    public string? ClientId { get; private set; }
    public event Action? OnSyncStatusChanged;

    public CloudSyncService(
        IGoogleDriveService googleDriveService,
        IExportService exportService,
        IImportService importService,
        IJSRuntime jsRuntime,
        IIndexedDbService indexedDb,
        ILogger<CloudSyncService> logger,
        ITaskService taskService,
        IActivityService activityService,
        ITimerService timerService)
    {
        _googleDriveService = googleDriveService;
        _exportService = exportService;
        _importService = importService;
        _jsRuntime = jsRuntime;
        _indexedDb = indexedDb;
        _logger = logger;
        _taskService = taskService;
        _activityService = activityService;
        _timerService = timerService;
        _deviceId = Guid.NewGuid().ToString("N")[..12];
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            var syncState = await LoadSyncStateAsync();
            ClientId = syncState.ClientId;
            LastSyncedAt = syncState.LastSyncedAt;
            _deviceId = syncState.DeviceId ?? _deviceId;

            if (!string.IsNullOrEmpty(ClientId))
            {
                await _googleDriveService.InitializeAsync(ClientId);
                StartPeriodicSync();
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogInitFailed, ex.Message);
        }
    }

    public async Task<bool> ConnectAsync(string clientId)
    {
        try
        {
            await _googleDriveService.InitializeAsync(clientId);
            await _googleDriveService.ConnectAsync();

            ClientId = clientId;
            await SaveSyncStateAsync();

            StartPeriodicSync();
            NotifyStatusChanged();

            var syncResult = await SyncNowAsync();
            if (!syncResult.Success)
            {
                _logger.LogWarning(Constants.SyncMessages.LogSyncFailed, syncResult.ErrorMessage);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogAuthFailed, ex.Message);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        StopPeriodicSync();

        try
        {
            await _googleDriveService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke Google token during disconnect");
        }

        ClientId = null;
        LastSyncedAt = null;
        await SaveSyncStateAsync();
        NotifyStatusChanged();
    }

    public async Task<SyncResult> SyncNowAsync()
    {
        if (!IsConnected)
        {
            return SyncResult.Failed(Constants.SyncMessages.NotConnected);
        }

        try
        {
            var fileId = await _googleDriveService.FindSyncFileAsync();

            if (fileId == null)
            {
                return await PushAsync();
            }

            var remoteJson = await _googleDriveService.ReadFileAsync(fileId);
            var remoteEnvelope = JsonSerializer.Deserialize<SyncEnvelope>(remoteJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (remoteEnvelope == null)
            {
                return await PushAsync();
            }

            var remoteLastSynced = remoteEnvelope.LastSyncedAt;

            if (LastSyncedAt == null || remoteLastSynced > LastSyncedAt)
            {
                return await PullAsync(fileId, remoteEnvelope);
            }

            if (LastSyncedAt > remoteLastSynced)
            {
                return await PushAsync(fileId);
            }

            _logger.LogInformation(Constants.SyncMessages.LogDataEqual);
            return SyncResult.UpToDate();
        }
        catch (UnauthorizedAccessException)
        {
            NotifyStatusChanged();
            return SyncResult.Failed(Constants.SyncMessages.ReconnectRequired);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogSyncFailed, ex.Message);
            return SyncResult.Failed(ex.Message);
        }
    }

    public Task AutoSyncOnStartAsync()
    {
        if (!_isInitialized || !IsConnected) return Task.CompletedTask;

        SafeTaskRunner.RunAndForget(
            async () =>
            {
                await Task.Delay(1000);
                var result = await SyncNowAsync();
                if (!result.Success && result.ErrorMessage == Constants.SyncMessages.ReconnectRequired)
                {
                    NotifyStatusChanged();
                }
            },
            _logger,
            Constants.SafeTaskOperations.CloudSyncAutoSync
        );

        return Task.CompletedTask;
    }

    public Task MarkDirtyAsync()
    {
        if (!IsConnected) return Task.CompletedTask;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        var token = _debounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Constants.Sync.DebounceDelayMs, token);
                if (token.IsCancellationRequested) return;

                _logger.LogDebug(Constants.SyncMessages.LogDebouncedSync);
                await PushAsync();
            }
            catch (OperationCanceledException)
            {
                // Debounce cancelled, new dirty signal received
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Constants.SyncMessages.LogSyncFailed, ex.Message);
            }
        }, token);

        return Task.CompletedTask;
    }

    private async Task<SyncResult> PushAsync(string? existingFileId = null)
    {
        try
        {
            _logger.LogInformation(Constants.SyncMessages.LogSyncPush);

            var exportJson = await _exportService.ExportToJsonStringAsync();
            var compressedBase64 = await _jsRuntime.InvokeAsync<string>(
                Constants.CompressionJsFunctions.GzipCompress, exportJson);

            var envelope = new SyncEnvelope
            {
                Version = Constants.Sync.SyncVersion,
                LastSyncedAt = DateTime.UtcNow,
                DeviceId = DeviceId,
                Compressed = true,
                Data = compressedBase64
            };

            var envelopeJson = JsonSerializer.Serialize(envelope);

            if (existingFileId != null)
            {
                await _googleDriveService.UpdateFileAsync(existingFileId, envelopeJson);
            }
            else
            {
                await _googleDriveService.CreateFileAsync(Constants.Sync.SyncFileName, envelopeJson);
            }

            LastSyncedAt = envelope.LastSyncedAt;
            await SaveSyncStateAsync();
            NotifyStatusChanged();

            _logger.LogInformation(Constants.SyncMessages.LogSyncComplete);
            return SyncResult.Pushed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogSyncFailed, ex.Message);
            return SyncResult.Failed(ex.Message);
        }
    }

    private async Task<SyncResult> PullAsync(string fileId, SyncEnvelope remoteEnvelope)
    {
        try
        {
            _logger.LogInformation(Constants.SyncMessages.LogSyncPull);

            string exportJson;
            if (remoteEnvelope.Compressed && !string.IsNullOrEmpty(remoteEnvelope.Data))
            {
                exportJson = await _jsRuntime.InvokeAsync<string>(
                    Constants.CompressionJsFunctions.GzipDecompress, remoteEnvelope.Data);
            }
            else
            {
                exportJson = remoteEnvelope.Data ?? string.Empty;
            }

            var result = await _importService.ImportFromStringAsync(exportJson);

            if (result.Success && result.TotalImported > 0)
            {
                await _taskService.ReloadAsync();
                await _activityService.ReloadAsync();
            }

            LastSyncedAt = remoteEnvelope.LastSyncedAt;
            await SaveSyncStateAsync();
            NotifyStatusChanged();

            _logger.LogInformation(Constants.SyncMessages.LogSyncComplete);
            return SyncResult.Pulled(
                result.ActivitiesImported, result.ActivitiesSkipped,
                result.TasksImported, result.TasksSkipped,
                result.SettingsImported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogSyncFailed, ex.Message);
            return SyncResult.Failed(ex.Message);
        }
    }

    private void StartPeriodicSync()
    {
        StopPeriodicSync();
        _periodicSyncTimer = new Timer(state =>
        {
            if (!IsConnected) return;

            _logger.LogDebug(Constants.SyncMessages.LogPeriodicSync);
            SafeTaskRunner.RunAndForget(
                () => SyncNowAsync(),
                _logger,
                Constants.SafeTaskOperations.CloudSyncPeriodic
            );
        }, null, Constants.Sync.PeriodicSyncIntervalMs, Constants.Sync.PeriodicSyncIntervalMs);
    }

    private void StopPeriodicSync()
    {
        _periodicSyncTimer?.Dispose();
        _periodicSyncTimer = null;
    }

    private async Task<SyncStateRecord> LoadSyncStateAsync()
    {
        try
        {
            return await _indexedDb.GetAsync<SyncStateRecord>(
                Constants.Storage.AppStateStore, "cloudSync") ?? new SyncStateRecord();
        }
        catch
        {
            return new SyncStateRecord();
        }
    }

    private async Task SaveSyncStateAsync()
    {
        try
        {
            var state = new SyncStateRecord
            {
                ClientId = ClientId,
                LastSyncedAt = LastSyncedAt,
                DeviceId = DeviceId
            };
            await _indexedDb.PutAsync(Constants.Storage.AppStateStore, state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save sync state");
        }
    }

    private void NotifyStatusChanged() => OnSyncStatusChanged?.Invoke();

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        StopPeriodicSync();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
    }
}

public class SyncEnvelope
{
    public int Version { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public bool Compressed { get; set; }
    public string? Data { get; set; }
}

public class SyncStateRecord
{
    public string Id { get; set; } = "cloudSync";
    public string? ClientId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? DeviceId { get; set; }
}
