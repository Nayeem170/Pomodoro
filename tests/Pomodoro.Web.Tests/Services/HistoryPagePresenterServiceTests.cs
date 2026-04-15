using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for HistoryPagePresenterService
/// </summary>
[Trait("Category", "Service")]
public class HistoryPagePresenterServiceTests
{
    private readonly HistoryPagePresenterService _service;

    public HistoryPagePresenterServiceTests()
    {
        var mockLogger = new Mock<ILogger<HistoryPagePresenterService>>();
        _service = new HistoryPagePresenterService(mockLogger.Object);
    }

    [Fact]
    public void FormatFocusTime_ShouldReturnStringRepresentation()
    {
        // Arrange
        var minutes = 30;

        // Act
        var result = _service.FormatFocusTime(minutes);

        // Assert
        Assert.Equal("30", result);
    }

    [Fact]
    public void FormatFocusTime_WithZero_ShouldReturnZero()
    {
        // Arrange
        var minutes = 0;

        // Act
        var result = _service.FormatFocusTime(minutes);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void FormatFocusTime_WithLargeValue_ShouldReturnStringRepresentation()
    {
        // Arrange
        var minutes = 480;

        // Act
        var result = _service.FormatFocusTime(minutes);

        // Assert
        Assert.Equal("480", result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(180)]
    [InlineData(240)]
    public void FormatFocusTime_WithVariousValues_ShouldReturnStringRepresentation(int minutes)
    {
        // Act
        var result = _service.FormatFocusTime(minutes);

        // Assert
        Assert.Equal(minutes.ToString(), result);
    }
}

[Trait("Category", "Service")]
public class HistoryPagePaginationStateTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var state = new HistoryPagePaginationState();

        Assert.Equal(0, state.CurrentSkip);
        Assert.False(state.HasMoreActivities);
        Assert.False(state.ObserverInitialized);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var state = new HistoryPagePaginationState
        {
            CurrentSkip = 20,
            HasMoreActivities = true,
            ObserverInitialized = true
        };

        Assert.Equal(20, state.CurrentSkip);
        Assert.True(state.HasMoreActivities);
        Assert.True(state.ObserverInitialized);
    }

    [Fact]
    public void Properties_CanBeUpdated()
    {
        var state = new HistoryPagePaginationState
        {
            CurrentSkip = 10,
            HasMoreActivities = true,
            ObserverInitialized = true
        };

        state.CurrentSkip = 30;
        state.HasMoreActivities = false;
        state.ObserverInitialized = false;

        Assert.Equal(30, state.CurrentSkip);
        Assert.False(state.HasMoreActivities);
        Assert.False(state.ObserverInitialized);
    }
}

