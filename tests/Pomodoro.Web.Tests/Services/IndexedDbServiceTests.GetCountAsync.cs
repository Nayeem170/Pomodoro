using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Test cases for IndexedDbService.GetCountAsync() method.
/// Verifies counting items in IndexedDB stores.
/// </summary>
public partial class IndexedDbServiceTests
{
    public class GetCountAsyncTests : IndexedDbServiceTests
    {
        [Fact]
        public async Task GetCountAsync_NotInitialized_Throws()
        {
            // Arrange
            var service = new IndexedDbService(_mockJsRuntime.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.GetCountAsync("testStore"));
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            await SetupInitialization();
            _mockJsRuntime.Setup(x => x.InvokeAsync<int>(
                Constants.IndexedDbJsFunctions.GetCount, 
                It.IsAny<object[]>()))
                .ReturnsAsync(42);

            // Act
            var result = await _service.GetCountAsync("testStore");

            // Assert
            Assert.Equal(42, result);
            _mockJsRuntime.Verify(x => x.InvokeAsync<int>(
                Constants.IndexedDbJsFunctions.GetCount, 
                It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task GetCountAsync_WhenException_LogsDebugAndReturnsZero()
        {
            // Arrange
            await SetupInitialization();
            var ex = new InvalidOperationException("Test error");
            _mockJsRuntime.Setup(x => x.InvokeAsync<int>(
                Constants.IndexedDbJsFunctions.GetCount, 
                It.IsAny<object[]>()))
                .ThrowsAsync(ex);

            // Act
            var result = await _service.GetCountAsync("testStore");

            // Assert
            Assert.Equal(0, result);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
