using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class TaskListReorderTests : TestContext
{
    public TaskListReorderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static TaskItem NewTask(string name, int sortOrder = 0, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        SortOrder = sortOrder,
        CreatedAt = createdAt ?? DateTime.UtcNow
    };

    [Fact]
    public void IsReorderable_FalseWhenAnySiblingIsGoogleTask()
    {
        var local = NewTask("Local");
        var google = NewTask("Google");
        google.GoogleTaskId = "g1";

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { local, google })
            .Add(p => p.CurrentTaskId, null));

        var rows = cut.FindAll(".task-row");
        rows.Count.Should().Be(2);
        rows.Should().OnlyContain(r => r.GetAttribute("draggable") == "false");
    }

    [Fact]
    public void IsReorderable_GoogleSubtreeDoesNotBlockLocalRoots()
    {
        var a = NewTask("A");
        var b = NewTask("B");
        var anchor = NewTask("L");
        var googleParent = NewTask("GP");
        googleParent.ParentTaskId = anchor.Id;
        googleParent.GoogleTaskId = "gp";
        var googleChild = NewTask("GS");
        googleChild.GoogleTaskId = "gs";
        googleChild.GoogleParentTaskId = "gp";

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { a, b, anchor, googleParent, googleChild })
            .Add(p => p.CurrentTaskId, null));

        var draggableStates = cut.FindAll(".task-row")
            .GroupBy(r => r.QuerySelector(".task-text")!.TextContent)
            .ToDictionary(g => g.Key, g => g.First().GetAttribute("draggable"));

        draggableStates["A"].Should().Be("true");
        draggableStates["B"].Should().Be("true");
        draggableStates["L"].Should().Be("true");
        draggableStates["GP"].Should().Be("false",
            "single-member local-parent group under L is not reorderable");
        draggableStates["GS"].Should().Be("false",
            "Google group members are never reorderable in phase 1");
    }

    [Fact]
    public void IsReorderable_FalseForSingleMemberGroup()
    {
        var solo = NewTask("Solo");

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { solo })
            .Add(p => p.CurrentTaskId, null));

        cut.Find(".task-row").GetAttribute("draggable").Should().Be("false");
    }

    [Fact]
    public void IsReorderable_TrueForAllLocalGroupWithMultipleMembers()
    {
        var a = NewTask("A");
        var b = NewTask("B");

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { a, b })
            .Add(p => p.CurrentTaskId, null));

        cut.FindAll(".task-row").Should()
            .OnlyContain(r => r.GetAttribute("draggable") == "true");
    }

    [Fact]
    public void RootOrder_FollowsSortOrderThenNewestFirstTiebreak()
    {
        var late = NewTask("Late", sortOrder: 2000, createdAt: new DateTime(2026, 1, 1));
        var early = NewTask("Early", sortOrder: 1000, createdAt: new DateTime(2026, 1, 5));
        var tie = NewTask("Tie", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2));

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { late, early, tie })
            .Add(p => p.CurrentTaskId, null));

        cut.FindAll(".task-row")
            .Select(r => r.QuerySelector(".task-text")?.TextContent)
            .Should().Equal(["Early", "Tie", "Late"],
                "roots sort by SortOrder ascending; ties break to newest-first (legacy root order)");
    }

    [Fact]
    public void ChildOrder_FollowsSortOrderThenCreatedAt()
    {
        var parent = NewTask("Parent", createdAt: new DateTime(2025, 12, 31));
        var childB = NewTask("ChildB", sortOrder: 1000, createdAt: new DateTime(2026, 1, 2));
        childB.ParentTaskId = parent.Id;
        var childA = NewTask("ChildA", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        childA.ParentTaskId = parent.Id;
        var childC = NewTask("ChildC", sortOrder: 500, createdAt: new DateTime(2026, 1, 3));
        childC.ParentTaskId = parent.Id;

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { parent, childB, childA, childC })
            .Add(p => p.CurrentTaskId, null));

        cut.FindAll(".task-row")
            .Select(r => r.QuerySelector(".task-text")?.TextContent)
            .Should().Equal("Parent", "ChildC", "ChildA", "ChildB");
    }

    [Fact]
    public async Task OnReorder_BubblesFromChildRowToTaskListCallback()
    {
        var a = NewTask("A", createdAt: new DateTime(2026, 1, 2));
        var b = NewTask("B", createdAt: new DateTime(2026, 1, 1));
        ReorderRequest? received = null;

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { a, b })
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskReorder, new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r))));

        var rows = cut.FindAll(".task-row");
        rows[0].QuerySelector(".task-text")!.TextContent.Should().Be("A",
            "A is newest so it renders first among untouched roots");
        await rows[0].TriggerEventAsync("ondragstart", new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        var targetRow = cut.FindAll(".task-row")[1];
        targetRow.QuerySelector(".drop-zone.bot").Should().NotBeNull();
        await targetRow.QuerySelector(".drop-zone.bot")!
            .TriggerEventAsync("ondrop", new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        received.Should().NotBeNull();
        received!.TaskId.Should().Be(a.Id);
        received.TargetId.Should().Be(b.Id);
        received.InsertBefore.Should().BeFalse();
    }

    [Fact]
    public async Task OnToggleFollowParent_BubblesFromChildRowToTaskListCallback()
    {
        var parent = NewTask("Parent");
        var child = NewTask("Child");
        child.ParentTaskId = parent.Id;
        Guid? capturedId = null;

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { parent, child })
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnToggleFollowParent, new EventCallback<Guid>(null, (Action<Guid>)(id => capturedId = id))));

        var childRow = cut.FindAll(".task-row")
            .Single(r => r.QuerySelector(".task-text")!.TextContent == "Child");
        await cut.InvokeAsync(() => childRow.QuerySelector(".follow-parent")!.Click());

        capturedId.Should().Be(child.Id);
    }

    [Fact]
    public async Task DragStartShowsZonesOnOtherRows_AndDragEndClearsThem()
    {
        var a = NewTask("A");
        var b = NewTask("B");

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem> { a, b })
            .Add(p => p.CurrentTaskId, null));

        cut.FindAll(".drop-zone").Should().BeEmpty();

        await cut.FindAll(".task-row")[0].TriggerEventAsync("ondragstart",
            new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        cut.FindAll(".drop-zone").Count.Should().Be(2,
            "the non-dragged row shows top and bottom drop zones");

        await cut.FindAll(".task-row")[0].TriggerEventAsync("ondragend",
            new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        cut.FindAll(".drop-zone").Should().BeEmpty();
    }
}
