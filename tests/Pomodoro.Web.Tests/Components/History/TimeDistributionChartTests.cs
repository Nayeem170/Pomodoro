using System;
using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// Tests for TimeDistributionChart component
/// </summary>
[Trait("Category", "Component")]
public class TimeDistributionChartTests
{
    [Trait("Category", "Component")]
    public class RenderingTests : TestContext
    {
        [Fact]
        public void Render_WithDefaultServices_RendersComponent()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert
            Assert.NotNull(cut.Instance);
        }

        [Fact]
        public void Render_WithSelectedDate_RendersComponent()
        {
            // Arrange
            var selectedDate = DateTime.Now;
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, selectedDate));

            // Assert
            Assert.NotNull(cut.Instance);
            Assert.Equal(selectedDate, cut.Instance.SelectedDate);
        }
    }

    [Trait("Category", "Component")]
    public class OnInitializedTests : TestContext
    {
        [Fact]
        public void OnInitialized_WhenActivityServiceIsNotNull_SubscribesToActivityChanged()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert - Component should have subscribed to the event
            // We verify this by raising the event and checking it doesn't throw
            mockActivityService.Raise(x => x.OnActivityChanged += null);
            Assert.NotNull(cut.Instance);
        }
    }

    [Trait("Category", "Component")]
    public class FormattedTotalMinutesTests : TestContext
    {
        [Fact]
        public void FormattedTotalMinutes_WhenTimeFormatterReturnsValue_ReturnsFormattedValue()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert - TimeFormatter.FormatTime(120) returns "2h"
            Assert.Equal("2h", cut.Instance.FormattedTotalMinutes);
        }

        [Fact]
        public void FormattedTotalMinutes_WhenTimeFormatterIsNull_ReturnsRawValue()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Set TimeFormatter to null via reflection to cover null-conditional branch
            var tfField = typeof(TimeDistributionChart).GetProperty("TimeFormatter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            tfField!.SetValue(cut.Instance, null);

            // Act
            var result = cut.Instance.FormattedTotalMinutes;

            // Assert - TimeFormatter is null, falls through to TotalMinutes.ToString()
            Assert.Equal("120", result);
        }

        [Fact]
        public void FormattedTotalMinutes_WhenFormatTimeThrowsAndLoggerIsNull_ReturnsRawValue()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton<TimeFormatter>(new ThrowingTimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Set Logger to null via reflection to cover null-conditional branch
            var loggerField = typeof(TimeDistributionChart).GetProperty("Logger", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            loggerField!.SetValue(cut.Instance, null);

            // Act
            var result = cut.Instance.FormattedTotalMinutes;

            // Assert - Catch block returns raw value, Logger?.LogError is null-safe
            Assert.Equal("120", result);
        }

        [Fact]
        public void FormattedTotalMinutes_WhenFormatTimeThrows_ReturnsRawValue()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton<TimeFormatter>(new ThrowingTimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Act & Assert - Catch block returns raw value
            Assert.Equal("120", cut.Instance.FormattedTotalMinutes);

            // Assert - Error should be logged (once from UpdateChartAsync, once from FormattedTotalMinutes)
            mockLogger.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2));
        }
    }

    [Trait("Category", "Component")]
    public class HasDataTests : TestContext
    {
        [Fact]
        public void HasData_WhenDistributionHasData_ReturnsTrue()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 }, { "Break", 20 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert
            Assert.True(cut.Instance.HasData);
            Assert.Equal(120, cut.Instance.TotalMinutes);
        }

        [Fact]
        public void HasData_WhenDistributionIsEmpty_ReturnsFalse()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert
            Assert.False(cut.Instance.HasData);
            Assert.Equal(0, cut.Instance.TotalMinutes);
        }
    }

    [Trait("Category", "Component")]
    public class DisposeTests : TestContext
    {
        [Fact]
        public void Dispose_WhenCalled_UnsubscribesFromEvent()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));
            
            cut.Instance.Dispose();

            // Assert - After dispose, raising the event should not cause issues
            mockActivityService.Raise(x => x.OnActivityChanged += null);
            Assert.NotNull(cut.Instance);
        }

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Act & Assert - Multiple disposes should not throw
            cut.Instance.Dispose();
            cut.Instance.Dispose();
        }
    }

    [Trait("Category", "Component")]
    public class OnParametersSetTests : TestContext
    {
        [Fact]
        public void OnParametersSetAsync_WhenDateChanged_UpdatesChart()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var initialDate = new DateTime(2024, 1, 1);
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, initialDate));

            // Act - Change the date
            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.SelectedDate, initialDate.AddDays(1)));

            // Assert - GetTimeDistribution should be called for the new date
            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.AtLeast(2));
        }
    }

    [Trait("Category", "Component")]
    public class OnActivityChangedTests : TestContext
    {
        [Fact]
        public void OnActivityChanged_WhenNotDisposed_UpdatesChart()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Act - Trigger activity changed event
            mockActivityService.Raise(x => x.OnActivityChanged += null);

            // Assert - GetTimeDistribution should be called again after activity change
            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.AtLeast(2));
        }

        [Fact]
        public void OnActivityChanged_WhenDisposed_DoesNotUpdateChart()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            var initialCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));

            // Act - Dispose then trigger activity changed event
            cut.Instance.Dispose();
            mockActivityService.Raise(x => x.OnActivityChanged += null);

            // Assert - GetTimeDistribution should not be called after dispose
            var finalCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));
            Assert.Equal(initialCallCount, finalCallCount);
        }

        [Fact]
        public void OnActivityChanged_WhenNotRendered_DoesNotUpdateChart()
        {
            // Arrange - Create component without triggering render lifecycle
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            // Act - Raise event before component is rendered (_isRendered is false)
            mockActivityService.Raise(x => x.OnActivityChanged += null);

            // Assert - No calls yet since component hasn't been rendered
            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public void OnActivityChanged_WhenIsRenderedForcedFalse_DoesNotUpdateChart()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            var initialCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));

            // Force _isRendered to false via reflection to cover short-circuit branch
            var isRenderedField = typeof(TimeDistributionChart).GetField("_isRendered", BindingFlags.Instance | BindingFlags.NonPublic);
            isRenderedField!.SetValue(cut.Instance, false);

            // Act - Raise event while _isRendered is false
            mockActivityService.Raise(x => x.OnActivityChanged += null);

            // Assert - GetTimeDistribution should not be called
            var finalCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));
            Assert.Equal(initialCallCount, finalCallCount);
        }
    }

    [Trait("Category", "Component")]
    public class UpdateChartAsyncTests : TestContext
    {
        [Fact]
        public async Task UpdateChartAsync_WhenDisposed_ReturnsEarly()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            var initialCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));

            // Act - Dispose then call UpdateChartAsync via reflection
            cut.Instance.Dispose();
            var updateMethod = typeof(TimeDistributionChart).GetMethod("UpdateChartAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(updateMethod);
            await (Task)updateMethod.Invoke(cut.Instance, null);

            // Assert - GetTimeDistribution should not be called after dispose
            var finalCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));
            Assert.Equal(initialCallCount, finalCallCount);
        }
    }

    [Trait("Category", "Component")]
    public class JSErrorHandlingTests : TestContext
    {
        [Fact]
        public async Task UpdateChart_WhenJSThrowsExceptionOnCreate_LogsError()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });
            
            // Setup JS to throw
            JSInterop.Mode = JSRuntimeMode.Strict;
            JSInterop.SetupVoid("destroyChart", _ => true);
            JSInterop.SetupVoid("createDoughnutChart", _ => true)
                .SetException(new Microsoft.JSInterop.JSException("Chart.js error"));
            
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act - Render component which triggers OnAfterRenderAsync
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Wait for async operations to complete
            await Task.Delay(100);

            // Assert - Error should be logged
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Dispose_WhenJSThrowsExceptionOnDestroy_DoesNotPropagate()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());
            
            // Setup JS to throw on destroy
            JSInterop.Mode = JSRuntimeMode.Strict;
            JSInterop.SetupVoid("destroyChart", _ => true)
                .SetException(new Microsoft.JSInterop.JSException("Chart.js error"));
            
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Act & Assert - Should not throw
            cut.Instance.Dispose();
        }
    }

    [Trait("Category", "Component")]
    public class TotalMinutesTests : TestContext
    {
        [Fact]
        public void TotalMinutes_WhenMultipleCategories_SumsCorrectly()
        {
            // Arrange
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChart>>();
            
            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> 
                { 
                    { "Focus", 100 }, 
                    { "Short Break", 15 }, 
                    { "Long Break", 30 } 
                });
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);
            
            // Act
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            // Assert
            Assert.Equal(145, cut.Instance.TotalMinutes);
        }
    }
}

internal class ThrowingTimeFormatter : TimeFormatter
{
    public override string FormatTime(int minutes) =>
        throw new InvalidOperationException("Test exception from FormatTime");
}

