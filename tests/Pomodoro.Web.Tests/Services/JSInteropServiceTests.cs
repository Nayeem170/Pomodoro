using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Unit tests for JSInteropService
/// </summary>
[Trait("Category", "Service")]
public class JSInteropServiceTests
{
    [Fact]
    public void Constructor_WithValidJSRuntime_InitializesSuccessfully()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>().Object;

        // Act
        var service = new JSInteropService(jsRuntime);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullJSRuntime_InitializesSuccessfully()
    {
        // Arrange
        IJSRuntime jsRuntime = null!;

        // Act
        var service = new JSInteropService(jsRuntime);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ImplementsIJSInteropService()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>().Object;

        // Act
        var service = new JSInteropService(jsRuntime);

        // Assert
        service.Should().BeAssignableTo<IJSInteropService>();
    }

    [Fact]
    public async Task InvokeAsync_WithIdentifier_ReturnsValue()
    {
        // Arrange
        var identifier = "testFunction";
        var args = new object[] { "arg1", "arg2" };
        var expectedValue = "test result";
        
        // Create a mock JSRuntime that returns the expected value
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()))
            .Returns(new ValueTask<string>(expectedValue));

        var service = new JSInteropService(mockRuntime.Object);

        // Act
        var result = await service.InvokeAsync<string>(identifier, args);

        // Assert
        result.Should().Be(expectedValue);
        mockRuntime.Verify(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIdentifierAndArgs_ReturnsValue()
    {
        // Arrange
        var identifier = "testFunction";
        var args = new object[] { "arg1", "arg2" };
        var expectedValue = 42;
        
        // Create a mock JSRuntime that returns the expected value
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<int>(identifier, It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedValue));

        var service = new JSInteropService(mockRuntime.Object);

        // Act
        var result = await service.InvokeAsync<int>(identifier, args);

        // Assert
        result.Should().Be(expectedValue);
        mockRuntime.Verify(x => x.InvokeAsync<int>(identifier, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIdentifierAndArgs_ReturnsBool()
    {
        // Arrange
        var identifier = "testFunction";
        var args = new object[] { "arg1", "arg2" };
        var expectedValue = true;
        
        // Create a mock JSRuntime that returns the expected value
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<bool>(identifier, It.IsAny<object[]>()))
            .Returns(new ValueTask<bool>(expectedValue));

        var service = new JSInteropService(mockRuntime.Object);

        // Act
        var result = await service.InvokeAsync<bool>(identifier, args);

        // Assert
        result.Should().Be(expectedValue);
        mockRuntime.Verify(x => x.InvokeAsync<bool>(identifier, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyArgs_ReturnsValue()
    {
        // Arrange
        var identifier = "testFunction";
        var args = Array.Empty<object>();
        var expectedValue = "test result";
        
        // Create a mock JSRuntime that returns the expected value
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()))
            .Returns(new ValueTask<string>(expectedValue));

        var service = new JSInteropService(mockRuntime.Object);

        // Act
        var result = await service.InvokeAsync<string>(identifier, args);

        // Assert
        result.Should().Be(expectedValue);
        mockRuntime.Verify(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithComplexObjectArgs_ReturnsValue()
    {
        // Arrange
        var identifier = "testFunction";
        var args = new object[] { new { Name = "test" }, 123 };
        var expectedValue = "test result";
        
        // Create a mock JSRuntime that returns the expected value
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()))
            .Returns(new ValueTask<string>(expectedValue));

        var service = new JSInteropService(mockRuntime.Object);

        // Act
        var result = await service.InvokeAsync<string>(identifier, args);

        // Assert
        result.Should().Be(expectedValue);
        mockRuntime.Verify(x => x.InvokeAsync<string>(identifier, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task InvokeVoidAsync_WithIdentifier_CallsJSRuntime()
    {
        // Arrange
        var identifier = "testFunction";
        var args = new object[] { "arg1", "arg2" };
        
        // Create a mock JSRuntime that doesn't throw exceptions
        var mockRuntime = new Mock<IJSRuntime>();
        
        var service = new JSInteropService(mockRuntime.Object);

        // Act & Assert
        await service.InvokeVoidAsync(identifier, args);
        // The test passes if no exception is thrown
    }

    [Fact]
    public async Task InvokeVoidAsync_WithEmptyArgs_CallsJSRuntime()
    {
        // Arrange
        var identifier = "testFunction";
        var args = Array.Empty<object>();
        
        // Create a mock JSRuntime that doesn't throw exceptions
        var mockRuntime = new Mock<IJSRuntime>();
        
        var service = new JSInteropService(mockRuntime.Object);

        // Act & Assert
        await service.InvokeVoidAsync(identifier, args);
        // The test passes if no exception is thrown
    }
}
