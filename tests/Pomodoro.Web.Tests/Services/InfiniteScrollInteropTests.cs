using System;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Xunit;
using FluentAssertions;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services
{
    public class InfiniteScrollInteropTests
    {
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly InfiniteScrollInterop _infiniteScrollInterop;

        public InfiniteScrollInteropTests()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();
            _infiniteScrollInterop = new InfiniteScrollInterop(_mockJsRuntime.Object);
        }

        [Fact]
        public void Constructor_WithValidJSRuntime_InitializesSuccessfully()
        {
            // Arrange & Act
            var service = new InfiniteScrollInterop(_mockJsRuntime.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullJSRuntime_InitializesSuccessfully()
        {
            // Arrange & Act
            var service = new InfiniteScrollInterop(null!);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Service_ShouldImplementIInfiniteScrollInterop()
        {
            // Assert
            _infiniteScrollInterop.Should().BeAssignableTo<IInfiniteScrollInterop>();
        }

        [Fact]
        public async Task IsSupportedAsync_WhenJSRuntimeReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.isSupported", It.IsAny<object[]>()))
                     .ReturnsAsync(true);

            // Act
            var result = await _infiniteScrollInterop.IsSupportedAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSupportedAsync_WhenJSRuntimeReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.isSupported", It.IsAny<object[]>()))
                     .ReturnsAsync(false);

            // Act
            var result = await _infiniteScrollInterop.IsSupportedAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSupportedAsync_WhenJSRuntimeThrowsJSException_ReturnsFalse()
        {
            // Arrange
            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.isSupported", It.IsAny<object[]>()))
                     .ThrowsAsync(new JSException("Test exception"));

            // Act
            var result = await _infiniteScrollInterop.IsSupportedAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateObserverAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var sentinelId = "sentinel1";
            var dotNetRef = DotNetObjectReference.Create(new object());
            var containerId = "container1";
            var rootMargin = "0px";
            var timeoutMs = 1000;

            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.createObserver", It.IsAny<object[]>()))
                 .ReturnsAsync(true);

            // Act
            var result = await _infiniteScrollInterop.CreateObserverAsync(
                sentinelId, dotNetRef, containerId, rootMargin, timeoutMs);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CreateObserverAsync_WhenJSRuntimeReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var sentinelId = "sentinel1";
            var dotNetRef = DotNetObjectReference.Create(new object());
            var containerId = "container1";
            var rootMargin = "0px";
            var timeoutMs = 1000;

            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.createObserver", It.IsAny<object[]>()))
                 .ReturnsAsync(false);

            // Act
            var result = await _infiniteScrollInterop.CreateObserverAsync(
                sentinelId, dotNetRef, containerId, rootMargin, timeoutMs);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateObserverAsync_WhenJSRuntimeThrowsJSException_ReturnsFalse()
        {
            // Arrange
            var sentinelId = "sentinel1";
            var dotNetRef = DotNetObjectReference.Create(new object());
            var containerId = "container1";
            var rootMargin = "0px";
            var timeoutMs = 1000;

            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.createObserver", It.IsAny<object[]>()))
                 .ThrowsAsync(new JSException("Test exception"));

            // Act
            var result = await _infiniteScrollInterop.CreateObserverAsync(
                sentinelId, dotNetRef, containerId, rootMargin, timeoutMs);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateObserverAsync_WithNullParameters_CallsJSRuntimeCorrectly()
        {
            // Arrange
            var sentinelId = (string?)null;
            var dotNetRef = (DotNetObjectReference<object>?)null;
            var containerId = (string?)null;
            var rootMargin = (string?)null;
            var timeoutMs = 0;

            _mockJsRuntime.Setup(x => x.InvokeAsync<bool>("infiniteScroll.createObserver", It.IsAny<object[]>()))
                 .ReturnsAsync(true);

            // Act
            var result = await _infiniteScrollInterop.CreateObserverAsync(
                sentinelId!, dotNetRef!, containerId!, rootMargin!, timeoutMs);

            // Assert
            result.Should().BeTrue();
            _mockJsRuntime.Verify(x => x.InvokeAsync<bool>("infiniteScroll.createObserver", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DestroyObserverAsync_WithValidId_DoesNotThrow()
        {
            // Arrange
            var sentinelId = "sentinel1";

            // Act & Assert - Should not throw
            await _infiniteScrollInterop.DestroyObserverAsync(sentinelId);
        }

        [Fact]
        public async Task DestroyObserverAsync_WithNullId_DoesNotThrow()
        {
            // Arrange
            var sentinelId = (string?)null;

            // Act & Assert - Should not throw
            await _infiniteScrollInterop.DestroyObserverAsync(sentinelId!);
        }

        [Fact]
        public async Task DestroyObserverAsync_WhenJSRuntimeThrowsJSException_DoesNotThrow()
        {
            _mockJsRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>("infiniteScroll.destroyObserver", It.IsAny<object[]>()))
                         .ThrowsAsync(new JSException("Test exception"));

            await _infiniteScrollInterop.DestroyObserverAsync("sentinel1");
        }

        [Fact]
        public async Task DestroyAllObserversAsync_WhenJSRuntimeThrowsJSException_DoesNotThrow()
        {
            var throwingRuntime = new ThrowingJSRuntime();
            var interop = new InfiniteScrollInterop(throwingRuntime);

            await interop.DestroyAllObserversAsync();
        }

        [Fact]
        public async Task DestroyAllObserversAsync_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            await _infiniteScrollInterop.DestroyAllObserversAsync();
        }
    }

    internal class ThrowingJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default, object?[]? args = null)
            => throw new JSException("Test exception");

        [Obsolete]
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => throw new JSException("Test exception");
    }
}