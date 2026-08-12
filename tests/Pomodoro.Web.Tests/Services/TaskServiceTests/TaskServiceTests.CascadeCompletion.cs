using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    private void SetupTaskStore(params TaskItem[] tasks)
    {
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(tasks.ToList());
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
    }

    [Fact]
    public async Task CompleteTaskAsync_LastSubtaskCompleted_AutoCompletesParent()
    {
        // Arrange - parent with two incomplete subtasks
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        var child1 = CreateSampleTask(name: "Child 1", isCompleted: false);
        child1.ParentTaskId = parentId;
        var child2 = CreateSampleTask(name: "Child 2", isCompleted: false);
        child2.ParentTaskId = parentId;

        SetupTaskStore(parent, child1, child2);
        var service = CreateService();
        await service.InitializeAsync();

        await service.CompleteTaskAsync(child1.Id);

        // Act - complete the LAST subtask
        await service.CompleteTaskAsync(child2.Id);

        // Assert - parent must auto-complete now that all subtasks are done
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_LastSubtaskCompleted_SetsParentCompletedAt()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        var child1 = CreateSampleTask(name: "Child 1", isCompleted: false);
        child1.ParentTaskId = parentId;
        var child2 = CreateSampleTask(name: "Child 2", isCompleted: false);
        child2.ParentTaskId = parentId;

        SetupTaskStore(parent, child1, child2);
        var service = CreateService();
        await service.InitializeAsync();

        await service.CompleteTaskAsync(child1.Id);

        // Act
        await service.CompleteTaskAsync(child2.Id);

        // Assert - REQ-2 CompletedAt
        service.AllTasks.First(t => t.Id == parentId).CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteTaskAsync_LastSubtaskCompleted_CascadesThroughAncestors()
    {
        // Arrange - grandparent -> parent -> subtask
        var grandparentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var grandparent = CreateSampleTask(id: grandparentId, name: "Grandparent", isCompleted: false);
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        parent.ParentTaskId = grandparentId;
        var sub = CreateSampleTask(name: "Sub", isCompleted: false);
        sub.ParentTaskId = parentId;

        SetupTaskStore(grandparent, parent, sub);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - complete the only subtask
        await service.CompleteTaskAsync(sub.Id);

        // Assert - REQ-3 both ancestors auto-complete
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeTrue();
        service.AllTasks.First(t => t.Id == grandparentId).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithRemainingIncompleteSubtask_LeavesParentIncomplete()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        var child1 = CreateSampleTask(name: "Child 1", isCompleted: false);
        child1.ParentTaskId = parentId;
        var child2 = CreateSampleTask(name: "Child 2", isCompleted: false);
        child2.ParentTaskId = parentId;

        SetupTaskStore(parent, child1, child2);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - complete only ONE of two subtasks
        await service.CompleteTaskAsync(child1.Id);

        // Assert - REQ-4 parent stays incomplete
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteTaskAsync_SoftDeletedSibling_DoesNotBlockParentAutoComplete()
    {
        // Arrange - parent, one live subtask, one soft-deleted incomplete subtask
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        var liveChild = CreateSampleTask(name: "Live", isCompleted: false);
        liveChild.ParentTaskId = parentId;
        var deletedChild = CreateSampleTask(name: "Deleted", isCompleted: false);
        deletedChild.ParentTaskId = parentId;
        deletedChild.IsDeleted = true;

        SetupTaskStore(parent, liveChild, deletedChild);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - complete the live subtask; deleted sibling must be ignored
        await service.CompleteTaskAsync(liveChild.Id);

        // Assert - REQ-4 parent auto-completes (deleted subtask does not count)
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_GoogleLinkedIncompleteSubtask_BlocksParentAutoComplete()
    {
        // Arrange - parent backed by a Google task; one live subtask (local link),
        // one subtask linked ONLY via GoogleParentTaskId (unresolved local link).
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        parent.GoogleTaskId = "g1";
        var localChild = CreateSampleTask(name: "Local", isCompleted: false);
        localChild.ParentTaskId = parentId;
        var googleChild = CreateSampleTask(name: "Google", isCompleted: false);
        googleChild.GoogleParentTaskId = "g1";

        SetupTaskStore(parent, localChild, googleChild);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - complete the locally-linked subtask
        await service.CompleteTaskAsync(localChild.Id);

        // Assert - REQ-5 the Google-linked incomplete subtask is still counted
        // (dual-edge predicate), so the parent must NOT auto-complete
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task UncompleteTaskAsync_WhenSubtaskUncompleted_AutoUncompletesParent()
    {
        // Arrange - fully complete parent + two complete subtasks
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: true);
        parent.CompletedAt = DateTime.UtcNow;
        var child1 = CreateSampleTask(name: "Child 1", isCompleted: true);
        child1.ParentTaskId = parentId;
        var child2 = CreateSampleTask(name: "Child 2", isCompleted: true);
        child2.ParentTaskId = parentId;

        SetupTaskStore(parent, child1, child2);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - uncomplete one subtask
        await service.UncompleteTaskAsync(child1.Id);

        // Assert - REQ-6 parent auto-uncompletes
        var stored = service.AllTasks.First(t => t.Id == parentId);
        stored.IsCompleted.Should().BeFalse();
        stored.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UncompleteTaskAsync_CascadesThroughAncestors()
    {
        // Arrange - fully complete 3-level chain
        var grandparentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var grandparent = CreateSampleTask(id: grandparentId, name: "Grandparent", isCompleted: true);
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: true);
        parent.ParentTaskId = grandparentId;
        var leaf = CreateSampleTask(name: "Leaf", isCompleted: true);
        leaf.ParentTaskId = parentId;

        SetupTaskStore(grandparent, parent, leaf);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - uncomplete the leaf
        await service.UncompleteTaskAsync(leaf.Id);

        // Assert - REQ-7 both ancestors auto-uncomplete
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeFalse();
        service.AllTasks.First(t => t.Id == grandparentId).IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task UncompleteTaskAsync_LeavesCompletedSiblingsAndDescendantsUntouched()
    {
        // Arrange - parent(complete) with child1(complete, has its own complete descendant)
        // and child2(complete sibling)
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: true);
        var child1 = CreateSampleTask(name: "Child 1", isCompleted: true);
        child1.ParentTaskId = parentId;
        var child2 = CreateSampleTask(name: "Child 2", isCompleted: true);
        child2.ParentTaskId = parentId;
        var grandchild = CreateSampleTask(name: "Grandchild", isCompleted: true);
        grandchild.ParentTaskId = child1.Id;

        SetupTaskStore(parent, child1, child2, grandchild);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - uncomplete child1 only
        await service.UncompleteTaskAsync(child1.Id);

        // Assert - REQ-8 sibling child2 and descendant grandchild stay complete;
        // propagation is strictly upward so parent uncompletes but not the sibling/descendant
        service.AllTasks.First(t => t.Id == parentId).IsCompleted.Should().BeFalse();
        service.AllTasks.First(t => t.Id == child2.Id).IsCompleted.Should().BeTrue();
        service.AllTasks.First(t => t.Id == grandchild.Id).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringAncestorAutoCompleted_StampsLastCompletedDate()
    {
        // Arrange - recurring parent with one subtask
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, name: "Parent", isCompleted: false);
        parent.Repeat = new RepeatRule { Type = RepeatType.Daily };
        var child = CreateSampleTask(name: "Child", isCompleted: false);
        child.ParentTaskId = parentId;

        SetupTaskStore(parent, child);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - complete the subtask, auto-completing the recurring parent
        await service.CompleteTaskAsync(child.Id);

        // Assert - REQ-9 recurrence cursor stamped, same as a manual completion
        var stored = service.AllTasks.First(t => t.Id == parentId);
        stored.IsCompleted.Should().BeTrue();
        stored.Repeat!.LastCompletedDate.Should().NotBeNull();
        stored.Repeat!.LastCompletedDate!.Value.Date.Should().Be(DateTime.Now.Date);
    }

    [Fact]
    public async Task CompleteTaskAsync_CyclicParentChain_TerminatesWithoutHanging()
    {
        // Arrange - malformed cyclic chain: A.ParentTaskId = B, B.ParentTaskId = A,
        // plus a clean leaf C under A. B is already complete so A is completable.
        var aId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        var a = CreateSampleTask(id: aId, name: "A", isCompleted: false);
        a.ParentTaskId = bId;
        var b = CreateSampleTask(id: bId, name: "B", isCompleted: true);
        b.ParentTaskId = aId;
        var c = CreateSampleTask(name: "C", isCompleted: false);
        c.ParentTaskId = aId;

        SetupTaskStore(a, b, c);
        var service = CreateService();
        await service.InitializeAsync();

        // Act - completing C walks C -> A and must terminate despite A<->B cycle
        await service.CompleteTaskAsync(c.Id);

        // Assert - REQ-10 terminates (the await returning proves no infinite loop);
        // A auto-completes, B (already complete) is left as-is
        service.AllTasks.First(t => t.Id == aId).IsCompleted.Should().BeTrue();
        service.AllTasks.First(t => t.Id == bId).IsCompleted.Should().BeTrue();
    }
}
