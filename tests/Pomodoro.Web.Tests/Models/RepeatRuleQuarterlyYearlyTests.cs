using System.Text.Json;
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

    [Fact]
    public void OccursOn_Quarterly_ExplicitGroup_OverridesAnchorPhase()
    {
        var anchor = new DateTime(2024, 5, 10);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 10, QuarterlyMonth = 1 });

        // Act
        var jul = task.Repeat!.OccursOn(task, new DateTime(2024, 7, 10));
        var oct = task.Repeat!.OccursOn(task, new DateTime(2024, 10, 10));
        var jan = task.Repeat!.OccursOn(task, new DateTime(2025, 1, 10));

        // Assert
        Assert.True(jul);
        Assert.True(oct);
        Assert.True(jan);
    }

    [Fact]
    public void OccursOn_Quarterly_ExplicitGroup_OtherGroupsReturnFalse()
    {
        var anchor = new DateTime(2024, 5, 10);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 10, QuarterlyMonth = 1 });

        // Act
        var aug = task.Repeat!.OccursOn(task, new DateTime(2024, 8, 10));
        var nov = task.Repeat!.OccursOn(task, new DateTime(2024, 11, 10));
        var feb = task.Repeat!.OccursOn(task, new DateTime(2025, 2, 10));

        // Assert
        Assert.False(aug);
        Assert.False(nov);
        Assert.False(feb);
    }

    [Fact]
    public void OccursOn_Quarterly_Group3_FromFebruaryAnchor_MatchesGroupMonths()
    {
        var anchor = new DateTime(2024, 2, 15);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = 15, QuarterlyMonth = 3 });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2024, 9, 15));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Deserialize_OldJson_WithoutQuarterlyMonth_DefaultsToNull()
    {
        var json = """{"Type":5,"QuarterlyDay":15}""";

        // Act
        var rule = JsonSerializer.Deserialize<RepeatRule>(json);

        // Assert
        Assert.NotNull(rule);
        Assert.Equal(RepeatType.Quarterly, rule.Type);
        Assert.Equal(15, rule.QuarterlyDay);
        Assert.Null(rule.QuarterlyMonth);
    }

    [Fact]
    public void WeekOrdinalOf_March2026Mondays_CountsOneThroughFive()
    {
        // Act
        var ordinals = new[]
        {
            RepeatRule.WeekOrdinalOf(new DateTime(2026, 3, 2)),
            RepeatRule.WeekOrdinalOf(new DateTime(2026, 3, 9)),
            RepeatRule.WeekOrdinalOf(new DateTime(2026, 3, 16)),
            RepeatRule.WeekOrdinalOf(new DateTime(2026, 3, 23)),
            RepeatRule.WeekOrdinalOf(new DateTime(2026, 3, 30))
        };

        // Assert
        Assert.Equal([1, 2, 3, 4, 5], ordinals);
    }

    [Fact]
    public void WeekOrdinalOf_FourthAndLastFridayOfFourFridayMonth_IsFour()
    {
        // Act
        var ordinal = RepeatRule.WeekOrdinalOf(new DateTime(2026, 2, 27));

        // Assert
        Assert.Equal(4, ordinal);
    }

    [Fact]
    public void IsLastWeekdayOfMonth_February2026_DetectsLastWeekday()
    {
        // Act
        var last = RepeatRule.IsLastWeekdayOfMonth(new DateTime(2026, 2, 27));
        var before = RepeatRule.IsLastWeekdayOfMonth(new DateTime(2026, 2, 20));

        // Assert
        Assert.True(last);
        Assert.False(before);
    }

    [Theory]
    [InlineData(1, 31)]
    [InlineData(2, 29)]
    [InlineData(3, 31)]
    [InlineData(4, 30)]
    [InlineData(5, 31)]
    [InlineData(6, 30)]
    [InlineData(7, 31)]
    [InlineData(8, 31)]
    [InlineData(9, 30)]
    [InlineData(10, 31)]
    [InlineData(11, 30)]
    [InlineData(12, 31)]
    public void MaxSelectableDay_ForEachMonth_ReturnsFebruaryCapAndShortMonths(int month, int expected)
    {
        // Act
        var max = RepeatRule.MaxSelectableDay(month);

        // Assert
        Assert.Equal(expected, max);
    }

    [Fact]
    public void OccursOn_MonthlyWeekdayMode_FirstMonday_ReturnsTrue()
    {
        var anchor = new DateTime(2026, 3, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = 1, Weekdays = [DayOfWeek.Monday] });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 2));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_MonthlyWeekdayMode_SecondMondayWithFirstOrdinal_ReturnsFalse()
    {
        var anchor = new DateTime(2026, 3, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = 1, Weekdays = [DayOfWeek.Monday] });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 9));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OccursOn_MonthlyWeekdayMode_LastOrdinal_MatchesLastMondayOnly()
    {
        var anchor = new DateTime(2026, 3, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = RepeatRule.LastWeekOfMonth, Weekdays = [DayOfWeek.Monday] });

        // Act
        var last = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 30));
        var fourth = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 23));

        // Assert
        Assert.True(last);
        Assert.False(fourth);
    }

    [Fact]
    public void OccursOn_MonthlyWeekdayMode_FourthOrdinal_InFourMondayMonth_CoincidesWithLast()
    {
        var anchor = new DateTime(2026, 2, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = 4, Weekdays = [DayOfWeek.Monday] });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2026, 2, 23));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OccursOn_MonthlyWeekdayMode_MultipleWeekdays_MatchesSecondOfEach()
    {
        var anchor = new DateTime(2026, 3, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = 2, Weekdays = [DayOfWeek.Monday, DayOfWeek.Wednesday] });

        // Act
        var secondMonday = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 9));
        var secondWednesday = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 11));

        // Assert
        Assert.True(secondMonday);
        Assert.True(secondWednesday);
    }

    [Fact]
    public void OccursOn_QuarterlyWeekdayMode_FirstFridayInGroupMonth_ReturnsTrueAndNonGroupFalse()
    {
        var anchor = new DateTime(2026, 2, 15);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Quarterly, QuarterlyMonth = 2, WeekOfMonth = 1, Weekdays = [DayOfWeek.Friday] });

        // Act
        var november = task.Repeat!.OccursOn(task, new DateTime(2026, 11, 6));
        var october = task.Repeat!.OccursOn(task, new DateTime(2026, 10, 2));

        // Assert
        Assert.True(november);
        Assert.False(october);
    }

    [Fact]
    public void OccursOn_YearlyWeekdayMode_LastFridayOfMarch_ReturnsTrueAndOtherMonthFalse()
    {
        var anchor = new DateTime(2026, 3, 1);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Yearly, YearlyMonth = 3, WeekOfMonth = RepeatRule.LastWeekOfMonth, Weekdays = [DayOfWeek.Friday] });

        // Act
        var march = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 27));
        var april = task.Repeat!.OccursOn(task, new DateTime(2026, 4, 24));

        // Assert
        Assert.True(march);
        Assert.False(april);
    }

    [Fact]
    public void OccursOn_WeekdayModeWithEmptyWeekdays_ReturnsFalse()
    {
        var anchor = new DateTime(2026, 3, 2);
        var task = TaskAnchoredAt(anchor,
            new RepeatRule { Type = RepeatType.Monthly, WeekOfMonth = 1, Weekdays = [] });

        // Act
        var result = task.Repeat!.OccursOn(task, new DateTime(2026, 3, 2));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Deserialize_OldJson_WithoutWeekOfMonth_DefaultsToNull()
    {
        var json = """{"Type":4,"MonthlyDay":15,"Weekdays":[1]}""";

        // Act
        var rule = JsonSerializer.Deserialize<RepeatRule>(json);

        // Assert
        Assert.NotNull(rule);
        Assert.Equal(RepeatType.Monthly, rule.Type);
        Assert.Null(rule.WeekOfMonth);
    }

    [Fact]
    public void OccursOn_QuarterlyWeekdayMode_GatesApplyLikeDayOfMonthMode()
    {
        var anchor = new DateTime(2026, 2, 15);
        var rule = new RepeatRule
        {
            Type = RepeatType.Quarterly,
            QuarterlyMonth = 2,
            WeekOfMonth = 1,
            Weekdays = [DayOfWeek.Friday]
        };
        var expiredRule = new RepeatRule
        {
            Type = RepeatType.Quarterly,
            QuarterlyMonth = 2,
            WeekOfMonth = 1,
            Weekdays = [DayOfWeek.Friday],
            EndDate = new DateTime(2026, 11, 5)
        };
        var pausedRule = new RepeatRule
        {
            Type = RepeatType.Quarterly,
            QuarterlyMonth = 2,
            WeekOfMonth = 1,
            Weekdays = [DayOfWeek.Friday],
            IsPaused = true,
            PausedDate = new DateTime(2026, 2, 15)
        };

        // Act
        var beforeAnchor = rule.OccursOn(TaskAnchoredAt(anchor, rule), new DateTime(2026, 2, 13));
        var expired = expiredRule.OccursOn(TaskAnchoredAt(anchor, expiredRule), new DateTime(2026, 11, 6));
        var paused = pausedRule.OccursOn(TaskAnchoredAt(anchor, pausedRule), new DateTime(2026, 11, 6));

        // Assert
        Assert.False(beforeAnchor);
        Assert.False(expired);
        Assert.False(paused);
    }
}
