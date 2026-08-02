using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<GoogleDriveService> _logger;
    private bool _isConnected;
    private string? _accountEmail;
    private string? _accessToken;
    private long _tokenExpiresAtMs;

    public bool IsConnected => _isConnected;
    public string? AccountEmail => _accountEmail;
    public string? AccessToken => _accessToken;
    public DateTime? TokenExpiresAt => _tokenExpiresAtMs > 0
        ? DateTimeOffset.FromUnixTimeMilliseconds(_tokenExpiresAtMs).UtcDateTime
        : null;

    public GoogleDriveService(IJSRuntime jsRuntime, ILogger<GoogleDriveService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public void SetConnected(bool connected) => _isConnected = connected;

    public void SetAccountEmail(string? email) => _accountEmail = email;

    public async Task<bool> TrySilentAuthAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>(Constants.GoogleDriveJsFunctions.TrySilentAuth);
            if (!string.IsNullOrEmpty(token))
            {
                _accessToken = token;
                _tokenExpiresAtMs = TokenExpiryFromNow();
                _isConnected = true;
                await FetchAccountEmailAsync(token);
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
            _accessToken = token;
            _tokenExpiresAtMs = TokenExpiryFromNow();
            _isConnected = true;
            await FetchAccountEmailAsync(token);
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
            _accountEmail = null;
            _accessToken = null;
            _tokenExpiresAtMs = 0;
            _logger.LogInformation(Constants.SyncMessages.LogDisconnect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect Google Drive");
            throw;
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>(Constants.GoogleDriveJsFunctions.GetAccessToken);
    }

    public async Task RestoreAccessTokenAsync(string? token, DateTime? expiresAt)
    {
        if (string.IsNullOrEmpty(token) || expiresAt is null) return;
        var expiresAtMs = new DateTimeOffset(expiresAt.Value).ToUnixTimeMilliseconds();
        await _jsRuntime.InvokeVoidAsync(Constants.GoogleDriveJsFunctions.SetAccessToken, token, expiresAtMs);
        _accessToken = token;
        _tokenExpiresAtMs = expiresAtMs;
        _isConnected = true;
    }

    private static long TokenExpiryFromNow() =>
        DateTimeOffset.UtcNow.Add(Constants.Sync.AccessTokenLifetime).ToUnixTimeMilliseconds();

    private async Task FetchAccountEmailAsync(string accessToken)
    {
        try
        {
            var email = await _jsRuntime.InvokeAsync<string?>(
                Constants.GoogleDriveJsFunctions.GetUserInfo, accessToken);
            _accountEmail = email;
            if (!string.IsNullOrEmpty(email))
            {
                _logger.LogInformation(Constants.SyncMessages.LogUserInfoSuccess, email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.SyncMessages.LogUserInfoFetchFailed, ex.Message);
            _accountEmail = null;
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
