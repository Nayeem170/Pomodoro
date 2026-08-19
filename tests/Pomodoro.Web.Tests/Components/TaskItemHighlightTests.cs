using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class TaskItemHighlightTests : TestContext
{
    public TaskItemHighlightTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static TaskItem NewTask(string name = "Sub task") =>
        new() { Id = Guid.NewGuid(), Name = name, CreatedAt = DateTime.UtcNow };

    private int ScrollCallCount() =>
        JSInterop.Invocations.Count(i => i.Identifier == "taskScrollInterop.scrollIntoViewIfNeeded");

    [Fact]
    public void IsNewlyAddedTrue_AppendsTaskRowNewClass()
    {
        // Act
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, NewTask())
            .Add(x => x.IsNewlyAdded, true));

        // Assert
        cut.Markup.Should().Contain("task-row--new");
    }

    [Fact]
    public void IsNewlyAddedFalse_NoTaskRowNewClass()
    {
        // Act
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, NewTask())
            .Add(x => x.IsNewlyAdded, false));

        // Assert
        cut.Markup.Should().NotContain("task-row--new");
    }

    [Fact]
    public void OnAfterRenderAsync_IsNewlyAdded_InvokesScrollInteropOnce()
    {
        // Act
        RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, NewTask())
            .Add(x => x.IsNewlyAdded, true));

        // Assert
        ScrollCallCount().Should().Be(1);
    }

    [Fact]
    public void OnAfterRenderAsync_AlreadyScrolled_DoesNotInvokeAgain()
    {
        // Act
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, NewTask())
            .Add(x => x.IsNewlyAdded, true));
        cut.Render();

        // Assert
        ScrollCallCount().Should().Be(1);
    }

    [Fact]
    public void OnAfterRenderAsync_NotNewlyAdded_DoesNotInvokeScroll()
    {
        // Act
        RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, NewTask())
            .Add(x => x.IsNewlyAdded, false));

        // Assert
        ScrollCallCount().Should().Be(0);
    }

    [Fact]
    public void HandleDemote_OpensPicker_InvokesScrollInteropOnce()
    {
        // Arrange - task with siblings renders the "Demote" button.
        var task = NewTask("Task A");
        var sibling = NewTask("Task B");
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.Depth, 0)
            .Add(x => x.Siblings, new List<TaskItem> { sibling }));

        // Act - open the picker.
        cut.Find("button[aria-label=\"Demote\"]").Click();

        // Assert
        cut.Find(".demote-picker").Should().NotBeNull();
        ScrollCallCount().Should().Be(1);
    }

    [Fact]
    public void CancelDemote_AfterOpen_DoesNotInvokeScrollAgain()
    {
        // Arrange - picker opened once (consumes the scroll request).
        var task = NewTask("Task A");
        var sibling = NewTask("Task B");
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.Depth, 0)
            .Add(x => x.Siblings, new List<TaskItem> { sibling }));
        cut.Find("button[aria-label=\"Demote\"]").Click();
        var countAfterOpen = ScrollCallCount();

        // Act - close via Cancel.
        cut.Find(".demote-pick-cancel").Click();

        // Assert
        ScrollCallCount().Should().Be(countAfterOpen);
    }
}
