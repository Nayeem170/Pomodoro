using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using System.Text.Json;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Test cases for IndexedDbService.GetAllAsync() method.
/// Verifies retrieval of all items from IndexedDB stores.
/// </summary>
[Trait("Category", "Service")]
public partial class IndexedDbServiceTests
{
    [Trait("Category", "Service")]
    public class GetAllAsyncTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task GetAllAsync_NotInitialized_Throws()
        {
            // Arrange
            var service = new IndexedDbService(_mockJsRuntime.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.GetAllAsync<TestItem>("testStore"));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyListWhenNull()
        {
            // Arrange
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("null");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems, 
                It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            // Act
            var result = await _service.GetAllAsync<TestItem>("testStore");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsDeserializedList()
        {
            // Arrange
            await SetupInitialization();
            var jsonDoc = JsonDocument.Parse("[{\"Id\":\"1\",\"Name\":\"Test1\"},{\"Id\":\"2\",\"Name\":\"Test2\"}]");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems, 
                It.IsAny<object[]>()))
                .ReturnsAsync(jsonDoc.RootElement);

            // Act
            var result = await _service.GetAllAsync<TestItem>("testStore");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Test1", result[0].Name);
            Assert.Equal("Test2", result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenException_LogsDebugAndReturnsEmpty()
        {
            // Arrange
            await SetupInitialization();
            var ex = new InvalidOperationException("Test error");
            _mockJsRuntime.Setup(x => x.InvokeAsync<JsonElement>(
                Constants.IndexedDbJsFunctions.GetAllItems, 
                It.IsAny<object[]>()))
                .ThrowsAsync(ex);

            // Act
            var result = await _service.GetAllAsync<TestItem>("testStore");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}

