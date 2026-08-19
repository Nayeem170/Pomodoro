using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TaskItemKeyboardTests : TestContext
{
    private static TaskItem NewTask(string name, int sortOrder = 0, DateTime? createdAt = null, Guid? parentId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        SortOrder = sortOrder,
        CreatedAt = createdAt ?? DateTime.UtcNow,
        ParentTaskId = parentId
    };

    private static KeyboardEventArgs AltKey(string key) => new() { Key = key, AltKey = true };

    private static KeyboardEventArgs PlainKey(string key) => new() { Key = key };

    private IRenderedComponent<TaskItemComponent> RenderRow(
        TaskItem item,
        IReadOnlyList<TaskItem> group,
        bool isReorderable,
        EventCallback<ReorderRequest> onReorder,
        int depth = 0) =>
        RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, item)
            .Add(p => p.Depth, depth)
            .Add(p => p.ReorderGroup, group)
            .Add(p => p.IsReorderable, isReorderable)
            .Add(p => p.OnReorder, onReorder));

    [Fact]
    public async Task HandleKeyDown_AltArrowUp_InvokesReorderWithPreviousSiblingBefore()
    {
        // Arrange
        var first = NewTask("First", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var middle = NewTask("Middle", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var last = NewTask("Last", sortOrder: 3000, createdAt: new DateTime(2026, 1, 3));
        var group = new List<TaskItem> { first, middle, last };
        ReorderRequest? received = null;

        var cut = RenderRow(middle, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowUp"));

        // Assert
        received.Should().NotBeNull();
        received!.TaskId.Should().Be(middle.Id);
        received.TargetId.Should().Be(first.Id);
        received.InsertBefore.Should().BeTrue();
    }

    [Fact]
    public async Task HandleKeyDown_AltArrowDown_InvokesReorderWithNextSiblingAfter()
    {
        // Arrange
        var first = NewTask("First", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var middle = NewTask("Middle", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var last = NewTask("Last", sortOrder: 3000, createdAt: new DateTime(2026, 1, 3));
        var group = new List<TaskItem> { first, middle, last };
        ReorderRequest? received = null;

        var cut = RenderRow(middle, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowDown"));

        // Assert
        received.Should().NotBeNull();
        received!.TaskId.Should().Be(middle.Id);
        received.TargetId.Should().Be(last.Id);
        received.InsertBefore.Should().BeFalse();
    }

    [Fact]
    public async Task HandleKeyDown_AltArrowUpOnFirstRow_DoesNotInvoke()
    {
        // Arrange
        var first = NewTask("First", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var second = NewTask("Second", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var group = new List<TaskItem> { first, second };
        ReorderRequest? received = null;

        var cut = RenderRow(first, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowUp"));

        // Assert
        received.Should().BeNull("there is no sibling above the first row");
    }

    [Fact]
    public async Task HandleKeyDown_AltArrowDownOnLastRow_DoesNotInvoke()
    {
        // Arrange
        var first = NewTask("First", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var last = NewTask("Last", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var group = new List<TaskItem> { first, last };
        ReorderRequest? received = null;

        var cut = RenderRow(last, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowDown"));

        // Assert
        received.Should().BeNull("there is no sibling below the last row");
    }

    [Fact]
    public async Task HandleKeyDown_NotReorderable_DoesNotInvoke()
    {
        // Arrange
        var a = NewTask("A", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var b = NewTask("B", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var group = new List<TaskItem> { a, b };
        ReorderRequest? received = null;

        var cut = RenderRow(a, group, isReorderable: false,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowUp"));
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowDown"));

        // Assert
        received.Should().BeNull("non-reorderable groups reject keyboard reorder like drag");
    }

    [Fact]
    public async Task HandleKeyDown_WhileInlineEditing_DoesNotInvoke()
    {
        // Arrange
        var parent = NewTask("Parent");
        var a = NewTask("A", createdAt: new DateTime(2026, 1, 1), parentId: parent.Id);
        var b = NewTask("B", createdAt: new DateTime(2026, 1, 2), parentId: parent.Id);
        var group = new List<TaskItem> { a, b };
        ReorderRequest? received = null;

        var cut = RenderRow(b, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)),
            depth: 1);
        await cut.InvokeAsync(() => cut.Find(".task-text").DoubleClick());
        cut.FindAll(".task-text-input").Count.Should().Be(1,
            "sanity: the row is in inline-edit mode");

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowUp"));

        // Assert
        received.Should().BeNull("keyboard reorder is disabled during inline edit");
    }

    [Fact]
    public async Task HandleKeyDown_ArrowWithoutAlt_DoesNotReorder()
    {
        // Arrange
        var a = NewTask("A", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var b = NewTask("B", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var group = new List<TaskItem> { a, b };
        ReorderRequest? received = null;

        var cut = RenderRow(b, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", PlainKey("ArrowUp"));
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", PlainKey("ArrowDown"));

        // Assert
        received.Should().BeNull("Arrow keys without Alt have no reorder binding");
    }

    [Fact]
    public async Task HandleKeyDown_EnterWithoutAlt_StillSelects()
    {
        // Arrange
        var a = NewTask("A", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var b = NewTask("B", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));
        var group = new List<TaskItem> { a, b };
        Guid? selectedId = null;

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, b)
            .Add(p => p.ReorderGroup, group)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.OnSelect, new EventCallback<Guid>(null, (Action<Guid>)(id => selectedId = id))));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", PlainKey("Enter"));

        // Assert
        selectedId.Should().Be(b.Id, "Enter without Alt keeps its existing select behavior");
    }

    [Fact]
    public async Task HandleKeyDown_SingleMemberGroup_DoesNotInvokeOrThrow()
    {
        // Arrange
        var solo = NewTask("Solo");
        var group = new List<TaskItem> { solo };
        ReorderRequest? received = null;

        var cut = RenderRow(solo, group, isReorderable: true,
            new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r)));

        // Act
        await cut.Find(".task-row").TriggerEventAsync("onkeydown", AltKey("ArrowUp"));

        // Assert
        received.Should().BeNull("a single-member group has no neighbor");
    }
}
