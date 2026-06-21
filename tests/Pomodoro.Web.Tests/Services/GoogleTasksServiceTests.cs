using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class GoogleTasksServiceTests
{
    private readonly TestJsRuntime _jsRuntime;
    private readonly Mock<IGoogleDriveService> _googleDriveServiceMock;
    private readonly Mock<ILogger<GoogleTasksService>> _loggerMock;
    private readonly GoogleTasksService _service;

    public GoogleTasksServiceTests()
    {
        _jsRuntime = new TestJsRuntime();
        _googleDriveServiceMock = new Mock<IGoogleDriveService>();
        _loggerMock = new Mock<ILogger<GoogleTasksService>>();
        _service = new GoogleTasksService(_jsRuntime, _googleDriveServiceMock.Object, _loggerMock.Object);
        _googleDriveServiceMock.Setup(x => x.IsConnected).Returns(true);
        _googleDriveServiceMock.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("test-token");
    }

    [Fact]
    public async Task GetTaskListsAsync_ReturnsParsedLists()
    {
        var listData = new { items = new[] { new { id = "list-1", title = "Personal" }, new { id = "list-2", title = "Work" } } };
        var responseJson = JsonSerializer.Serialize(listData);
        _jsRuntime.QueueResult(responseJson);

        var lists = await _service.GetTaskListsAsync();

        Assert.Equal(2, lists.Count);
        Assert.Equal("list-1", lists[0].Id);
        Assert.Equal("Personal", lists[0].Title);
        Assert.Equal("list-2", lists[1].Id);
        Assert.Equal("Work", lists[1].Title);
    }

    [Fact]
    public async Task GetTaskListsAsync_ReturnsEmpty_WhenNoItems()
    {
        var responseJson = JsonSerializer.Serialize(new { items = Array.Empty<object>() });
        _jsRuntime.QueueResult(responseJson);

        var lists = await _service.GetTaskListsAsync();

        Assert.NotNull(lists);
        Assert.Empty(lists);
    }

    [Fact]
    public async Task GetTaskListsAsync_ThrowsUnauthorized_On401()
    {
        _jsRuntime.QueueException(new JSException("Error 401: Unauthorized"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTaskListsAsync());
    }

    [Fact]
    public async Task GetTaskListsAsync_ThrowsForbidden_On403()
    {
        _jsRuntime.QueueException(new JSException("Error 403: Forbidden"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTaskListsAsync());
        Assert.Contains("access forbidden", ex.Message);
    }

    [Fact]
    public async Task GetTaskListsAsync_Throws_WhenNoToken()
    {
        _googleDriveServiceMock.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTaskListsAsync());
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsParsedTasks()
    {
        var responseJson = JsonSerializer.Serialize(new object[]
        {
            new { id = "task-1", title = "Buy milk", notes = (string?)null, status = "needsAction", updated = "2026-01-01T00:00:00.000Z", etag = "etag1" },
            new { id = "task-2", title = "Write tests", notes = "important", status = "completed", updated = "2026-01-02T00:00:00.000Z", etag = "etag2", due = "2026-06-21T00:00:00.000Z" }
        });
        _jsRuntime.QueueResult(responseJson);

        var tasks = await _service.GetTasksAsync("list-1");

        Assert.Equal(2, tasks.Count);
        Assert.Equal("task-1", tasks[0].Id);
        Assert.Equal("Buy milk", tasks[0].Title);
        Assert.Null(tasks[0].Notes);
        Assert.Equal("needsAction", tasks[0].Status);
        Assert.Equal("task-2", tasks[1].Id);
        Assert.Equal("Write tests", tasks[1].Title);
        Assert.Equal("important", tasks[1].Notes);
        Assert.Equal("2026-06-21T00:00:00.000Z", tasks[1].Due);
        Assert.Equal("completed", tasks[1].Status);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsEmpty_WhenNoItems()
    {
        var responseJson = JsonSerializer.Serialize(Array.Empty<object>());
        _jsRuntime.QueueResult(responseJson);

        var tasks = await _service.GetTasksAsync("list-1");

        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task GetTasksAsync_ThrowsUnauthorized_On401()
    {
        _jsRuntime.QueueException(new JSException("Error 401: Unauthorized"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTasksAsync("list-1"));
    }

    [Fact]
    public async Task GetTasksAsync_Retries_On429()
    {
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));
        var responseJson = JsonSerializer.Serialize(new[] { new { id = "t1", title = "Retry", status = "needsAction", updated = "2026-01-01T00:00:00.000Z" } });
        _jsRuntime.QueueResult(responseJson);

        var tasks = await _service.GetTasksAsync("list-1");

        Assert.Single(tasks);
        Assert.Equal("Retry", tasks[0].Title);
    }

    [Fact]
    public async Task GetTasksAsync_PassesUpdatedMin()
    {
        var responseJson = JsonSerializer.Serialize(Array.Empty<object>());
        _jsRuntime.QueueResult(responseJson);

        await _service.GetTasksAsync("list-1", "2026-01-01T00:00:00.000Z");

        Assert.Equal("googleTasks.listTasks", _jsRuntime.LastMethod);
    }

    [Fact]
    public async Task IsConnectedAsync_ReturnsGoogleDriveConnectionState()
    {
        _googleDriveServiceMock.Setup(x => x.IsConnected).Returns(true);
        Assert.True(await _service.IsConnectedAsync());

        _googleDriveServiceMock.Setup(x => x.IsConnected).Returns(false);
        Assert.False(await _service.IsConnectedAsync());
    }

    [Fact]
    public async Task GetTaskListsAsync_RetriesUpTo3Times_On429()
    {
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTaskListsAsync());
    }

    [Fact]
    public async Task GetTaskListsAsync_RateLimitMessage_ContainsRetriesInfo()
    {
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));
        _jsRuntime.QueueException(new JSException("Error 429: Too Many Requests"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetTaskListsAsync());
        Assert.Contains("rate-limited", ex.Message);
    }

    [Fact]
    public async Task GetTasksAsync_ThrowsGeneric_OnUnexpectedError()
    {
        _jsRuntime.QueueException(new JSException("Error 500: Internal Server Error"));

        await Assert.ThrowsAsync<JSException>(() => _service.GetTasksAsync("list-1"));
    }

    [Fact]
    public async Task GetTasksAsync_ParsesParentAndPosition()
    {
        var taskData = new object[]
        {
            new { id = "task-1", title = "Task with parent", status = "needsAction", updated = "2025-06-20T10:00:00Z", parent = "parent-task-id", position = "00000001" }
        };
        _jsRuntime.QueueResult(JsonSerializer.Serialize(taskData));

        var tasks = await _service.GetTasksAsync("list-1");

        Assert.Single(tasks);
        Assert.Equal("parent-task-id", tasks[0].Parent);
        Assert.Equal("00000001", tasks[0].Position);
    }

    private class TestJsRuntime : IJSRuntime
    {
        private int _callIndex;
        private readonly List<(object? Result, Exception? Exception)> _queue = new();

        public string? LastMethod { get; private set; }
        public object?[]? LastArgs { get; private set; }

        public void QueueResult(object? result) => _queue.Add((result, null));
        public void QueueException(Exception ex) => _queue.Add((null, ex));

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            LastMethod = identifier;
            LastArgs = args;

            if (_callIndex < _queue.Count)
            {
                var entry = _queue[_callIndex];
                _callIndex++;
                if (entry.Exception != null)
                    throw entry.Exception;
                return new ValueTask<TValue>((TValue)entry.Result!);
            }

            _callIndex++;
            return default(ValueTask<TValue>);
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
}
