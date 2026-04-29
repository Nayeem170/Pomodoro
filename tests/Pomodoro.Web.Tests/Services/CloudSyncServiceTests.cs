using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class CloudSyncServiceTests : IDisposable
{
    private readonly Mock<IGoogleDriveService> _mockGoogleDrive;
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IImportService> _mockImportService;
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IIndexedDbService> _mockIndexedDb;
    private readonly Mock<ILogger<CloudSyncService>> _mockLogger;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ITimerService> _mockTimerService;
    private readonly CloudSyncService _sut;

    public CloudSyncServiceTests()
    {
        _mockGoogleDrive = new Mock<IGoogleDriveService>();
        _mockExportService = new Mock<IExportService>();
        _mockImportService = new Mock<IImportService>();
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockIndexedDb = new Mock<IIndexedDbService>();
        _mockLogger = new Mock<ILogger<CloudSyncService>>();
        _mockTaskService = new Mock<ITaskService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockTimerService = new Mock<ITimerService>();

        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(new SyncStateRecord());

        _sut = new CloudSyncService(
            _mockGoogleDrive.Object,
            _mockExportService.Object,
            _mockImportService.Object,
            _mockJsRuntime.Object,
            _mockIndexedDb.Object,
            _mockLogger.Object,
            _mockTaskService.Object,
            _mockActivityService.Object,
            _mockTimerService.Object);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    #region Constructor

    [Fact]
    public void Constructor_SetsDeviceId()
    {
        Assert.NotEmpty(_sut.DeviceId);
        Assert.Equal(12, _sut.DeviceId.Length);
    }

    [Fact]
    public void Constructor_NotConnectedByDefault()
    {
        Assert.False(_sut.IsConnected);
    }

    [Fact]
    public void Constructor_NotInitializedByDefault()
    {
        Assert.False(_sut.IsInitialized);
    }

    [Fact]
    public void Constructor_LastSyncedAtIsNull()
    {
        Assert.Null(_sut.LastSyncedAt);
    }

    [Fact]
    public void Constructor_ClientIdIsNull()
    {
        Assert.Null(_sut.ClientId);
    }

    #endregion

    #region InitializeAsync

    [Fact]
    public async Task InitializeAsync_LoadsStateFromIndexedDb()
    {
        var state = new SyncStateRecord
        {
            ClientId = "test-client-id",
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "device-123"
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);

        await _sut.InitializeAsync();

        Assert.Equal("test-client-id", _sut.ClientId);
        Assert.Equal(state.LastSyncedAt, _sut.LastSyncedAt);
        Assert.Equal("device-123", _sut.DeviceId);
    }

    [Fact]
    public async Task InitializeAsync_SetsIsInitialized()
    {
        await _sut.InitializeAsync();

        Assert.True(_sut.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WithConnectedState_InitializesGoogleDrive()
    {
        var state = new SyncStateRecord
        {
            ClientId = "test-client-id",
            IsConnected = true,
            AccessToken = "test-token"
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);

        await _sut.InitializeAsync();

        _mockGoogleDrive.Verify(g => g.InitializeAsync("test-client-id"), Times.Once);
        _mockGoogleDrive.Verify(g => g.SetAccessTokenAsync("test-token"), Times.Once);
        _mockGoogleDrive.Verify(g => g.SetConnected(true), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithConnectedState_SetsIsConnected()
    {
        var state = new SyncStateRecord
        {
            ClientId = "test-client-id",
            IsConnected = true,
            AccessToken = "test-token"
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);

        await _sut.InitializeAsync();

        Assert.True(_sut.IsConnected);
    }

    [Fact]
    public async Task InitializeAsync_WithoutClientId_DoesNotInitializeGoogleDrive()
    {
        var state = new SyncStateRecord
        {
            ClientId = null,
            IsConnected = false
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);

        await _sut.InitializeAsync();

        _mockGoogleDrive.Verify(g => g.InitializeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_RetriesOnFailure()
    {
        var state = new SyncStateRecord
        {
            ClientId = "test-client-id",
            IsConnected = true,
            AccessToken = "test-token"
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);
        _mockGoogleDrive
            .SetupSequence(g => g.InitializeAsync("test-client-id"))
            .ThrowsAsync(new Exception("Init error"))
            .ThrowsAsync(new Exception("Init error"))
            .Returns(Task.CompletedTask);

        await _sut.InitializeAsync();

        Assert.True(_sut.IsInitialized);
        _mockGoogleDrive.Verify(
            g => g.InitializeAsync("test-client-id"),
            Times.Exactly(3));
    }

    [Fact]
    public async Task InitializeAsync_SetsInitializedEvenAfterAllRetriesFail()
    {
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ThrowsAsync(new Exception("Persistent failure"));

        await _sut.InitializeAsync();

        Assert.True(_sut.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotReinitialize()
    {
        await _sut.InitializeAsync();
        await _sut.InitializeAsync();

        _mockIndexedDb.Verify(
            db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"),
            Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_RaisesOnSyncStatusChanged()
    {
        var eventRaised = false;
        _sut.OnSyncStatusChanged += () => eventRaised = true;

        await _sut.InitializeAsync();

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task InitializeAsync_WhenIndexedDbReturnsNull_UsesDefaults()
    {
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync((SyncStateRecord?)null);

        await _sut.InitializeAsync();

        Assert.Null(_sut.ClientId);
        Assert.Null(_sut.LastSyncedAt);
        Assert.True(_sut.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WithNullDeviceId_KeepsGeneratedId()
    {
        var state = new SyncStateRecord
        {
            DeviceId = null
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);

        var originalDeviceId = _sut.DeviceId;

        await _sut.InitializeAsync();

        Assert.Equal(originalDeviceId, _sut.DeviceId);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyAccessToken_DoesNotSetAccessToken()
    {
        var state = new SyncStateRecord
        {
            ClientId = "test-client-id",
            IsConnected = true,
            AccessToken = null
        };
        _mockIndexedDb
            .Setup(db => db.GetAsync<SyncStateRecord>(Constants.Storage.AppStateStore, "cloudSync"))
            .ReturnsAsync(state);
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);

        await _sut.InitializeAsync();

        _mockGoogleDrive.Verify(g => g.SetAccessTokenAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region ConnectAsync

    [Fact]
    public async Task ConnectAsync_ReturnsTrueOnSuccess()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");

        var result = await _sut.ConnectAsync("client-id");

        Assert.True(result);
    }

    [Fact]
    public async Task ConnectAsync_InitializesGoogleDrive()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");

        await _sut.ConnectAsync("client-id");

        _mockGoogleDrive.Verify(g => g.InitializeAsync("client-id"), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_SetsClientId()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");

        await _sut.ConnectAsync("client-id");

        Assert.Equal("client-id", _sut.ClientId);
    }

    [Fact]
    public async Task ConnectAsync_SavesSyncState()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");

        await _sut.ConnectAsync("client-id");

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                s.ClientId == "client-id" && s.IsConnected)),
            Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_RaisesOnSyncStatusChanged()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");
        var eventRaised = false;
        _sut.OnSyncStatusChanged += () => eventRaised = true;

        await _sut.ConnectAsync("client-id");

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task ConnectAsync_TriggersInitialSync()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.ConnectAsync("client-id");

        _mockGoogleDrive.Verify(g => g.FindSyncFileAsync(), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_ReturnsFalseOnException()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ThrowsAsync(new Exception("Auth failed"));

        var result = await _sut.ConnectAsync("client-id");

        Assert.False(result);
    }

    [Fact]
    public async Task ConnectAsync_OnException_DoesNotSetClientId()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ThrowsAsync(new Exception("Auth failed"));

        await _sut.ConnectAsync("client-id");

        Assert.Null(_sut.ClientId);
    }

    #endregion

    #region DisconnectAsync

    [Fact]
    public async Task DisconnectAsync_RevokesGoogleDriveAuth()
    {
        await _sut.DisconnectAsync();

        _mockGoogleDrive.Verify(g => g.DisconnectAsync(), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_ClearsClientId()
    {
        await _sut.DisconnectAsync();

        Assert.Null(_sut.ClientId);
    }

    [Fact]
    public async Task DisconnectAsync_ClearsLastSyncedAt()
    {
        await _sut.DisconnectAsync();

        Assert.Null(_sut.LastSyncedAt);
    }

    [Fact]
    public async Task DisconnectAsync_SavesSyncState()
    {
        await _sut.DisconnectAsync();

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                !s.IsConnected && s.ClientId == null && s.LastSyncedAt == null)),
            Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_RaisesOnSyncStatusChanged()
    {
        var eventRaised = false;
        _sut.OnSyncStatusChanged += () => eventRaised = true;

        await _sut.DisconnectAsync();

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task DisconnectAsync_WhenRevokeFails_StillClearsState()
    {
        _mockGoogleDrive.Setup(g => g.DisconnectAsync()).ThrowsAsync(new Exception("Revoke failed"));

        await _sut.DisconnectAsync();

        Assert.Null(_sut.ClientId);
        Assert.Null(_sut.LastSyncedAt);
    }

    #endregion

    #region SyncNowAsync

    [Fact]
    public async Task SyncNowAsync_WhenNotConnected_ReturnsFailed()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(false);

        var result = await _sut.SyncNowAsync();

        Assert.False(result.Success);
        Assert.Equal(Constants.SyncMessages.NotConnected, result.ErrorMessage);
        Assert.Equal(SyncAction.Failed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenNoRemoteFile_Pushes()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pushed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenNoRemoteFile_CreatesFile()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncNowAsync();

        _mockGoogleDrive.Verify(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()), Times.Once);
        _mockGoogleDrive.Verify(g => g.UpdateFileAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncNowAsync_WhenRemoteNewer_Pulls()
    {
        var remoteTime = DateTime.UtcNow.AddMinutes(10);
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = remoteTime,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        _mockImportService.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(1, 0, 1, 0, true));

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pulled, result.Action);
        Assert.Equal(1, result.ActivitiesImported);
        Assert.Equal(1, result.TasksImported);
    }

    [Fact]
    public async Task SyncNowAsync_WhenLocalNewer_Pushes()
    {
        var localTime = DateTime.UtcNow.AddMinutes(10);
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow.AddMinutes(-10),
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        SetLastSyncedAt(localTime);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.UpdateFileAsync("file-id", It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pushed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenLocalNewer_UpdatesExistingFile()
    {
        var localTime = DateTime.UtcNow.AddMinutes(10);
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow.AddMinutes(-10),
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        SetLastSyncedAt(localTime);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.UpdateFileAsync("file-id", It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.SyncNowAsync();

        _mockGoogleDrive.Verify(g => g.UpdateFileAsync("file-id", It.IsAny<string>()), Times.Once);
        _mockGoogleDrive.Verify(g => g.CreateFileAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncNowAsync_WhenTimestampsEqual_ReturnsUpToDate()
    {
        var syncTime = DateTime.UtcNow;
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = syncTime,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        SetLastSyncedAt(syncTime);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.UpToDate, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenRemoteNull_Pushes()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync("null");
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pushed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenLastSyncedAtNull_Pulls()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        _mockImportService.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        var result = await _sut.SyncNowAsync();

        Assert.True(result.Success);
        Assert.Equal(SyncAction.Pulled, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenUnauthorizedAccessException_ReturnsReconnectRequired()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        var result = await _sut.SyncNowAsync();

        Assert.False(result.Success);
        Assert.Equal(Constants.SyncMessages.ReconnectRequired, result.ErrorMessage);
        Assert.Equal(SyncAction.Failed, result.Action);
    }

    [Fact]
    public async Task SyncNowAsync_WhenUnauthorizedAccessException_ClearsAccessToken()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        await _sut.SyncNowAsync();

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                s.AccessToken == null)),
            Times.Once);
    }

    [Fact]
    public async Task SyncNowAsync_WhenGeneralException_ReturnsFailed()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var result = await _sut.SyncNowAsync();

        Assert.False(result.Success);
        Assert.Equal(SyncAction.Failed, result.Action);
        Assert.Contains("Network error", result.ErrorMessage);
    }

    #endregion

    #region PushAsync (via SyncNowAsync)

    [Fact]
    public async Task PushAsync_CompressesDataViaJs()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("export-data");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed-data");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncNowAsync();

        _mockJsRuntime.Verify(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()), Times.Once);
        _mockExportService.Verify(e => e.ExportToJsonStringAsync(), Times.Once);
    }

    [Fact]
    public async Task PushAsync_CreatesEnvelopeWithCorrectVersion()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncNowAsync();

        _mockGoogleDrive.Verify(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName,
            It.Is<string>(json => IsValidEnvelope(json, "compressed"))), Times.Once);
    }

    [Fact]
    public async Task PushAsync_SetsLastSyncedAt()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncNowAsync();

        Assert.NotNull(_sut.LastSyncedAt);
    }

    [Fact]
    public async Task PushAsync_SavesSyncState()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncNowAsync();

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                s.LastSyncedAt != null)),
            Times.Once);
    }

    [Fact]
    public async Task PushAsync_RaisesOnSyncStatusChanged()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        var eventRaised = false;
        _sut.OnSyncStatusChanged += () => eventRaised = true;

        await _sut.SyncNowAsync();

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task PushAsync_WhenCompressionFails_ReturnsFailed()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ThrowsAsync(new JSException("Compression failed"));

        var result = await _sut.SyncNowAsync();

        Assert.False(result.Success);
        Assert.Equal(SyncAction.Failed, result.Action);
    }

    #endregion

    #region PullAsync (via SyncNowAsync)

    [Fact]
    public async Task PullAsync_DecompressesDataViaJs()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("decompressed-data");
        _mockImportService.Setup(i => i.ImportFromStringAsync("decompressed-data"))
            .ReturnsAsync(ImportResult.Succeeded(2, 0, 3, 0, true));

        await _sut.SyncNowAsync();

        _mockJsRuntime.Verify(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()), Times.Once);
    }

    [Fact]
    public async Task PullAsync_ImportsData()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("decompressed-data");
        _mockImportService.Setup(i => i.ImportFromStringAsync("decompressed-data"))
            .ReturnsAsync(ImportResult.Succeeded(5, 2, 3, 1, true));

        var result = await _sut.SyncNowAsync();

        _mockImportService.Verify(i => i.ImportFromStringAsync("decompressed-data"), Times.Once);
        Assert.Equal(5, result.ActivitiesImported);
        Assert.Equal(2, result.ActivitiesSkipped);
        Assert.Equal(3, result.TasksImported);
        Assert.Equal(1, result.TasksSkipped);
        Assert.True(result.SettingsImported);
    }

    [Fact]
    public async Task PullAsync_WhenImportSucceedsWithData_ReloadsServices()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("decompressed-data");
        _mockImportService.Setup(i => i.ImportFromStringAsync("decompressed-data"))
            .ReturnsAsync(ImportResult.Succeeded(1, 0, 1, 0, true));

        await _sut.SyncNowAsync();

        _mockTaskService.Verify(t => t.ReloadAsync(), Times.Once);
        _mockActivityService.Verify(a => a.ReloadAsync(), Times.Once);
    }

    [Fact]
    public async Task PullAsync_WhenImportSucceedsWithNoData_DoesNotReloadServices()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("decompressed-data");
        _mockImportService.Setup(i => i.ImportFromStringAsync("decompressed-data"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        await _sut.SyncNowAsync();

        _mockTaskService.Verify(t => t.ReloadAsync(), Times.Never);
        _mockActivityService.Verify(a => a.ReloadAsync(), Times.Never);
    }

    [Fact]
    public async Task PullAsync_SetsLastSyncedAt()
    {
        var remoteTime = DateTime.UtcNow;
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = remoteTime,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        _mockImportService.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        await _sut.SyncNowAsync();

        Assert.Equal(remoteTime, _sut.LastSyncedAt);
    }

    [Fact]
    public async Task PullAsync_SavesSyncState()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        _mockImportService.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        await _sut.SyncNowAsync();

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                s.LastSyncedAt != null)),
            Times.Once);
    }

    [Fact]
    public async Task PullAsync_WhenNotCompressed_UsesDataDirectly()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = false,
            Data = "{\"raw\":\"data\"}"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockImportService.Setup(i => i.ImportFromStringAsync("{\"raw\":\"data\"}"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        await _sut.SyncNowAsync();

        _mockJsRuntime.Verify(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()), Times.Never);
        _mockImportService.Verify(i => i.ImportFromStringAsync("{\"raw\":\"data\"}"), Times.Once);
    }

    [Fact]
    public async Task PullAsync_WhenDecompressionFails_ReturnsFailed()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ThrowsAsync(new JSException("Decompression failed"));

        var result = await _sut.SyncNowAsync();

        Assert.False(result.Success);
        Assert.Equal(SyncAction.Failed, result.Action);
    }

    [Fact]
    public async Task PullAsync_RaisesOnSyncStatusChanged()
    {
        var envelope = new SyncEnvelope
        {
            Version = Constants.Sync.SyncVersion,
            LastSyncedAt = DateTime.UtcNow,
            DeviceId = "remote-device",
            Compressed = true,
            Data = "compressed-data"
        };
        var envelopeJson = JsonSerializer.Serialize(envelope);

        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");
        _mockGoogleDrive.Setup(g => g.ReadFileAsync("file-id")).ReturnsAsync(envelopeJson);
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipDecompress, It.IsAny<object?[]>()))
            .ReturnsAsync("{}");
        _mockImportService.Setup(i => i.ImportFromStringAsync("{}"))
            .ReturnsAsync(ImportResult.Succeeded(0, 0, 0, 0, false));

        var eventRaised = false;
        _sut.OnSyncStatusChanged += () => eventRaised = true;

        await _sut.SyncNowAsync();

        Assert.True(eventRaised);
    }

    #endregion

    #region ScheduleSyncAsync

    [Fact]
    public async Task ScheduleSyncAsync_WhenNotConnected_DoesNothing()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(false);

        await _sut.ScheduleSyncAsync();

        _mockExportService.Verify(e => e.ExportToJsonStringAsync(), Times.Never);
    }

    [Fact]
    public async Task ScheduleSyncAsync_WhenConnected_DebouncesPush()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.ScheduleSyncAsync();

        var tcs = new TaskCompletionSource();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(Constants.Sync.DebounceDelayMs + 1000));

        try
        {
            await Task.Delay(Constants.Sync.DebounceDelayMs + 500, cts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        _mockExportService.Verify(e => e.ExportToJsonStringAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ScheduleSyncAsync_CalledMultipleTimes_OnlyPushesOnce()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.ScheduleSyncAsync();
        await Task.Delay(100);
        await _sut.ScheduleSyncAsync();
        await Task.Delay(100);
        await _sut.ScheduleSyncAsync();

        await Task.Delay(Constants.Sync.DebounceDelayMs + 500);

        _mockExportService.Verify(e => e.ExportToJsonStringAsync(),
            Times.Once);
    }

    #endregion

    #region SyncInBackgroundAsync

    [Fact]
    public async Task SyncInBackgroundAsync_WhenNotInitialized_DoesNothing()
    {
        await _sut.SyncInBackgroundAsync();

        _mockGoogleDrive.Verify(g => g.FindSyncFileAsync(), Times.Never);
    }

    [Fact]
    public async Task SyncInBackgroundAsync_WhenNotConnected_DoesNothing()
    {
        await _sut.InitializeAsync();
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(false);

        await _sut.SyncInBackgroundAsync();

        _mockGoogleDrive.Verify(g => g.FindSyncFileAsync(), Times.Never);
    }

    #endregion

    #region ClearRemoteDataAsync

    [Fact]
    public async Task ClearRemoteDataAsync_DeletesRemoteFile()
    {
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");

        await _sut.ClearRemoteDataAsync();

        _mockGoogleDrive.Verify(g => g.DeleteFileAsync("file-id"), Times.Once);
    }

    [Fact]
    public async Task ClearRemoteDataAsync_WhenNoRemoteFile_DoesNotDelete()
    {
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);

        await _sut.ClearRemoteDataAsync();

        _mockGoogleDrive.Verify(g => g.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ClearRemoteDataAsync_ClearsLastSyncedAt()
    {
        SetLastSyncedAt(DateTime.UtcNow);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");

        await _sut.ClearRemoteDataAsync();

        Assert.Null(_sut.LastSyncedAt);
    }

    [Fact]
    public async Task ClearRemoteDataAsync_SavesSyncState()
    {
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync("file-id");

        await _sut.ClearRemoteDataAsync();

        _mockIndexedDb.Verify(
            db => db.PutAsync(Constants.Storage.AppStateStore, It.Is<SyncStateRecord>(s =>
                s.LastSyncedAt == null)),
            Times.Once);
    }

    [Fact]
    public async Task ClearRemoteDataAsync_WhenUnauthorized_Throws()
    {
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.ClearRemoteDataAsync());
    }

    [Fact]
    public async Task ClearRemoteDataAsync_WhenGeneralException_Throws()
    {
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ClearRemoteDataAsync());
    }

    #endregion

    #region OnSyncStatusChanged Event

    [Fact]
    public void OnSyncStatusChanged_CanBeSubscribedAndUnsubscribed()
    {
        var count = 0;
        Action handler = () => count++;

        _sut.OnSyncStatusChanged += handler;
        _sut.GetType().GetMethod("NotifyStatusChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_sut, null);
        Assert.Equal(1, count);

        _sut.OnSyncStatusChanged -= handler;
        _sut.GetType().GetMethod("NotifyStatusChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_sut, null);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnSyncStatusChanged_MultipleHandlers_AllInvoked()
    {
        var count1 = 0;
        var count2 = 0;
        _sut.OnSyncStatusChanged += () => count1++;
        _sut.OnSyncStatusChanged += () => count2++;

        await _sut.InitializeAsync();

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
    }

    #endregion

    #region SyncInBackgroundAsync - Connected Path

    [Fact]
    public async Task SyncInBackgroundAsync_WhenInitializedAndConnected_Syncs()
    {
        await _sut.InitializeAsync();
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        await _sut.SyncInBackgroundAsync();

        await Task.Delay(500);

        _mockGoogleDrive.Verify(g => g.FindSyncFileAsync(), Times.Once);
    }

    #endregion

    #region ScheduleSyncAsync - Exception Path

    [Fact]
    public async Task ScheduleSyncAsync_WhenPushFails_LogsError()
    {
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync())
            .ThrowsAsync(new Exception("Export failed"));

        await _sut.ScheduleSyncAsync();

        await Task.Delay(Constants.Sync.DebounceDelayMs + 500);

        _mockLogger.Verify(
            l => l.Log(It.Is<Microsoft.Extensions.Logging.LogLevel>(ll => ll == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region StartPeriodicSync - Timer Callback

    [Fact]
    public async Task StartPeriodicSync_CreatesTimer_WhenConnected()
    {
        _mockGoogleDrive.Setup(g => g.ConnectAsync()).ReturnsAsync("token");

        await _sut.ConnectAsync("test-client");

        var field = typeof(CloudSyncService).GetField("_periodicSyncTimer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var timer = (Timer?)field?.GetValue(_sut);

        Assert.NotNull(timer);
        timer!.Dispose();
    }

    #endregion

    #region SaveSyncStateAsync - Exception Path

    [Fact]
    public async Task SaveSyncStateAsync_WhenIndexedDbFails_LogsWarning()
    {
        await _sut.InitializeAsync();
        _mockGoogleDrive.Setup(g => g.IsConnected).Returns(true);
        _mockGoogleDrive.Setup(g => g.FindSyncFileAsync()).ReturnsAsync((string?)null);
        _mockExportService.Setup(e => e.ExportToJsonStringAsync()).ReturnsAsync("{}");
        _mockJsRuntime.Setup(js => js.InvokeAsync<string>(
            Constants.CompressionJsFunctions.GzipCompress, It.IsAny<object?[]>()))
            .ReturnsAsync("compressed");
        _mockGoogleDrive.Setup(g => g.CreateFileAsync(
            Constants.Sync.SyncFileName, It.IsAny<string>()))
            .ReturnsAsync("file-id");

        _mockIndexedDb.Setup(db => db.PutAsync(Constants.Storage.AppStateStore, It.IsAny<SyncStateRecord>()))
            .ThrowsAsync(new Exception("DB write failed"));

        await _sut.SyncNowAsync();

        _mockLogger.Verify(
            l => l.Log(It.Is<Microsoft.Extensions.Logging.LogLevel>(ll => ll == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Dispose();
        _sut.Dispose();
    }

    #endregion

    private void SetLastSyncedAt(DateTime value)
    {
        var field = typeof(CloudSyncService).GetProperty("LastSyncedAt");
        field?.SetValue(_sut, value);
    }

    private static bool IsValidEnvelope(string json, string expectedData)
    {
        var envelope = JsonSerializer.Deserialize<SyncEnvelope>(json);
        return envelope != null &&
               envelope.Version == Constants.Sync.SyncVersion &&
               envelope.Compressed &&
               envelope.Data == expectedData;
    }
}
