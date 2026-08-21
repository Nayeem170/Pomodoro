using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class RepeatRuleQuarterlyYearlyTests
{
    private static TaskItem TaskAnchoredAt(DateTime anchor, RepeatRule rule)
    {
        return new TaskItem { CreatedAt = anchor, Repeat = rule };
    }

    [Fact]
    public void OccursOn_Quarterly_ThreeMonthsAfterAnchor_ReturnsTrue()
    {
        var anchor = new DateTime(2024, 2, 15);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15 });

        // Act
        var result = task.Repeat!.OccursOn(task, anchor.AddMonths(3));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Quarterly_SixMonthsAfterAnchor_ReturnsTrue()
    {
        var anchor = new DateTime(2024, 2, 15);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15 });

        // Act
        var result = task.Repeat!.OccursOn(task, anchor.AddMonths(6));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Quarterly_OneAndTwoMonthsAfterAnchor_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 2, 15);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15 });

        // Act
        var plusOne = task.Repeat!.OccursOn(task, anchor.AddMonths(1));
        var plusTwo = task.Repeat!.OccursOn(task, anchor.AddMonths(2));

        // Assert
        Assert.False(plusOne);
        Assert.False(plusTwo);
    }

    [Fact]
    public void OccursOn_Quarterly_Day31InShortMonth_ClampsToLastDay()
    {
        var anchor = new DateTime(2024, 8, 31);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 31 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2024, 11, 30));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Quarterly_Day31Chain_FebruaryNonLeap_ReturnsTrue()
    {
        var anchor = new DateTime(2024, 8, 31);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 31 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 2, 28));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Quarterly_UnsetDay_DefaultsToAnchorDay()
    {
        var anchor = new DateTime(2024, 2, 15);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2024, 5, 15));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Quarterly_YearBoundaryDelta_ReturnsTrue()
    {
        var anchor = new DateTime(2024, 11, 15);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 2, 15));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Yearly_UnsetFields_DefaultToAnchorMonthAndDay()
    {
        var anchor = new DateTime(2024, 3, 10);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Yearly });

        // Act
        var result = task.Repeat!.OccursOn(task, anchor.AddYears(1));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Yearly_ConfiguredMonthAndDay_ReturnsTrue()
    {
        var anchor = new DateTime(2024, 1, 1);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, YearlyMonth = 3, YearlyDay = 10 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 3, 10));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_Yearly_DayMismatch_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 1, 1);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, YearlyMonth = 3, YearlyDay = 11 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 3, 10));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OccursOn_Yearly_MonthMismatch_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 1, 1);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, YearlyDay = 10 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 4, 10));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OccursOn_Yearly_LeapAnchorDay_NonLeapYearClampsToFeb28()
    {
        var anchor = new DateTime(2024, 2, 29);
        var task = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Yearly });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2025, 2, 28));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_DateBeforeAnchor_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 2, 15);
        var quarterly = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15 });
        var yearly = TaskAnchoredAt(anchor, new RepeatRule { Type = RepeatType.Yearly });

        // Act
        var quarterlyResult = quarterly.Repeat!.OccursOn(quarterly, anchor.AddDays(-1));
        var yearlyResult = yearly.Repeat!.OccursOn(yearly, anchor.AddDays(-1));

        // Assert
        Assert.False(quarterlyResult);
        Assert.False(yearlyResult);
    }

    [Fact]
    public void OccursOn_EndDateBeforeCandidate_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 2, 15);
        var quarterly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15, EndDate = new DateTime(2024, 3, 15) });
        var yearly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, EndDate = new DateTime(2024, 6, 15) });

        // Act
        var quarterlyResult = quarterly.Repeat!.OccursOn(quarterly, anchor.AddMonths(3));
        var yearlyResult = yearly.Repeat!.OccursOn(yearly, anchor.AddYears(1));

        // Assert
        Assert.False(quarterlyResult);
        Assert.False(yearlyResult);
    }

    [Fact]
    public void OccursOn_PausedOnOrAfterAnchor_ReturnsFalse()
    {
        var anchor = new DateTime(2024, 2, 15);
        var pausedDate = new DateTime(2024, 3, 1);
        var quarterly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15, IsPaused = true, PausedDate = pausedDate });
        var yearly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, IsPaused = true, PausedDate = pausedDate });

        // Act
        var quarterlyResult = quarterly.Repeat!.OccursOn(quarterly, anchor.AddMonths(3));
        var yearlyResult = yearly.Repeat!.OccursOn(yearly, anchor.AddYears(1));

        // Assert
        Assert.False(quarterlyResult);
        Assert.False(yearlyResult);
    }

    [Fact]
    public void OccursOn_PausedBeforeAnchor_PauseIsIgnored()
    {
        var anchor = new DateTime(2024, 2, 15);
        var pausedDate = new DateTime(2024, 1, 1);
        var quarterly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15, IsPaused = true, PausedDate = pausedDate });
        var yearly = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, IsPaused = true, PausedDate = pausedDate });

        // Act
        var quarterlyResult = quarterly.Repeat!.OccursOn(quarterly, anchor.AddMonths(3));
        var yearlyResult = yearly.Repeat!.OccursOn(yearly, anchor.AddYears(1));

        // Assert
        Assert.True(quarterlyResult);
        Assert.True(yearlyResult);
    }
}
