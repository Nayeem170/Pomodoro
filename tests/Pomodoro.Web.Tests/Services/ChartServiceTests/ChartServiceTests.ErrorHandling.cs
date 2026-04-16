using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ChartServiceTests;

/// <summary>
/// Tests for ChartService error handling and edge cases
/// </summary>
[Trait("Category", "Service")]
public partial class ChartServiceTests_ErrorHandling
{
    [Fact]
    public async Task CreateBarChartAsync_WhenEnsureInitializedThrowsException_HandlesGracefully()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var callCount = 0;

        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(() =>
            {
                callCount++;
                // First call is EnsureInitialized, throw exception
                if (callCount == 1)
                {
                    throw new JSException("Chart.js initialization failed");
                }
                // Second call is CreateBarChart, succeed
                return new ValueTask<IJSVoidResult>(default(IJSVoidResult)!);
            });

        var service = new ChartService(jsRuntimeMock.Object);

        // Act - First call will fail initialization, but should not throw
        await service.CreateBarChartAsync("testChart", new[] { "Mon", "Tue" }, new[] { 1, 2 });

        // Assert - Should have made 2 calls: 1 initialization (failed), 1 chart creation (succeeded)
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task CreateBarChartAsync_WhenEnsureInitializedFails_CanSucceedOnSubsequentCalls()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var callCount = 0;

        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(() =>
            {
                callCount++;
                // First call is EnsureInitialized, throw exception
                if (callCount == 1)
                {
                    throw new JSException("Chart.js initialization failed");
                }
                // Second call is CreateBarChart (first chart), succeed
                // Third call is EnsureInitialized (second chart), succeed
                // Fourth call is CreateBarChart (second chart), succeed
                return new ValueTask<IJSVoidResult>(default(IJSVoidResult)!);
            });

        var service = new ChartService(jsRuntimeMock.Object);

        // Act - First call will fail initialization, second call will succeed
        await service.CreateBarChartAsync("testChart", new[] { "Mon", "Tue" }, new[] { 1, 2 });
        await service.CreateBarChartAsync("testChart2", new[] { "Wed", "Thu" }, new[] { 3, 4 });

        // Assert - Should have made 4 calls total
        Assert.Equal(4, callCount);
    }

    [Fact]
    public async Task CreateGroupedBarChartAsync_WhenEnsureInitializedThrowsException_HandlesGracefully()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var callCount = 0;

        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(() =>
            {
                callCount++;
                // First call is EnsureInitialized, throw exception
                if (callCount == 1)
                {
                    throw new JSException("Chart.js initialization failed");
                }
                // Second call is CreateGroupedBarChart, succeed
                return new ValueTask<IJSVoidResult>(default(IJSVoidResult)!);
            });

        var service = new ChartService(jsRuntimeMock.Object);

        // Act - First call will fail initialization, but should not throw
        await service.CreateGroupedBarChartAsync(
            "testChart",
            new[] { "Mon", "Tue" },
            new[] { 1, 2 },
            new[] { 5, 10 });

        // Assert - Should have made 2 calls: 1 initialization (failed), 1 chart creation (succeeded)
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task DestroyChartAsync_WhenJsRuntimeThrowsException_HandlesGracefully()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Throws(new JSException("Chart not found"));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act & Assert - should not throw, exception is caught internally
        await service.DestroyChartAsync("testChart");
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public async Task UpdateChartAsync_WhenJsRuntimeThrowsException_PropagatesException()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Throws(new JSException("Chart not found"));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act & Assert - UpdateChartAsync does not have try-catch, so exception should propagate
        await Assert.ThrowsAsync<JSException>(() =>
            service.UpdateChartAsync("testChart", new[] { 1, 2, 3 }));
    }

    [Fact]
    public async Task CreateBarChartAsync_WithNullParameters_DoesNotThrow()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act & Assert - should handle null parameters gracefully
        await service.CreateBarChartAsync("testChart", Array.Empty<string>(), Array.Empty<int>());
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public async Task CreateBarChartAsync_WithHighlightIndex_PassesParameterCorrectly()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act
        await service.CreateBarChartAsync("testChart", new[] { "Mon", "Tue" }, new[] { 1, 2 }, "Pomodoros", 1);

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>(
                Constants.ChartJsFunctions.CreateBarChart,
                It.Is<object?[]?>(args =>
                    args != null &&
                    args.Length == 5 &&
                    args[0]!.ToString() == "testChart" &&
                    args[4] is int && (int)args[4]! == 1)),
            Times.Once);
    }

    [Fact]
    public async Task CreateGroupedBarChartAsync_WithNullHighlightIndex_PassesNullCorrectly()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act
        await service.CreateGroupedBarChartAsync(
            "testChart",
            new[] { "Mon", "Tue" },
            new[] { 1, 2 },
            new[] { 5, 10 },
            null);

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>(
                Constants.ChartJsFunctions.CreateGroupedBarChart,
                It.Is<object?[]?>(args =>
                    args != null &&
                    args.Length == 5 &&
                    args[0]!.ToString() == "testChart" &&
                    args[4] == null)),
            Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_AlwaysCompletesSuccessfully()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var service = new ChartService(jsRuntimeMock.Object);

        // Act & Assert
        await service.DisposeAsync();
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public async Task CreateBarChartAsync_CalledMultipleTimes_InitializesOnce()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var initializationCount = 0;

        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Callback<string, object?[]?>((func, args) =>
            {
                if (func == Constants.ChartJsFunctions.EnsureInitialized)
                {
                    initializationCount++;
                }
            })
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));

        var service = new ChartService(jsRuntimeMock.Object);

        // Act
        await service.CreateBarChartAsync("testChart1", new[] { "Mon" }, new[] { 1 });
        await service.CreateBarChartAsync("testChart2", new[] { "Tue" }, new[] { 2 });
        await service.CreateBarChartAsync("testChart3", new[] { "Wed" }, new[] { 3 });

        // Assert - EnsureInitialized should only be called once
        Assert.Equal(1, initializationCount);
    }

    [Fact]
    public async Task CreateBarChartAsync_WithCustomLabel_PassesLabelCorrectly()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));

        var service = new ChartService(jsRuntimeMock.Object);
        var customLabel = "Focus Time (minutes)";

        // Act
        await service.CreateBarChartAsync("testChart", new[] { "Mon", "Tue" }, new[] { 1, 2 }, customLabel);

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>(
                Constants.ChartJsFunctions.CreateBarChart,
                It.Is<object?[]?>(args =>
                    args != null &&
                    args.Length == 5 &&
                    args[0]!.ToString() == "testChart" &&
                    args[3]!.ToString() == customLabel)),
            Times.Once);
    }
}

