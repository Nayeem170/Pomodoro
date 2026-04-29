using System.Reflection;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class WeeklyTimeDistributionBaseTests
{
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<TimeFormatter> _mockTimeFormatter;

    private class TestableWeeklyTimeDistribution : WeeklyTimeDistributionBase
    {
        public void SetActivityService(IActivityService service) => ActivityService = service;
        public void SetTimeFormatter(TimeFormatter formatter) => TimeFormatter = formatter;
    }

    public WeeklyTimeDistributionBaseTests()
    {
        _mockActivityService = new Mock<IActivityService>();
        _mockTimeFormatter = new Mock<TimeFormatter>();
        _mockTimeFormatter.Setup(t => t.FormatTime(It.IsAny<int>())).Returns("25m");
    }

    private TestableWeeklyTimeDistribution CreateSut(
        List<ActivityRecord>? activities = null,
        DateTime? weekStart = null)
    {
        _mockActivityService.Setup(a => a.GetAllActivities()).Returns(activities ?? new List<ActivityRecord>());
        var sut = new TestableWeeklyTimeDistribution();
        sut.SetActivityService(_mockActivityService.Object);
        sut.SetTimeFormatter(_mockTimeFormatter.Object);
        sut.WeekStart = weekStart ?? new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Local);
        sut.WeeklyFocusMinutes = new Dictionary<DateTime, int>();
        sut.WeeklyBreakMinutes = new Dictionary<DateTime, int>();
        sut.GetType().GetMethod("OnInitialized", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(sut, null);
        return sut;
    }

    [Fact]
    public void CalculateSegments_EmptyWeek_NoSegments()
    {
        var sut = CreateSut();

        Assert.Empty(sut.Segments);
        Assert.Equal(0, sut.TotalMinutes);
    }

    [Fact]
    public void CalculateSegments_SinglePomodoro_OneSegment()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task 1", CompletedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Single(sut.Segments);
        Assert.Equal("Task 1", sut.Segments[0].Label);
        Assert.Equal(25, sut.TotalMinutes);
        Assert.Equal(100.0, sut.Segments[0].Percentage);
    }

    [Fact]
    public void CalculateSegments_MultipleTasks_CorrectPercentages()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task B", CompletedAt = new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Local), DurationMinutes = 50, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Equal(2, sut.Segments.Count);
        Assert.Equal(67, sut.Segments[0].Percentage);
        Assert.Equal(33, sut.Segments[1].Percentage);
        Assert.Equal(75, sut.TotalMinutes);
    }

    [Fact]
    public void CalculateSegments_WithBreaks_IncludesBreakSegments()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task 1", CompletedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = new DateTime(2026, 4, 20, 10, 25, 0, DateTimeKind.Local), DurationMinutes = 5, WasCompleted = true },
            new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = new DateTime(2026, 4, 20, 10, 30, 0, DateTimeKind.Local), DurationMinutes = 15, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Equal(2, sut.Segments.Count);
        Assert.Equal(56, sut.Segments[0].Percentage);
        Assert.Equal(44, sut.Segments[1].Percentage);
        Assert.Equal(45, sut.TotalMinutes);
    }

    [Fact]
    public void CalculateSegments_ActivitiesOutsideWeek_Ignored()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Old Task", CompletedAt = new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Empty(sut.Segments);
    }

    [Fact]
    public void FormattedTotal_ReturnsFormattedTime()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task 1", CompletedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Equal("25m", sut.FormattedTotal);
    }

    [Fact]
    public void Dispose_DoesNotThrowWhenCalledTwice()
    {
        var sut = CreateSut();
        sut.Dispose();
        sut.Dispose();
    }

    [Fact]
    public void CalculateSegments_TaskWithNullName_UsesFocusTimeLabel()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = null, CompletedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        var sut = CreateSut(activities);

        Assert.Equal("Focus time", sut.Segments[0].Label);
    }
}
