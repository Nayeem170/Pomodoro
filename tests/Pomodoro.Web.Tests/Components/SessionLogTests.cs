using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class SessionLogTests : TestContext
{
    [Fact]
    public void EmptySessions_ShowsEmptyMessage()
    {
        var cut = RenderComponent<SessionLog>(p => p
            .Add(x => x.Sessions, new List<ActivityRecord>()));

        cut.FindAll(".session-log-empty").Should().HaveCount(1);
        cut.Markup.Should().Contain("No sessions yet");
    }

    [Fact]
    public void WithSessions_ShowsSessionRows()
    {
        var sessions = new List<ActivityRecord>
        {
            new()
            {
                Type = SessionType.Pomodoro,
                TaskName = "Design API",
                CompletedAt = new DateTime(2026, 8, 3, 14, 15, 0),
                DurationMinutes = 25
            },
            new()
            {
                Type = SessionType.Pomodoro,
                TaskName = "Write docs",
                CompletedAt = new DateTime(2026, 8, 3, 13, 42, 0),
                DurationMinutes = 25
            }
        };

        var cut = RenderComponent<SessionLog>(p => p
            .Add(x => x.Sessions, sessions));

        cut.FindAll(".session-log-row").Should().HaveCount(2);
        cut.Markup.Should().Contain("Design API");
        cut.Markup.Should().Contain("Write docs");
        cut.Markup.Should().Contain("25m");
    }

    [Fact]
    public void ShowsSessionCount()
    {
        var sessions = new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = "Task 1", DurationMinutes = 25 },
            new() { Type = SessionType.Pomodoro, TaskName = "Task 2", DurationMinutes = 25 },
            new() { Type = SessionType.Pomodoro, TaskName = "Task 3", DurationMinutes = 25 }
        };

        var cut = RenderComponent<SessionLog>(p => p
            .Add(x => x.Sessions, sessions));

        cut.Markup.Should().Contain("3 done");
    }

    [Fact]
    public void SessionWithNullTaskName_ShowsFocusFallback()
    {
        var sessions = new List<ActivityRecord>
        {
            new() { Type = SessionType.Pomodoro, TaskName = null, DurationMinutes = 25 }
        };

        var cut = RenderComponent<SessionLog>(p => p
            .Add(x => x.Sessions, sessions));

        cut.Markup.Should().Contain("Focus");
    }

    [Fact]
    public void ShowsHeader()
    {
        var cut = RenderComponent<SessionLog>(p => p
            .Add(x => x.Sessions, new List<ActivityRecord>()));

        cut.Markup.Should().Contain("Today's Sessions");
    }
}
