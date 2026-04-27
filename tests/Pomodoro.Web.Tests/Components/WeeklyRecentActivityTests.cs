using Bunit;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class WeeklyRecentActivityTests : TestHelper
{
    [Fact]
    public void Render_WithNoActivities_ShowsEmptyState()
    {
        ActivityServiceMock.Setup(a => a.GetAllActivities()).Returns(new List<ActivityRecord>());

        var cut = RenderComponent<WeeklyRecentActivity>(parameters =>
            parameters.Add(p => p.WeekStart, new DateTime(2026, 4, 20)));

        Assert.Contains("No activities this week", cut.Markup);
    }

    [Fact]
    public void Render_WithPomodoroActivity_ShowsActivityRow()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        ActivityServiceMock.Setup(a => a.GetAllActivities()).Returns(activities);

        var cut = RenderComponent<WeeklyRecentActivity>(parameters =>
            parameters.Add(p => p.WeekStart, new DateTime(2026, 4, 20)));

        Assert.Contains("Pomodoro completed", cut.Markup);
        Assert.Contains("Task A", cut.Markup);
        Assert.Contains("25 min", cut.Markup);
    }

    [Fact]
    public void Render_WithBreakActivity_ShowsBreakRow()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 5, WasCompleted = true },
            new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = new DateTime(2026, 4, 22, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 15, WasCompleted = true }
        };
        ActivityServiceMock.Setup(a => a.GetAllActivities()).Returns(activities);

        var cut = RenderComponent<WeeklyRecentActivity>(parameters =>
            parameters.Add(p => p.WeekStart, new DateTime(2026, 4, 20)));

        Assert.Contains("Short break", cut.Markup);
        Assert.Contains("Long break", cut.Markup);
    }

    [Fact]
    public void Render_WithActivityOutsideWeek_DoesNotShow()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Old Task", CompletedAt = new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        ActivityServiceMock.Setup(a => a.GetAllActivities()).Returns(activities);

        var cut = RenderComponent<WeeklyRecentActivity>(parameters =>
            parameters.Add(p => p.WeekStart, new DateTime(2026, 4, 20)));

        Assert.Contains("No activities this week", cut.Markup);
    }

    [Fact]
    public void Render_WithNullTaskName_ShowsOnlyDateMeta()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = null, CompletedAt = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Local), DurationMinutes = 25, WasCompleted = true }
        };
        ActivityServiceMock.Setup(a => a.GetAllActivities()).Returns(activities);

        var cut = RenderComponent<WeeklyRecentActivity>(parameters =>
            parameters.Add(p => p.WeekStart, new DateTime(2026, 4, 20)));

        Assert.Contains("Pomodoro completed", cut.Markup);
        Assert.Contains("25 min", cut.Markup);
        Assert.DoesNotContain("·", cut.Markup);
    }
}
