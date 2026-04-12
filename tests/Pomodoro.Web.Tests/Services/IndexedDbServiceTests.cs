using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using System.Text.Json;
using Xunit;

namespace Pomodoro.Web.Tests.Services
{
    public partial class IndexedDbServiceTests
    {
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly Mock<ILogger<IndexedDbService>> _mockLogger;
        private readonly IndexedDbService _service;

        public IndexedDbServiceTests()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();
            _mockLogger = new Mock<ILogger<IndexedDbService>>();
            _service = new IndexedDbService(_mockJsRuntime.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InitializeAsync_CallsInitDatabase()
        {
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            await _service.InitializeAsync();

            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_WhenAlreadyInitialized_DoesNotCallInitDatabaseAgain()
        {
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            await _service.InitializeAsync();
            await _service.InitializeAsync();

            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_WhenExceptionThrown_ThrowsAndLogs()
        {
            var ex = new InvalidOperationException("Test");
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()))
                .ThrowsAsync(ex);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InitializeAsync());
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), ex, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task InitializeJsConstantsAsync_CallsPomodoroConstantsInitialize()
        {
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PomodoroConstantsInitialize, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            await _service.InitializeJsConstantsAsync(25, 5, 15);

            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PomodoroConstantsInitialize, It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InitializeJsConstantsAsync_WhenException_LogsWarning()
        {
            var ex = new InvalidOperationException("Test");
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PomodoroConstantsInitialize, It.IsAny<object[]>()))
                .ThrowsAsync(ex);

            await _service.InitializeJsConstantsAsync(25, 5, 15);

            _mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), ex, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_NotInitialized_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAsync<TestItem>("store", "1"));
        }

        [Fact]
        public async Task GetAsync_ReturnsDeserializedItem()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("{\"Id\":\"1\",\"Name\":\"Test\"}");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItem, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.GetAsync<TestItem>("store", "1");

            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task GetAsync_ReturnsNullWhenUndef()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItem, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.GetAsync<TestItem>("store", "1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_JsonException_FiresEventAndReturnsDefault()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            var jsonDoc = JsonDocument.Parse("{\"Id\":123}");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItem, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.GetAsync<TestItem>("store", "1");

            Assert.Null(result);
            Assert.NotNull(errorMsg);
        }

        [Fact]
        public async Task GetAsync_GeneralException_FiresEventAndReturnsDefault()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItem, It.IsAny<object[]>()))
                .ThrowsAsync(new InvalidOperationException("Network Error"));

            var result = await _service.GetAsync<TestItem>("store", "1");

            Assert.Null(result);
            Assert.NotNull(errorMsg);
        }

        [Fact]
        public async Task GetAllAsync_Exception_ReturnsEmptyAndEvents()
        {
            await SetupInitialization();
            string? errorMsg = null;
            _service.OnStorageError += msg => errorMsg = msg;

            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetAllItems, It.IsAny<object[]>()))
                .ThrowsAsync(new InvalidOperationException("Error"));

            var result = await _service.GetAllAsync<TestItem>("store");

            Assert.Empty(result);
            Assert.NotNull(errorMsg);
        }
        [Fact]
        public async Task QueryByIndexAsync_ReturnsList()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("[{\"Id\":\"1\"}]");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByIndex, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByIndexAsync<TestItem>("store", "index", "val");

            Assert.Single(result);
        }

        [Fact]
        public async Task QueryByIndexAsync_Exception_ReturnsEmpty()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByIndex, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.QueryByIndexAsync<TestItem>("store", "index", "val");

            Assert.Empty(result);
        }

        [Fact]
        public async Task QueryByIndexAsync_ReturnsEmptyWhenUndefined()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByIndex, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByIndexAsync<TestItem>("store", "index", "val");

            Assert.Empty(result);
        }

        [Fact]
        public async Task QueryByDateRangeAsync_ReturnsList()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("[{\"Id\":\"1\"}]");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByDateRange, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Single(result);
        }

        [Fact]
        public async Task QueryByDateRangeAsync_Exception_ReturnsEmpty()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByDateRange, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Empty(result);
        }

        [Fact]
        public async Task QueryByDateRangeAsync_ReturnsEmptyWhenUndefined()
        {
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByDateRange, It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            var result = await _service.QueryByDateRangeAsync<TestItem>("store", "index", "start", "end");

            Assert.Empty(result);
        }

        [Fact]
        public async Task PutAsync_ReturnsTrue()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutItem, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            var result = await _service.PutAsync("store", new TestItem());

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutItem, It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PutAsync_Exception_ReturnsFalse()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutItem, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.PutAsync("store", new TestItem());

            Assert.False(result);
        }

        [Fact]
        public async Task PutAllAsync_ReturnsTrue()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutAllItems, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            var result = await _service.PutAllAsync("store", new List<TestItem> { new TestItem() });

            Assert.True(result);
        }

        [Fact]
        public async Task PutAllAsync_EmptyReturnsTrue()
        {
            await SetupInitialization();

            var result = await _service.PutAllAsync("store", new List<TestItem>());

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutAllItems, It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public async Task PutAllAsync_NullReturnsTrue()
        {
            await SetupInitialization();

            var result = await _service.PutAllAsync<TestItem>("store", null!);

            Assert.True(result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutAllItems, It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public async Task PutAllAsync_Exception_ReturnsFalse()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.PutAllItems, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.PutAllAsync("store", new List<TestItem> { new TestItem() });

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.DeleteItem, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);
            var result = await _service.DeleteAsync("store", "1");
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_Exception_ReturnsFalse()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.DeleteItem, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.DeleteAsync("store", "1");
            Assert.False(result);
        }

        [Fact]
        public async Task ClearAsync_ReturnsTrue()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.ClearStore, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);

            var result = await _service.ClearAsync("store");
            Assert.True(result);
        }

        [Fact]
        public async Task ClearAsync_Exception_ReturnsFalse()
        {
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.ClearStore, It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("Err"));

            var result = await _service.ClearAsync("store");
            Assert.False(result);
        }

        [Fact]
        public async Task DisposeAsync_Completes()
        {
            await _service.DisposeAsync();
        }

        private async Task SetupInitialization()
        {
            _mockJsRuntime.Setup(x => x.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(Constants.IndexedDbJsFunctions.InitDatabase, It.IsAny<object[]>()))
                .ReturnsAsync((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null!);
            await _service.InitializeAsync();
        }

        private class TestItem
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        [Fact]
        public async Task GetAllAsync_WhenJsonIsNull_ReturnsEmptyList()
        {
            await SetupInitialization();

            var nullJsonElement = JsonDocument.Parse("null").RootElement;

            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetAllItems, It.IsAny<object[]>()))
                .ReturnsAsync(nullJsonElement);

            var result = await _service.GetAllAsync<TestItem>("test-store");
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void DeserializeList_WhenDeserializeReturnsNull_ReturnsEmptyList()
        {
            var method = typeof(IndexedDbService).GetMethod("DeserializeList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var genericMethod = method.MakeGenericMethod(typeof(TestItem));
            var nullElement = JsonDocument.Parse("null").RootElement;
            var result = (System.Collections.Generic.List<TestItem>?)genericMethod.Invoke(_service, new object[] { nullElement, "store", "key" });
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}