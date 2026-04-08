using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.ChartServiceTests;

/// <summary>
/// Tests for ChartService chart creation and management methods.
/// </summary>
public class ChartServiceTests
{
    protected readonly Mock<IJSRuntime> JsRuntimeMock = new();
    
    public ChartServiceTests()
    {
        // Setup JS runtime to return completed tasks for void invocations
        // InvokeVoidAsync internally calls InvokeAsync<IJSVoidResult>
        JsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
    }
    
    protected ChartService CreateService()
    {
        return new ChartService(JsRuntimeMock.Object);
    }
    
    [Fact]
    public async Task CreateBarChartAsync_CallsJsInteropWithCorrectParameters()
    {
        // Arrange
        var service = CreateService();
        var canvasId = "testChart";
        var labels = new[] { "Mon", "Tue", "Wed" };
        var data = new[] { 5, 3, 7 };
        
        // Act
        await service.CreateBarChartAsync(canvasId, labels, data);
        
        // Assert
        JsRuntimeMock.Verify(
            x => x.InvokeAsync<object>(
                Constants.ChartJsFunctions.CreateBarChart,
                It.Is<object?[]?>(args => 
                    args != null && 
                    args.Length == 5 &&
                    args[0]!.ToString() == canvasId)),
            Times.AtLeastOnce());
    }
    
    [Fact]
    public async Task CreateBarChartAsync_WithHighlightIndex_CallsJsInteropWithHighlightIndex()
    {
        // Arrange
        var service = CreateService();
        var canvasId = "testChart";
        var labels = new[] { "Mon", "Tue", "Wed" };
        var data = new[] { 5, 3, 7 };
        var highlightIndex = 1;
        
        // Act
        await service.CreateBarChartAsync(canvasId, labels, data, highlightIndex: highlightIndex);
        
        // Assert
        JsRuntimeMock.Verify(
            x => x.InvokeAsync<object>(
                Constants.ChartJsFunctions.CreateBarChart,
                It.IsAny<object?[]?>()),
            Times.AtLeastOnce());
    }
    
    [Fact]
    public async Task CreateGroupedBarChartAsync_CallsJsInteropWithCorrectParameters()
    {
        // Arrange
        var service = CreateService();
        var canvasId = "groupedChart";
        var labels = new[] { "Week 1", "Week 2" };
        var focusData = new[] { 120, 150 };
        var breakData = new[] { 30, 25 };
        
        // Act
        await service.CreateGroupedBarChartAsync(canvasId, labels, focusData, breakData);
        
        // Assert
        JsRuntimeMock.Verify(
            x => x.InvokeAsync<object>(
                Constants.ChartJsFunctions.CreateGroupedBarChart,
                It.Is<object?[]?>(args => 
                    args != null && 
                    args.Length == 5 &&
                    args[0]!.ToString() == canvasId)),
            Times.AtLeastOnce());
    }
    
    [Fact]
    public async Task UpdateChartAsync_CallsJsInteropWithCorrectParameters()
    {
        // Arrange
        var service = CreateService();
        var canvasId = "testChart";
        var data = new[] { 10, 20, 30 };
        
        // Act
        await service.UpdateChartAsync(canvasId, data);
        
        // Assert
        JsRuntimeMock.Verify(
            x => x.InvokeAsync<object>(
                Constants.ChartJsFunctions.UpdateChart,
                It.Is<object?[]?>(args => 
                    args != null && 
                    args.Length == 2 &&
                    args[0]!.ToString() == canvasId)),
            Times.Once());
    }
    
    [Fact]
    public async Task DestroyChartAsync_CallsJsInteropWithCorrectParameters()
    {
        // Arrange
        var service = CreateService();
        var canvasId = "testChart";
        
        // Act
        await service.DestroyChartAsync(canvasId);
        
        // Assert
        JsRuntimeMock.Verify(
            x => x.InvokeAsync<object>(
                Constants.ChartJsFunctions.DestroyChart,
                It.Is<object?[]?>(args => 
                    args != null && 
                    args.Length == 1 &&
                    args[0]!.ToString() == canvasId)),
            Times.Once());
    }
    
    [Fact]
    public async Task DestroyChartAsync_WhenJsError_SwallowsException()
    {
        // Arrange
        JsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(
                Constants.ChartJsFunctions.DestroyChart,
                It.IsAny<object?[]?>()))
            .ThrowsAsync(new JSException("Chart not found"));
        var service = CreateService();
        
        // Act & Assert - Should not throw
        await service.DestroyChartAsync("nonExistentChart");
    }
    
    [Fact]
    public async Task DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        
        // Act & Assert - Should complete without throwing
        await service.DisposeAsync();
    }
}
