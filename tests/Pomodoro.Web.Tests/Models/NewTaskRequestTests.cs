using FluentAssertions;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class NewTaskRequestTests
{
    [Fact]
    public void Defaults_AllOptionalParameters()
    {
        var req = new NewTaskRequest("Task");

        req.Name.Should().Be("Task");
        req.RepeatType.Should().Be(RepeatType.None);
        req.ScheduledDate.Should().BeNull();
        req.Weekdays.Should().BeNull();
        req.CustomDays.Should().Be(0);
        req.MonthlyDay.Should().Be(0);
        req.IsPaused.Should().BeFalse();
        req.PausedDate.Should().BeNull();
        req.ListId.Should().BeNull();
        req.QuarterlyDay.Should().Be(0);
        req.QuarterlyMonth.Should().Be(0);
        req.YearlyDay.Should().Be(0);
        req.YearlyMonth.Should().Be(0);
        req.WeekOfMonth.Should().Be(0);
    }

    [Fact]
    public void Sets_PausedDate_WhenProvided()
    {
        var pausedDate = new DateTime(2026, 1, 15);
        var req = new NewTaskRequest("Paused", IsPaused: true, PausedDate: pausedDate);

        req.IsPaused.Should().BeTrue();
        req.PausedDate.Should().Be(pausedDate);
    }

    [Fact]
    public void Sets_AllParameters()
    {
        var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday };
        var scheduled = new DateTime(2026, 2, 1);
        var pausedDate = new DateTime(2026, 1, 20);

        var req = new NewTaskRequest(
            "Full",
            RepeatType.Weekly,
            scheduled,
            weekdays,
            0,
            0,
            true,
            pausedDate,
            "list-123");

        req.Name.Should().Be("Full");
        req.RepeatType.Should().Be(RepeatType.Weekly);
        req.ScheduledDate.Should().Be(scheduled);
        req.Weekdays.Should().BeEquivalentTo(weekdays);
        req.IsPaused.Should().BeTrue();
        req.PausedDate.Should().Be(pausedDate);
        req.ListId.Should().Be("list-123");
    }
}
