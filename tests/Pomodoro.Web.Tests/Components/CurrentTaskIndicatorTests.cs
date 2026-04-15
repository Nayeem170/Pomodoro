using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Timer;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class CurrentTaskIndicatorTests : TestContext
{
    public CurrentTaskIndicatorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_WithPomodoroSessionAndNoTask_ShowsSelectPrompt()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(parameters => parameters
            .Add(p => p.CurrentSessionType, SessionType.Pomodoro)
            .Add(p => p.CurrentTaskId, (Guid?)null)
            .Add(p => p.Tasks, new List<TaskItem>()));

        cut.Find(".current-task-indicator .task-label").TextContent.Should()
            .Be(Constants.TaskUI.SelectTaskPrompt);
    }

    [Fact]
    public void Render_WithPomodoroSessionAndMatchingTask_ShowsTaskName()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "My Task" };

        var cut = RenderComponent<CurrentTaskIndicator>(parameters => parameters
            .Add(p => p.CurrentSessionType, SessionType.Pomodoro)
            .Add(p => p.CurrentTaskId, taskId)
            .Add(p => p.Tasks, new List<TaskItem> { task }));

        cut.Find(".current-task-indicator .task-label").TextContent.Should()
            .Be(Constants.TaskUI.CurrentTaskLabel);
        cut.Find(".current-task-indicator .task-name").TextContent.Should()
            .Be("My Task");
    }

    [Fact]
    public void Render_WithPomodoroSessionAndNonMatchingTask_ShowsNothing()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(parameters => parameters
            .Add(p => p.CurrentSessionType, SessionType.Pomodoro)
            .Add(p => p.CurrentTaskId, Guid.NewGuid())
            .Add(p => p.Tasks, new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Other Task" }
            }));

        cut.Find(".current-task-indicator").ChildNodes.Should().BeEmpty();
    }

    [Fact]
    public void Render_WithShortBreakSession_DoesNotRenderIndicator()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(parameters => parameters
            .Add(p => p.CurrentSessionType, SessionType.ShortBreak)
            .Add(p => p.CurrentTaskId, Guid.NewGuid())
            .Add(p => p.Tasks, new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Some Task" }
            }));

        cut.Markup.Should().NotContain("current-task-indicator");
    }

    [Fact]
    public void Render_WithLongBreakSession_DoesNotRenderIndicator()
    {
        var cut = RenderComponent<CurrentTaskIndicator>(parameters => parameters
            .Add(p => p.CurrentSessionType, SessionType.LongBreak)
            .Add(p => p.CurrentTaskId, Guid.NewGuid())
            .Add(p => p.Tasks, new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Some Task" }
            }));

        cut.Markup.Should().NotContain("current-task-indicator");
    }
}

