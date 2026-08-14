using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class TaskItemDragTests : TestContext
{
    private static TaskItem NewTask(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public void NonReorderableRow_ShowsNoDropOnlyOnHover()
    {
        var task = NewTask("Local");

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, false)
            .Add(p => p.IsDragActive, true));

        var row = cut.Find(".task-row");
        row.GetAttribute("draggable").Should().Be("false");
        cut.FindAll(".drop-zone").Should().BeEmpty();
        row.ClassName.Should().NotContain("no-drop",
            "the outline is scoped to the hovered row, not every non-reorderable row");

        cut.Find(".no-drop-zone").TriggerEvent("ondragover", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().Contain("no-drop");

        cut.Find(".no-drop-zone").TriggerEvent("ondragleave", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().NotContain("no-drop");
    }

    [Fact]
    public void ReorderableRow_RendersZonesOnlyDuringActiveDrag()
    {
        var task = NewTask("A");

        var idle = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, false));
        idle.Markup.Should().NotContain("drop-zone");

        var active = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, Guid.NewGuid()));
        active.FindAll(".drop-zone").Count.Should().Be(2);
        active.Find(".task-row").GetAttribute("draggable").Should().Be("true");
    }

    [Fact]
    public void DragOverZone_SetsDropIndicatorClass()
    {
        var task = NewTask("A");

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, Guid.NewGuid()));

        cut.Find(".drop-zone.top").TriggerEvent("ondragover", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().Contain("drop-before");

        cut.Find(".drop-zone.bot").TriggerEvent("ondragover", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().Contain("drop-after");
    }

    [Fact]
    public void DragLeaveZone_ClearsDropIndicator()
    {
        var task = NewTask("A");

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, Guid.NewGuid()));

        cut.Find(".drop-zone.top").TriggerEvent("ondragover", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().Contain("drop-before");

        cut.Find(".drop-zone.top").TriggerEvent("ondragleave", new DragEventArgs());
        cut.Find(".task-row").ClassName.Should().NotContain("drop-before");
    }

    [Fact]
    public async Task DropOnTopZone_InvokesReorderWithInsertBeforeTrue()
    {
        var target = NewTask("Target");
        var draggedId = Guid.NewGuid();
        ReorderRequest? received = null;

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, target)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, draggedId)
            .Add(p => p.OnReorder, new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r))));

        await cut.Find(".drop-zone.top").TriggerEventAsync("ondrop", new DragEventArgs());

        received.Should().NotBeNull();
        received!.TaskId.Should().Be(draggedId);
        received.TargetId.Should().Be(target.Id);
        received.InsertBefore.Should().BeTrue();
    }

    [Fact]
    public async Task DropOnBottomZone_InvokesReorderWithInsertBeforeFalse()
    {
        var target = NewTask("Target");
        var draggedId = Guid.NewGuid();
        ReorderRequest? received = null;

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, target)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, draggedId)
            .Add(p => p.OnReorder, new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(r => received = r))));

        await cut.Find(".drop-zone.bot").TriggerEventAsync("ondrop", new DragEventArgs());

        received.Should().NotBeNull();
        received!.InsertBefore.Should().BeFalse();
    }

    [Fact]
    public async Task DragStartAndEnd_InvokeCallbacks()
    {
        var task = NewTask("A");
        var started = 0;
        var ended = 0;

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.OnDragStarted, new EventCallback<Guid>(null, (Action<Guid>)(_ => started++)))
            .Add(p => p.OnDragEnded, new EventCallback(null, (Action)(() => ended++))));

        await cut.Find(".task-row").TriggerEventAsync("ondragstart", new DragEventArgs());
        await cut.Find(".task-row").TriggerEventAsync("ondragend", new DragEventArgs());

        started.Should().Be(1);
        ended.Should().Be(1);
    }

    [Fact]
    public void InlineEditingRow_IsNotDraggable()
    {
        var task = NewTask("A");

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, task)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, false));

        cut.Find(".task-row").GetAttribute("draggable").Should().Be("true");

        cut.Find(".task-text").DoubleClick();

        cut.Find(".task-row").GetAttribute("draggable").Should().Be("false",
            "drag must be disabled while inline editing a task name");
    }

    [Fact]
    public void DropOnSelfRow_IsIgnored()
    {
        var target = NewTask("Target");
        var invoked = false;

        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, target)
            .Add(p => p.IsReorderable, true)
            .Add(p => p.IsDragActive, true)
            .Add(p => p.DraggedTaskId, target.Id)
            .Add(p => p.OnReorder, new EventCallback<ReorderRequest>(null, (Action<ReorderRequest>)(_ => invoked = true))));

        cut.FindAll(".drop-zone").Should().BeEmpty(
            "a row must not present drop zones onto itself");
        invoked.Should().BeFalse();
    }
}
