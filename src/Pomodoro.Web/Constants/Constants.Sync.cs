namespace Pomodoro.Web;

public static partial class Constants
{
    public static class Sync
    {
        public const string SyncFileName = "pomodoro-sync.json";
        public const int SyncVersion = 1;
        public const int DebounceDelayMs = 5000;
        public const int PeriodicSyncIntervalMs = 30 * 60 * 1000;
        public const string AppDataFolder = "appDataFolder";
        public const string DriveScope = "https://www.googleapis.com/auth/drive.appdata";
        public const string DriveFilesEndpoint = "https://www.googleapis.com/drive/v3/files";
        public const string DriveUploadEndpoint = "https://www.googleapis.com/upload/drive/v3/files";
        public const string DefaultClientId = "778366289120-ejtm43pfvkgih3l3a5jp7op33alvnh97.apps.googleusercontent.com";
    }

    public static class SyncMessages
    {
        public const string Connected = "Connected to Google Drive";
        public const string Disconnected = "Disconnected from Google Drive";
        public const string Syncing = "Syncing...";
        public const string SyncComplete = "Sync complete";
        public const string SyncFailed = "Sync failed";
        public const string SyncPullSuccess = "Synced from cloud! {ActivityCount} activities, {TaskCount} tasks imported";
        public const string SyncPushSuccess = "Changes pushed to cloud";
        public const string ReconnectRequired = "Connection expired. Please reconnect Google Drive.";
        public const string NotConnected = "Not connected to Google Drive";
        public const string InitFailed = "Failed to initialize Google Drive";
        public const string AuthFailed = "Google authentication failed";
        public const string ClientIdRequired = "Please enter a Google Client ID first";
        public const string ClientIdInvalid = "Invalid Client ID format";
        public const string NoRemoteData = "No remote data found. This device will be the source.";
        public const string AlreadyUpToDate = "Already up to date";

        public const string LogSyncPush = "Pushing data to Google Drive";
        public const string LogSyncPull = "Pulling data from Google Drive";
        public const string LogSyncComplete = "Sync completed successfully";
        public const string LogSyncFailed = "Sync failed: {Error}";
        public const string LogSyncSkipped = "Sync skipped: {Reason}";
        public const string LogSyncUnauthorized = "Sync unauthorized, reconnection required";
        public const string LogSyncOffline = "Device is offline, queuing sync";
        public const string LogMarkDirty = "Data changed, scheduling sync in {DelayMs}ms";
        public const string LogDebouncedSync = "Debounced sync triggered";
        public const string LogPeriodicSync = "Periodic sync check";
        public const string LogInitSuccess = "Google Drive initialized with Client ID";
        public const string LogInitFailed = "Failed to initialize Google Drive: {Error}";
        public const string LogAuthSuccess = "Google Drive authenticated successfully";
        public const string LogAuthFailed = "Google Drive authentication failed: {Error}";
        public const string LogDisconnect = "Google Drive disconnected";
        public const string LogFileNotFound = "Sync file not found in Google Drive";
        public const string LogFileCreated = "Created new sync file: {FileId}";
        public const string LogFileUpdated = "Updated sync file: {FileId}";
        public const string LogCompressSuccess = "Data compressed: {OriginalBytes} -> {CompressedBytes} bytes";
        public const string LogDecompressSuccess = "Data decompressed: {CompressedBytes} -> {OriginalBytes} bytes";
        public const string LogRemoteNewer = "Remote data is newer, pulling changes";
        public const string LogLocalNewer = "Local data is newer, pushing changes";
        public const string LogDataEqual = "Local and remote data are in sync";
    }
}
