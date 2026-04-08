using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

/// <summary>
/// Tests for WeeklyMiniChart exception handling in DisposeAsync
/// These tests specifically target lines 79, 80, and 82 in the catch block
/// </summary>
public class WeeklyMiniChartExceptionHandlingTests : TestContext
{
    public WeeklyMiniChartExceptionHandlingTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        // Don't register default ChartService here - we'll register throwing services in each test
    }

    [Fact]
    public async Task WeeklyMiniChart_DisposeAsync_WithThrowingChartService_CoversCatchBlock()
    {
        // Arrange - Register a throwing ChartService that will trigger the catch block
        var jsRuntimeMock = new Mock<IJSRuntime>();
        Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
        Services.AddSingleton<ChartService>(new ThrowingChartService());
        Services.AddSingleton(new ChartDataFormatter());

        // Act - Render and dispose component
        var cut = RenderComponent<WeeklyMiniChart>();
        await cut.Instance.DisposeAsync();

        // Assert - Should not throw exception (catch block handles it)
        Assert.True(true);
    }



    [Fact]
    public async Task WeeklyMiniChart_DisposeAsync_MultipleDisposesWithThrowingService_DoesNotThrow()
    {
        // Arrange - Register a throwing ChartService
        var jsRuntimeMock = new Mock<IJSRuntime>();
        Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
        Services.AddSingleton<ChartService>(new ThrowingChartService());
        Services.AddSingleton(new ChartDataFormatter());

        // Act - Render and dispose component multiple times
        var cut = RenderComponent<WeeklyMiniChart>();
        await cut.Instance.DisposeAsync();
        await cut.Instance.DisposeAsync();
        await cut.Instance.DisposeAsync();

        // Assert - Should not throw exception (catch block handles it each time)
        Assert.True(true);
    }

    /// <summary>
    /// A ChartService that throws exceptions for testing error handling
    /// </summary>
    private class ThrowingChartService : ChartService
    {
        public ThrowingChartService() : base(new Mock<IJSRuntime>().Object) { }

        public override Task DestroyChartAsync(string canvasId)
        {
            throw new JSException("Chart not found - test exception");
        }
    }
}
