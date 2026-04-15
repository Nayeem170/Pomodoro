using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for TimeFormatter service
/// </summary>
[Trait("Category", "Service")]
public class TimeFormatterTests
{
    private readonly TimeFormatter _formatter;

    public TimeFormatterTests()
    {
        _formatter = new TimeFormatter();
    }

    [Fact]
    public void FormatTime_ZeroMinutes_ReturnsZeroFormat()
    {
        // Act
        var result = _formatter.FormatTime(0);

        // Assert
        Assert.Equal("0m", result);
    }

    [Fact]
    public void FormatTime_LessThanHour_ReturnsMinutesFormat()
    {
        // Act
        var result = _formatter.FormatTime(45);

        // Assert
        Assert.Equal("45m", result);
    }

    [Fact]
    public void FormatTime_OneHour_ReturnsHoursFormat()
    {
        // Act
        var result = _formatter.FormatTime(60);

        // Assert
        Assert.Equal("1h", result);
    }

    [Fact]
    public void FormatTime_HoursAndMinutes_ReturnsCombinedFormat()
    {
        // Act
        var result = _formatter.FormatTime(90);

        // Assert
        Assert.Equal("1h 30m", result);
    }

    [Fact]
    public void FormatTime_MultipleHours_ReturnsHoursFormat()
    {
        // Act
        var result = _formatter.FormatTime(120);

        // Assert
        Assert.Equal("2h", result);
    }

    [Fact]
    public void FormatTime_MultipleHoursWithMinutes_ReturnsCombinedFormat()
    {
        // Act
        var result = _formatter.FormatTime(150);

        // Assert
        Assert.Equal("2h 30m", result);
    }

    [Fact]
    public void FormatTime_LargeValue_ReturnsCorrectFormat()
    {
        // Act
        var result = _formatter.FormatTime(375);

        // Assert
        Assert.Equal("6h 15m", result);
    }

    [Fact]
    public void FormatSimpleTime_ZeroMinutes_ReturnsZeroFormat()
    {
        // Act
        var result = _formatter.FormatSimpleTime(0);

        // Assert
        Assert.Equal("0m", result);
    }

    [Fact]
    public void FormatSimpleTime_LessThanHour_ReturnsMinutesFormat()
    {
        // Act
        var result = _formatter.FormatSimpleTime(30);

        // Assert
        Assert.Equal("30m", result);
    }

    [Fact]
    public void FormatSimpleTime_ExactHour_ReturnsCombinedFormat()
    {
        // Act
        var result = _formatter.FormatSimpleTime(60);

        // Assert
        Assert.Equal("1h 0m", result);
    }

    [Fact]
    public void FormatSimpleTime_HoursAndMinutes_ReturnsCombinedFormat()
    {
        // Act
        var result = _formatter.FormatSimpleTime(135);

        // Assert
        Assert.Equal("2h 15m", result);
    }

    [Fact]
    public void FormatSimpleTime_MultipleHours_ReturnsCombinedFormat()
    {
        // Act
        var result = _formatter.FormatSimpleTime(180);

        // Assert
        Assert.Equal("3h 0m", result);
    }
}

