using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<GoogleDriveService> _logger;
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public GoogleDriveService(IJSRuntime jsRuntime, ILogger<GoogleDriveService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public void SetConnected(bool connected) => _isConnected = connected;

    public async Task SetAccessTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync(Constants.GoogleDriveJsFunctions.SetAccessToken, token);
        _isConnected = true;
    }

    public async Task<bool> TrySilentAuthAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>(Constants.GoogleDriveJsFunctions.TrySilentAuth);
            if (!string.IsNullOrEmpty(token))
            {
                _isConnected = true;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task InitializeAsync(string clientId)
    {
        try
        {
            await _jsRuntime.InvokeAsync<object>(Constants.GoogleDriveJsFunctions.Init, clientId, false);
            _logger.LogInformation(Constants.SyncMessages.LogInitSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.SyncMessages.LogInitFailed, ex.Message);
            throw;
        }
    }

    public async Task<string?> ConnectAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>(Constants.GoogleDriveJsFunctions.RequestAuth);
            _isConnected = true;
            _logger.LogInformation(Constants.SyncMessages.LogAuthSuccess);
            return token;
        }
        catch (Exception ex)
        {
            _isConnected = false;
            _logger.LogError(ex, Constants.SyncMessages.LogAuthFailed, ex.Message);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.GoogleDriveJsFunctions.RevokeAuth);
            _isConnected = false;
            _logger.LogInformation(Constants.SyncMessages.LogDisconnect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect Google Drive");
            throw;
        }
    }

    public async Task<string?> FindSyncFileAsync()
    {
        try
        {
            var fileId = await _jsRuntime.InvokeAsync<string?>(Constants.GoogleDriveJsFunctions.FindSyncFile, Constants.Sync.SyncFileName);
            if (fileId == null)
            {
                _logger.LogDebug(Constants.SyncMessages.LogFileNotFound);
            }
            return fileId;
        }
        catch (JSException ex) when (ex.Message.Contains("401"))
        {
            _isConnected = false;
            _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
            throw new UnauthorizedAccessException(Constants.SyncMessages.ReconnectRequired, ex);
        }
    }

    public async Task<string> ReadFileAsync(string fileId)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>(Constants.GoogleDriveJsFunctions.ReadFile, fileId);
        }
        catch (JSException ex) when (ex.Message.Contains("401"))
        {
            _isConnected = false;
            _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
            throw new UnauthorizedAccessException(Constants.SyncMessages.ReconnectRequired, ex);
        }
    }

    public async Task<string> CreateFileAsync(string fileName, string content)
    {
        try
        {
            var fileId = await _jsRuntime.InvokeAsync<string>(Constants.GoogleDriveJsFunctions.CreateFile, fileName, content);
            _logger.LogInformation(Constants.SyncMessages.LogFileCreated, fileId);
            return fileId;
        }
        catch (JSException ex) when (ex.Message.Contains("401"))
        {
            _isConnected = false;
            _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
            throw new UnauthorizedAccessException(Constants.SyncMessages.ReconnectRequired, ex);
        }
    }

    public async Task UpdateFileAsync(string fileId, string content)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.GoogleDriveJsFunctions.UpdateFile, fileId, content);
            _logger.LogInformation(Constants.SyncMessages.LogFileUpdated, fileId);
        }
        catch (JSException ex) when (ex.Message.Contains("401"))
        {
            _isConnected = false;
            _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
            throw new UnauthorizedAccessException(Constants.SyncMessages.ReconnectRequired, ex);
        }
    }

    public async Task DeleteFileAsync(string fileId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.GoogleDriveJsFunctions.DeleteFile, fileId);
        }
        catch (JSException ex) when (ex.Message.Contains("401"))
        {
            _isConnected = false;
            _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
            throw new UnauthorizedAccessException(Constants.SyncMessages.ReconnectRequired, ex);
        }
    }
}
