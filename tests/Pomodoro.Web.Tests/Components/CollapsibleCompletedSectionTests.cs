using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class CollapsibleCompletedSectionTests : TestContext
{
    [Fact]
    public void CompletedSection_CollapsedByDefault()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Active", IsCompleted = false },
            new() { Id = Guid.NewGuid(), Name = "Done task", IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, (Guid?)null));

        cut.FindAll(".completed-section").Should().HaveCount(1);
        cut.FindAll(".completed-toggle").Should().HaveCount(1);
        cut.Markup.Should().NotContain("Done task");
    }

    [Fact]
    public void CompletedSection_ToggleExpandsAndCollapses()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Active", IsCompleted = false },
            new() { Id = Guid.NewGuid(), Name = "Done task", IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, (Guid?)null));

        cut.Find(".completed-toggle").Click();
        cut.Render();
        cut.Markup.Should().Contain("Done task");

        cut.Find(".completed-toggle").Click();
        cut.Render();
        cut.Markup.Should().NotContain("Done task");
    }

    [Fact]
    public void CompletedSection_ShowsCount()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Done 1", IsCompleted = true },
            new() { Id = Guid.NewGuid(), Name = "Done 2", IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, (Guid?)null));

        cut.Find(".completed-count").TextContent.Should().Contain("2");
    }

    [Fact]
    public void CompletedRow_ShowsCheckboxAndDeleteOnly()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Completed Task",
            IsCompleted = true
        };

        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));

        cut.FindAll("button[aria-label=\"Undo\"]").Should().HaveCount(1);
        cut.FindAll("button[aria-label=\"Delete\"]").Should().HaveCount(1);
        cut.FindAll("button[aria-label=\"Edit task\"]").Should().HaveCount(0);
        cut.FindAll("button[aria-label=\"Add subtask\"]").Should().HaveCount(0);
    }

    [Fact]
    public void CompletedRow_HasCompletedClass()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Done",
            IsCompleted = true
        };

        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));

        cut.Find(".task-row").ClassList.Should().Contain("completed");
    }

    [Fact]
    public void ActiveRow_ShowsEditButton()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            IsCompleted = false
        };

        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));

        cut.FindAll("button[aria-label=\"Edit task\"]").Should().HaveCount(1);
    }

    [Fact]
    public void CompletedToggle_HasAriaExpanded()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Done", IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, (Guid?)null));

        var toggle = cut.Find(".completed-toggle");
        toggle.GetAttribute("aria-expanded").Should().BeEquivalentTo("false");

        toggle.Click();
        cut.Render();

        cut.Find(".completed-toggle").GetAttribute("aria-expanded").Should().BeEquivalentTo("true");
    }
}
