using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for ChartDataFormatter service
/// </summary>
[Trait("Category", "Service")]
public class ChartDataFormatterTests
{
    private readonly ChartDataFormatter _formatter;

    public ChartDataFormatterTests()
    {
        _formatter = new ChartDataFormatter();
    }

    [Fact]
    public void PrepareWeeklyChartData_EmptyDictionaries_ReturnsEmptyArrays()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday

        // Act
        var (labels, focusData, breakData) = _formatter.PrepareWeeklyChartData(
            new Dictionary<DateTime, int>(),
            new Dictionary<DateTime, int>(),
            weekStart);

        // Assert
        Assert.Equal(7, labels.Length);
        Assert.Equal(7, focusData.Length);
        Assert.Equal(7, breakData.Length);
        Assert.All(focusData, minutes => Assert.Equal(0, minutes));
        Assert.All(breakData, minutes => Assert.Equal(0, minutes));
    }

    [Fact]
    public void PrepareWeeklyChartData_WithFocusData_ReturnsCorrectData()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday
        var focusData = new Dictionary<DateTime, int>
        {
            [weekStart] = 60,
            [weekStart.AddDays(2)] = 90,
            [weekStart.AddDays(4)] = 120
        };

        // Act
        var (labels, focusResult, breakResult) = _formatter.PrepareWeeklyChartData(
            focusData,
            new Dictionary<DateTime, int>(),
            weekStart);

        // Assert
        Assert.Equal(60, focusResult[0]);
        Assert.Equal(0, focusResult[1]);
        Assert.Equal(90, focusResult[2]);
        Assert.Equal(0, focusResult[3]);
        Assert.Equal(120, focusResult[4]);
        Assert.Equal(0, focusResult[5]);
        Assert.Equal(0, focusResult[6]);
    }

    [Fact]
    public void PrepareWeeklyChartData_WithBreakData_ReturnsCorrectData()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday
        var breakData = new Dictionary<DateTime, int>
        {
            [weekStart.AddDays(1)] = 15,
            [weekStart.AddDays(3)] = 30
        };

        // Act
        var (labels, focusResult, breakResult) = _formatter.PrepareWeeklyChartData(
            new Dictionary<DateTime, int>(),
            breakData,
            weekStart);

        // Assert
        Assert.Equal(0, breakResult[0]);
        Assert.Equal(15, breakResult[1]);
        Assert.Equal(0, breakResult[2]);
        Assert.Equal(30, breakResult[3]);
        Assert.Equal(0, breakResult[4]);
        Assert.Equal(0, breakResult[5]);
        Assert.Equal(0, breakResult[6]);
    }

    [Fact]
    public void PrepareWeeklyChartData_WithBothDataTypes_ReturnsCorrectData()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday
        var focusData = new Dictionary<DateTime, int>
        {
            [weekStart] = 60,
            [weekStart.AddDays(2)] = 90
        };
        var breakData = new Dictionary<DateTime, int>
        {
            [weekStart.AddDays(1)] = 15,
            [weekStart.AddDays(3)] = 30
        };

        // Act
        var (labels, focusResult, breakResult) = _formatter.PrepareWeeklyChartData(
            focusData,
            breakData,
            weekStart);

        // Assert
        Assert.Equal(60, focusResult[0]);
        Assert.Equal(0, focusResult[1]);
        Assert.Equal(90, focusResult[2]);
        Assert.Equal(0, focusResult[3]);
        Assert.Equal(0, focusResult[4]);
        Assert.Equal(0, focusResult[5]);
        Assert.Equal(0, focusResult[6]);

        Assert.Equal(0, breakResult[0]);
        Assert.Equal(15, breakResult[1]);
        Assert.Equal(0, breakResult[2]);
        Assert.Equal(30, breakResult[3]);
        Assert.Equal(0, breakResult[4]);
        Assert.Equal(0, breakResult[5]);
        Assert.Equal(0, breakResult[6]);
    }

    [Fact]
    public void PrepareWeeklyChartData_ReturnsCorrectDayLabels()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday

        // Act
        var (labels, _, _) = _formatter.PrepareWeeklyChartData(
            new Dictionary<DateTime, int>(),
            new Dictionary<DateTime, int>(),
            weekStart);

        // Assert
        Assert.Equal(7, labels.Length);
        Assert.Equal("Mon", labels[0]);
        Assert.Equal("Tue", labels[1]);
        Assert.Equal("Wed", labels[2]);
        Assert.Equal("Thu", labels[3]);
        Assert.Equal("Fri", labels[4]);
        Assert.Equal("Sat", labels[5]);
        Assert.Equal("Sun", labels[6]);
    }

    [Fact]
    public void PrepareWeeklyChartData_DateOutsideWeek_Ignored()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 1); // Monday
        var focusData = new Dictionary<DateTime, int>
        {
            [weekStart.AddDays(-1)] = 100, // Before week
            [weekStart.AddDays(7)] = 200   // After week
        };

        // Act
        var (_, focusResult, _) = _formatter.PrepareWeeklyChartData(
            focusData,
            new Dictionary<DateTime, int>(),
            weekStart);

        // Assert
        Assert.All(focusResult, minutes => Assert.Equal(0, minutes));
    }

    [Fact]
    public void PrepareWeeklyChartData_MixedWeekStart_HandlesCorrectly()
    {
        // Arrange
        var weekStart = new DateTime(2024, 1, 3); // Wednesday
        var focusData = new Dictionary<DateTime, int>
        {
            [weekStart] = 60,
            [weekStart.AddDays(1)] = 90
        };

        // Act
        var (labels, focusResult, _) = _formatter.PrepareWeeklyChartData(
            focusData,
            new Dictionary<DateTime, int>(),
            weekStart);

        // Assert
        Assert.Equal("Wed", labels[0]);
        Assert.Equal("Thu", labels[1]);
        Assert.Equal("Fri", labels[2]);
        Assert.Equal("Sat", labels[3]);
        Assert.Equal("Sun", labels[4]);
        Assert.Equal("Mon", labels[5]);
        Assert.Equal("Tue", labels[6]);

        Assert.Equal(60, focusResult[0]);
        Assert.Equal(90, focusResult[1]);
        Assert.Equal(0, focusResult[2]);
        Assert.Equal(0, focusResult[3]);
        Assert.Equal(0, focusResult[4]);
        Assert.Equal(0, focusResult[5]);
        Assert.Equal(0, focusResult[6]);
    }
}

