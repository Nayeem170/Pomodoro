using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using System.Text.Json;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public partial class IndexedDbServiceTests
{
    public class GetAllAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task GetAllAsync_JsonException_FiresEventAndReturnsEmpty()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var element = JsonSerializer.SerializeToElement(123);
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.GetAllAsync<TestItem>("store");

            Assert.Empty(result);
            Assert.NotNull(errorMsg);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class QueryByIndexAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task QueryByIndexAsync_ReturnsEmptyWhenNull()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItemsByIndex,
                It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByIndexAsync<TestItem>("store", "index", "val");

            Assert.Empty(result);
        }

        [Fact]
        public async Task QueryByIndexAsync_JsonException_LogsWarningAndReturnsEmpty()
        {
            await SetupInitialization();

            var element = JsonSerializer.SerializeToElement(123);
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItemsByIndex,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.QueryByIndexAsync<TestItem>("store", "index", "val");

            Assert.Empty(result);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class QueryByDateRangeAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task QueryByDateRangeAsync_ReturnsEmptyWhenNull()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItemsByDateRange,
                It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Empty(result);
        }

        [Fact]
        public async Task QueryByDateRangeAsync_JsonException_LogsWarningAndReturnsEmpty()
        {
            await SetupInitialization();

            var element = JsonSerializer.SerializeToElement(123);
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItemsByDateRange,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Empty(result);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class PutAllAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task PutAllAsync_NullItems_ReturnsTrue()
        {
            await SetupInitialization();

            var result = await _service.PutAllAsync<TestItem>("store", null!);

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Constants.IndexedDbJsFunctions.PutAllItems,
                It.IsAny<object[]>()), Times.Never);
        }
    }

    public class GetAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task GetAsync_InvalidJsonType_LogsWarningAndReturnsDefault()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var element = JsonSerializer.SerializeToElement(123);
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItem,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.GetAsync<TestItem>("store", "1");

            Assert.Null(result);
            Assert.NotNull(errorMsg);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class GetAllAsyncCoverageTests2 : IndexedDbServiceTests
    {
        [Fact]
        public async Task GetAllAsync_SingleItemInsteadOfArray_LogsWarningAndReturnsEmpty()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var element = JsonSerializer.SerializeToElement("invalid");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.GetAllAsync<TestItem>("store");

            Assert.Empty(result);
            Assert.NotNull(errorMsg);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_InvalidJsonType_LogsWarningAndReturnsEmpty()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var element = JsonSerializer.SerializeToElement(123);
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.GetAllAsync<TestItem>("store");

            Assert.Empty(result);
            Assert.NotNull(errorMsg);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ObjectInsteadOfArray_LogsWarningAndReturnsEmpty()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var element = JsonSerializer.SerializeToElement(new { invalid = "structure" });
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems,
                It.IsAny<object[]>()))
                .ReturnsAsync(element);

            var result = await _service.GetAllAsync<TestItem>("store");

            Assert.Empty(result);
            Assert.NotNull(errorMsg);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class QueryByDateRangeAsyncCoverageTests2 : IndexedDbServiceTests
    {
        [Fact]
        public async Task QueryByDateRangeAsync_Exception_LogsDebugAndReturnsEmpty()
        {
            await SetupInitialization();

            var ex = new InvalidOperationException("Test error");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetItemsByDateRange,
                It.IsAny<object[]>()))
                .ThrowsAsync(ex);

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Empty(result);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class PutAllAsyncCoverageTests2 : IndexedDbServiceTests
    {
        [Fact]
        public async Task PutAllAsync_NullItems_ReturnsTrue()
        {
            await SetupInitialization();

            var result = await _service.PutAllAsync<TestItem>("store", null!);

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Constants.IndexedDbJsFunctions.PutAllItems,
                It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public async Task PutAllAsync_EmptyList_ReturnsTrue()
        {
            await SetupInitialization();

            var result = await _service.PutAllAsync<TestItem>("store", new List<TestItem>());

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Constants.IndexedDbJsFunctions.PutAllItems,
                It.IsAny<object[]>()), Times.Never);
        }
    }

    public class ClearAsyncCoverageTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task ClearAsync_Success_ReturnsTrue()
        {
            await SetupInitialization();

            _mockJsRuntime.Setup(x => x.InvokeAsync<ValueTask>(
                Constants.IndexedDbJsFunctions.ClearStore,
                It.IsAny<object[]>()))
                .ReturnsAsync(ValueTask.CompletedTask);

            var result = await _service.ClearAsync("store");

            Assert.True(result);
        }
    }
}
