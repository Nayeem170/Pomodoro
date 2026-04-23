namespace Pomodoro.Web.Services;

public interface IGoogleDriveService
{
    Task InitializeAsync(string clientId);
    Task<string?> ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected { get; }
    Task<string?> FindSyncFileAsync();
    Task<string> ReadFileAsync(string fileId);
    Task<string> CreateFileAsync(string fileName, string content);
    Task UpdateFileAsync(string fileId, string content);
    Task DeleteFileAsync(string fileId);
}
