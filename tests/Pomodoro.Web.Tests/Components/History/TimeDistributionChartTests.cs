using System;
using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.History;

[Trait("Category", "Component")]
public class TimeDistributionChartTests
{
    [Trait("Category", "Component")]
    public class RenderingTests : TestContext
    {
        [Fact]
        public void Render_WithDefaultServices_RendersComponent()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.NotNull(cut.Instance);
        }

        [Fact]
        public void Render_WithSelectedDate_RendersComponent()
        {
            var selectedDate = DateTime.Now;
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, selectedDate));

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
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            mockActivityService.Raise(x => x.OnActivityChanged += null);
            Assert.NotNull(cut.Instance);
        }
    }

    [Trait("Category", "Component")]
    public class FormattedTotalTests : TestContext
    {
        [Fact]
        public void FormattedTotal_WhenTimeFormatterReturnsValue_ReturnsFormattedValue()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.Equal("2h", cut.Instance.FormattedTotal);
        }

        [Fact]
        public void FormattedTotal_WhenTimeFormatterIsNull_ReturnsRawValue()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            var tfField = typeof(TimeDistributionChartBase).GetProperty("TimeFormatter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            tfField!.SetValue(cut.Instance, null);

            var result = cut.Instance.FormattedTotal;

            Assert.Equal("120", result);
        }

        [Fact]
        public void FormattedTotal_WhenFormatTimeThrows_ReturnsRawValue()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 120 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton<TimeFormatter>(new ThrowingTimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.Equal("120", cut.Instance.FormattedTotal);
        }
    }

    [Trait("Category", "Component")]
    public class SegmentsTests : TestContext
    {
        [Fact]
        public void Segments_WhenDistributionHasData_ContainsItems()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 }, { "Break", 20 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.True(cut.Instance.Segments.Count > 0);
            Assert.Equal(120, cut.Instance.TotalMinutes);
        }

        [Fact]
        public void Segments_WhenDistributionIsEmpty_IsEmpty()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.Empty(cut.Instance.Segments);
            Assert.Equal(0, cut.Instance.TotalMinutes);
        }
    }

    [Trait("Category", "Component")]
    public class DisposeTests : TestContext
    {
        [Fact]
        public void Dispose_WhenCalled_UnsubscribesFromEvent()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            cut.Instance.Dispose();

            mockActivityService.Raise(x => x.OnActivityChanged += null);
            Assert.NotNull(cut.Instance);
        }

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>());

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

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
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var initialDate = new DateTime(2024, 1, 1);
            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, initialDate));

            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.SelectedDate, initialDate.AddDays(1)));

            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.AtLeast(2));
        }
    }

    [Trait("Category", "Component")]
    public class OnActivityChangedTests : TestContext
    {
        [Fact]
        public void OnActivityChanged_WhenNotDisposed_UpdatesChart()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            mockActivityService.Raise(x => x.OnActivityChanged += null);

            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.AtLeast(2));
        }

        [Fact]
        public void OnActivityChanged_WhenDisposed_DoesNotUpdateChart()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            var initialCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));

            cut.Instance.Dispose();
            mockActivityService.Raise(x => x.OnActivityChanged += null);

            var finalCallCount = mockActivityService.Invocations.Count(x => x.Method.Name == nameof(IActivityService.GetTimeDistribution));
            Assert.Equal(initialCallCount, finalCallCount);
        }

        [Fact]
        public void OnActivityChanged_WhenNotRendered_DoesNotUpdateChart()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int> { { "Focus", 100 } });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            mockActivityService.Raise(x => x.OnActivityChanged += null);

            mockActivityService.Verify(x => x.GetTimeDistribution(It.IsAny<DateTime>()), Times.Never);
        }
    }

    [Trait("Category", "Component")]
    public class TotalMinutesTests : TestContext
    {
        [Fact]
        public void TotalMinutes_WhenMultipleCategories_SumsCorrectly()
        {
            var mockActivityService = new Mock<IActivityService>();
            var mockLogger = new Mock<ILogger<TimeDistributionChartBase>>();

            mockActivityService
                .Setup(x => x.GetTimeDistribution(It.IsAny<DateTime>()))
                .Returns(new Dictionary<string, int>
                {
                    { "Focus", 100 },
                    { "Short Break", 15 },
                    { "Long Break", 30 }
                });

            Services.AddSingleton(mockActivityService.Object);
            Services.AddSingleton(new TimeFormatter());
            Services.AddSingleton(mockLogger.Object);

            var cut = RenderComponent<TimeDistributionChart>(parameters => parameters
                .Add(p => p.SelectedDate, DateTime.Now));

            Assert.Equal(145, cut.Instance.TotalMinutes);
        }
    }
}

internal class ThrowingTimeFormatter : TimeFormatter
{
    public override string FormatTime(int minutes) =>
        throw new InvalidOperationException("Test exception from FormatTime");
}
