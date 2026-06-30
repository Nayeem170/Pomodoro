using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class GoogleDriveServiceTests : IDisposable
{
    private readonly TestJsRuntime _jsRuntime;
    private readonly Mock<ILogger<GoogleDriveService>> _logger;
    private readonly GoogleDriveService _service;

    public GoogleDriveServiceTests()
    {
        _jsRuntime = new TestJsRuntime();
        _logger = new Mock<ILogger<GoogleDriveService>>();
        _service = new GoogleDriveService(_jsRuntime, _logger.Object);
    }

    public void Dispose() => _jsRuntime.Dispose();

    [Fact]
    public async Task InitializeAsync_InvokesJsInit()
    {
        await _service.InitializeAsync("test-client-id");

        Assert.Equal("googleDrive.init", _jsRuntime.LastMethod);
        Assert.Equal("test-client-id", _jsRuntime.LastArgs?[0]);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsOnFailure()
    {
        _jsRuntime.ThrowNext = new Exception("Init failed");

        var ex = await Assert.ThrowsAsync<Exception>(() => _service.InitializeAsync("test-client-id"));
        Assert.Equal("Init failed", ex.Message);
    }

    [Fact]
    public async Task ConnectAsync_InvokesJsRequestAuth()
    {
        _jsRuntime.NextResult = "test-token";
        _jsRuntime.NextResult = "user@example.com";

        var token = await _service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.True(_service.IsConnected);
        Assert.Equal("user@example.com", _service.AccountEmail);
    }

    [Fact]
    public async Task ConnectAsync_SetsIsConnectedFalse_OnError()
    {
        _jsRuntime.ThrowNext = new Exception("Auth failed");

        await Assert.ThrowsAsync<Exception>(() => _service.ConnectAsync());
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task DisconnectAsync_InvokesJsRevokeAuth()
    {
        await _service.DisconnectAsync();

        Assert.Equal("googleDrive.revokeAuth", _jsRuntime.LastMethod);
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task DisconnectAsync_SetsIsConnectedFalse_OnError()
    {
        _jsRuntime.ThrowNext = new Exception("Disconnect failed");

        await Assert.ThrowsAsync<Exception>(() => _service.DisconnectAsync());
    }

    [Fact]
    public void SetConnected_SetsFlag()
    {
        _service.SetConnected(true);
        Assert.True(_service.IsConnected);

        _service.SetConnected(false);
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task TrySilentAuthAsync_InvokesJs()
    {
        _jsRuntime.NextResult = "silent-token";
        _jsRuntime.NextResult = "user@example.com";

        var result = await _service.TrySilentAuthAsync();

        Assert.True(result);
        Assert.True(_service.IsConnected);
        Assert.Equal("user@example.com", _service.AccountEmail);
    }

    [Fact]
    public async Task TrySilentAuthAsync_ReturnsFalse_WhenNoToken()
    {
        _jsRuntime.NextResult = (string?)null;

        var result = await _service.TrySilentAuthAsync();

        Assert.False(result);
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task TrySilentAuthAsync_ReturnsFalse_OnException()
    {
        _jsRuntime.ThrowNext = new Exception("Silent auth failed");

        var result = await _service.TrySilentAuthAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task FindSyncFileAsync_InvokesJs()
    {
        _jsRuntime.NextResult = (string?)"file-id-123";

        var fileId = await _service.FindSyncFileAsync();

        Assert.Equal("file-id-123", fileId);
        Assert.Equal("googleDrive.findSyncFile", _jsRuntime.LastMethod);
        Assert.Equal(Constants.Sync.SyncFileName, _jsRuntime.LastArgs?[0]);
    }

    [Fact]
    public async Task FindSyncFileAsync_ThrowsOn401()
    {
        _jsRuntime.ThrowNext = new JSException("Error 401: Unauthorized");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.FindSyncFileAsync());
        Assert.Contains(Constants.SyncMessages.ReconnectRequired, ex.Message);
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task FindSyncFileAsync_ReturnsNull_WhenFileNotFound()
    {
        _jsRuntime.NextResult = (string?)null;

        var fileId = await _service.FindSyncFileAsync();

        Assert.Null(fileId);
    }

    [Fact]
    public async Task ReadFileAsync_InvokesJs()
    {
        _jsRuntime.NextResult = "file-content";

        var content = await _service.ReadFileAsync("file-id-123");

        Assert.Equal("file-content", content);
        Assert.Equal("googleDrive.readFile", _jsRuntime.LastMethod);
        Assert.Equal("file-id-123", _jsRuntime.LastArgs?[0]);
    }

    [Fact]
    public async Task ReadFileAsync_ThrowsOn401()
    {
        _jsRuntime.ThrowNext = new JSException("Error 401: Unauthorized");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ReadFileAsync("file-id"));
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task CreateFileAsync_InvokesJs()
    {
        _jsRuntime.NextResult = "new-file-id";

        var fileId = await _service.CreateFileAsync("sync.json", "content");

        Assert.Equal("new-file-id", fileId);
        Assert.Equal("googleDrive.createFile", _jsRuntime.LastMethod);
        Assert.Equal("sync.json", _jsRuntime.LastArgs?[0]);
        Assert.Equal("content", _jsRuntime.LastArgs?[1]);
    }

    [Fact]
    public async Task CreateFileAsync_ThrowsOn401()
    {
        _jsRuntime.ThrowNext = new JSException("Error 401: Unauthorized");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateFileAsync("f", "c"));
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task UpdateFileAsync_InvokesJs()
    {
        await _service.UpdateFileAsync("file-id", "new-content");

        Assert.Equal("googleDrive.updateFile", _jsRuntime.LastMethod);
        Assert.Equal("file-id", _jsRuntime.LastArgs?[0]);
        Assert.Equal("new-content", _jsRuntime.LastArgs?[1]);
    }

    [Fact]
    public async Task UpdateFileAsync_ThrowsOn401()
    {
        _jsRuntime.ThrowNext = new JSException("Error 401: Unauthorized");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateFileAsync("f", "c"));
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task DeleteFileAsync_InvokesJs()
    {
        await _service.DeleteFileAsync("file-id");

        Assert.Equal("googleDrive.deleteFile", _jsRuntime.LastMethod);
        Assert.Equal("file-id", _jsRuntime.LastArgs?[0]);
    }

    [Fact]
    public async Task DeleteFileAsync_ThrowsOn401()
    {
        _jsRuntime.ThrowNext = new JSException("Error 401: Unauthorized");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteFileAsync("file-id"));
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task DisconnectAsync_ClearsAccountEmail()
    {
        _service.SetAccountEmail("user@example.com");
        Assert.Equal("user@example.com", _service.AccountEmail);

        await _service.DisconnectAsync();

        Assert.Null(_service.AccountEmail);
    }

    [Fact]
    public async Task SetAccountEmail_UpdatesProperty()
    {
        _service.SetAccountEmail("user@example.com");
        Assert.Equal("user@example.com", _service.AccountEmail);

        _service.SetAccountEmail(null);
        Assert.Null(_service.AccountEmail);
    }

    [Fact]
    public async Task ConnectAsync_FetchesEmail()
    {
        _jsRuntime.NextResult = "test-token";
        _jsRuntime.NextResult = "user@example.com";

        var token = await _service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.Equal("user@example.com", _service.AccountEmail);
    }

    [Fact]
    public async Task ConnectAsync_HandlesUserInfoFailure()
    {
        _jsRuntime.NextResult = "test-token";
        _jsRuntime.NextResult = null;

        var token = await _service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.Null(_service.AccountEmail);
        Assert.True(_service.IsConnected);
    }

    [Fact]
    public async Task ConnectAsync_HandlesUserInfoNull()
    {
        _jsRuntime.NextResult = "test-token";
        _jsRuntime.NextResult = (string?)null;

        var token = await _service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.Null(_service.AccountEmail);
    }

    [Fact]
    public async Task TrySilentAuthAsync_FetchesEmail()
    {
        _jsRuntime.NextResult = "silent-token";
        _jsRuntime.NextResult = "user@example.com";

        var result = await _service.TrySilentAuthAsync();

        Assert.True(result);
        Assert.Equal("user@example.com", _service.AccountEmail);
    }

    [Fact]
    public async Task TrySilentAuthAsync_HandlesUserInfoFailure()
    {
        _jsRuntime.NextResult = "silent-token";
        _jsRuntime.NextResult = null;

        var result = await _service.TrySilentAuthAsync();

        Assert.True(result);
        Assert.Null(_service.AccountEmail);
    }

    [Fact]
    public async Task RestoreAccessTokenAsync_RestoresTokenAndMarksConnected()
    {
        var expiry = DateTime.UtcNow.AddMinutes(50);

        await _service.RestoreAccessTokenAsync("persisted-token", expiry);

        Assert.Equal("googleDrive.setAccessToken", _jsRuntime.LastMethod);
        Assert.Equal("persisted-token", _jsRuntime.LastArgs?[0]);
        Assert.Equal("persisted-token", _service.AccessToken);
        Assert.NotNull(_service.TokenExpiresAt);
        Assert.True(_service.IsConnected);
    }

    [Fact]
    public async Task RestoreAccessTokenAsync_NullToken_DoesNothing()
    {
        await _service.RestoreAccessTokenAsync(null, DateTime.UtcNow.AddMinutes(50));

        Assert.Null(_jsRuntime.LastMethod);
        Assert.Null(_service.AccessToken);
        Assert.False(_service.IsConnected);
    }

    private class TestJsRuntime : IJSRuntime
    {
        private int _callIndex;
        private readonly List<(string Method, object?[] Args)> _calls = new();
        private readonly List<object?> _results = new();
        private readonly List<Exception?> _exceptions = new();

        public string? LastMethod => _calls.Count > 0 ? _calls[^1].Method : null;
        public object?[]? LastArgs => _calls.Count > 0 ? _calls[^1].Args : null;

        public object? NextResult
        {
            set { _results.Add(value); }
        }

        public Exception? ThrowNext
        {
            set { _exceptions.Add(value); }
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            _calls.Add((identifier, args ?? Array.Empty<object?>()));

            if (_exceptions.Count > _callIndex)
            {
                _callIndex++;
                throw _exceptions[_callIndex - 1];
            }

            var result = _results.Count > _callIndex ? _results[_callIndex] : default;
            _callIndex++;

            return new ValueTask<TValue>((TValue)result!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken)
        {
            return InvokeAsync<TValue>(identifier, (object?[]?)null);
        }

        public void Dispose() { }
    }

    [Fact]
    public async Task GetAccessTokenAsync_InvokesJs()
    {
        _jsRuntime.NextResult = "access-token-123";

        var token = await _service.GetAccessTokenAsync();

        Assert.Equal("googleDrive.getAccessToken", _jsRuntime.LastMethod);
        Assert.Equal("access-token-123", token);
    }

    [Fact]
    public async Task ConnectAsync_NullUserInfo_SetsEmailNull()
    {
        _jsRuntime.NextResult = "test-token";
        _jsRuntime.NextResult = (string?)null;

        var token = await _service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.Null(_service.AccountEmail);
    }

    [Fact]
    public async Task ConnectAsync_FetchEmailFails_SetsEmailNull()
    {
        var js = new SequentialJsRuntime(
            ("test-token", null),
            (null, new JSException("Error fetching user info")));

        var logger = new Mock<ILogger<GoogleDriveService>>();
        var service = new GoogleDriveService(js, logger.Object);

        var token = await service.ConnectAsync();

        Assert.Equal("test-token", token);
        Assert.Null(service.AccountEmail);
        Assert.Equal(0, js.RemainingCalls);
    }

    [Fact]
    public async Task TrySilentAuthAsync_FetchEmailFails_SetsEmailNull()
    {
        var js = new SequentialJsRuntime(
            ("silent-token", null),
            (null, new JSException("Error fetching user info")));

        var logger = new Mock<ILogger<GoogleDriveService>>();
        var service = new GoogleDriveService(js, logger.Object);

        var result = await service.TrySilentAuthAsync();

        Assert.True(result);
        Assert.Null(service.AccountEmail);
        Assert.Equal(0, js.RemainingCalls);
    }

    private class SequentialJsRuntime : IJSRuntime
    {
        private readonly Queue<(object? Result, Exception? Error)> _queue = new();
        public int RemainingCalls => _queue.Count;

        public SequentialJsRuntime(params (object? Result, Exception? Error)[] calls)
        {
            foreach (var c in calls) _queue.Enqueue(c);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (_queue.Count == 0)
                throw new InvalidOperationException($"Unexpected JS invocation: {identifier}");
            var call = _queue.Dequeue();
            if (call.Error != null)
                throw call.Error;
            return new ValueTask<TValue>((TValue)call.Result!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken)
            => InvokeAsync<TValue>(identifier, (object?[]?)null);
    }
}