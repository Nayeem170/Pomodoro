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
    private async Task<TaskService> CreateInitializedServiceAsync(params TaskItem[] tasks)
    {
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(tasks.ToList());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();
        return service;
    }

    [Fact]
    public async Task ReorderTaskAsync_NormalizesAllZeroGroupOnFirstReorder()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);

        var service = await CreateInitializedServiceAsync(a, b, c);

        var result = await service.ReorderTaskAsync(c.Id, a.Id, insertBefore: true);

        result.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(3000,
            "roots normalize newest-first: A is oldest so it lands last");
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "C").SortOrder.Should().Be(2500,
            "midpoint between B(2000) and A(3000) after the move");
        service.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => t.Name)
            .Should().Equal("B", "C", "A");
    }

    [Fact]
    public async Task ReorderTaskAsync_MidpointInsertPersistsOnlyMovedTask()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        a.SortOrder = 1000;
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        b.SortOrder = 2000;
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);
        c.SortOrder = 3000;

        var service = await CreateInitializedServiceAsync(a, b, c);
        MockTaskRepository.Invocations.Clear();

        var result = await service.ReorderTaskAsync(a.Id, c.Id, insertBefore: false);

        result.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(4000,
            "after-last edge insert: last + SortGap");
        MockTaskRepository.Verify(r => r.SaveAsync(It.Is<TaskItem>(t => t.Name == "A")), Times.Once);
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task ReorderTaskAsync_RenumbersGroupWhenGapExhausted()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        a.SortOrder = 1000;
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        b.SortOrder = 1001;
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);
        c.SortOrder = 2000;

        var service = await CreateInitializedServiceAsync(a, b, c);

        var result = await service.ReorderTaskAsync(c.Id, b.Id, insertBefore: true);

        result.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(1000);
        service.AllTasks.First(t => t.Name == "C").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(3000);
        service.Tasks
            .OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt)
            .Select(t => t.Name)
            .Should().Equal("A", "C", "B");
    }

    [Fact]
    public async Task ReorderTaskAsync_InsertBeforeFirstAndAfterLastAreStable()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        a.SortOrder = 1000;
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        b.SortOrder = 2000;

        var service = await CreateInitializedServiceAsync(a, b);

        var moveBeforeFirst = await service.ReorderTaskAsync(b.Id, a.Id, insertBefore: true);
        moveBeforeFirst.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(0);
        service.Tasks.OrderBy(t => t.SortOrder).Select(t => t.Name).Should().Equal("B", "A");

        var moveAfterLast = await service.ReorderTaskAsync(b.Id, a.Id, insertBefore: false);
        moveAfterLast.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(2000);
        service.Tasks.OrderBy(t => t.SortOrder).Select(t => t.Name).Should().Equal("A", "B");
    }

    [Fact]
    public async Task ReorderTaskAsync_SamePositionIsNoOpWithoutWrites()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        a.SortOrder = 1000;
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        b.SortOrder = 2000;

        var service = await CreateInitializedServiceAsync(a, b);
        MockTaskRepository.Invocations.Clear();

        var result = await service.ReorderTaskAsync(a.Id, b.Id, insertBefore: true);

        result.Should().BeTrue("position is unchanged; nothing to write");
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(1000);
    }

    [Fact]
    public async Task ReorderTaskAsync_SameIdReturnsFalse()
    {
        var a = CreateSampleTask(name: "A");
        var service = await CreateInitializedServiceAsync(a);
        MockTaskRepository.Invocations.Clear();

        (await service.ReorderTaskAsync(a.Id, a.Id, true)).Should().BeFalse();
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ReorderTaskAsync_DifferentParentsReturnsFalseWithoutWrites()
    {
        var parent = CreateSampleTask(name: "Parent");
        var childA = CreateSampleTask(name: "ChildA");
        childA.ParentTaskId = parent.Id;
        var rootB = CreateSampleTask(name: "RootB");

        var service = await CreateInitializedServiceAsync(parent, childA, rootB);
        MockTaskRepository.Invocations.Clear();

        (await service.ReorderTaskAsync(childA.Id, rootB.Id, true)).Should().BeFalse();
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ReorderTaskAsync_MissingOrDeletedTaskReturnsFalse()
    {
        var a = CreateSampleTask(name: "A");
        var deleted = CreateSampleTask(name: "Deleted");
        deleted.IsDeleted = true;

        var service = await CreateInitializedServiceAsync(a, deleted);
        MockTaskRepository.Invocations.Clear();

        (await service.ReorderTaskAsync(Guid.NewGuid(), a.Id, true)).Should().BeFalse();
        (await service.ReorderTaskAsync(deleted.Id, a.Id, true)).Should().BeFalse();
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ReorderTaskAsync_GoogleMemberInGroupReturnsFalseWithoutWrites()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var g = CreateSampleTask(name: "G");
        g.CreatedAt = new DateTime(2026, 1, 3);
        g.GoogleTaskId = "google-1";

        var service = await CreateInitializedServiceAsync(a, b, g);
        MockTaskRepository.Invocations.Clear();

        (await service.ReorderTaskAsync(a.Id, b.Id, true)).Should().BeFalse(
            "a group containing any Google task must not be reorderable");
        (await service.ReorderTaskAsync(g.Id, a.Id, true)).Should().BeFalse(
            "dragging a Google task must be rejected");
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task ReorderTaskAsync_ChildGroupNormalizesOldestFirstAndSupportsMidpoint()
    {
        var parent = CreateSampleTask(name: "Parent");
        parent.CreatedAt = new DateTime(2025, 12, 31);
        var c1 = CreateSampleTask(name: "C1");
        c1.CreatedAt = new DateTime(2026, 1, 1);
        c1.ParentTaskId = parent.Id;
        var c2 = CreateSampleTask(name: "C2");
        c2.CreatedAt = new DateTime(2026, 1, 2);
        c2.ParentTaskId = parent.Id;
        var c3 = CreateSampleTask(name: "C3");
        c3.CreatedAt = new DateTime(2026, 1, 3);
        c3.ParentTaskId = parent.Id;

        var service = await CreateInitializedServiceAsync(parent, c1, c2, c3);

        var result = await service.ReorderTaskAsync(c3.Id, c2.Id, insertBefore: true);

        result.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "C1").SortOrder.Should().Be(1000,
            "children normalize oldest-first ascending, unlike roots");
        service.AllTasks.First(t => t.Name == "C2").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "C3").SortOrder.Should().Be(1500,
            "midpoint between C1(1000) and C2(2000)");
        service.Tasks
            .Where(t => t.ParentTaskId == parent.Id)
            .OrderBy(t => t.SortOrder)
            .Select(t => t.Name)
            .Should().Equal(["C1", "C3", "C2"]);
    }

    [Fact]
    public async Task ReorderTaskAsync_RootGroupBehavesLikeChildGroup()
    {
        var parent = CreateSampleTask(name: "Parent");
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);

        var service = await CreateInitializedServiceAsync(parent, a, b);

        var result = await service.ReorderTaskAsync(a.Id, b.Id, insertBefore: true);

        result.Should().BeTrue();
        service.AllTasks.First(t => t.Name == "Parent").SortOrder.Should().Be(1000,
            "roots normalize newest-first: Parent (newest) leads");
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(1500,
            "midpoint between Parent(1000) and B(2000)");
        service.Tasks
            .Where(t => t.ParentTaskId == null)
            .OrderBy(t => t.SortOrder)
            .Select(t => t.Name)
            .Should().Equal("Parent", "A", "B");
    }

    [Fact]
    public async Task ReorderTaskAsync_EqualSortOrderTiebreaksByCreatedAtDeterministically()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);

        var service = await CreateInitializedServiceAsync(a, b);

        var first = service.Tasks.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt).Select(t => t.Name).ToList();
        var second = service.Tasks.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt).Select(t => t.Name).ToList();

        first.Should().Equal("A", "B");
        second.Should().Equal(first);
    }

    [Fact]
    public async Task ReorderTaskAsync_NormalizePathPersistsAllGroupMembers()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);

        var service = await CreateInitializedServiceAsync(a, b, c);
        MockTaskRepository.Invocations.Clear();

        await service.ReorderTaskAsync(c.Id, a.Id, insertBefore: true);

        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Exactly(4),
            "normalize writes all 3 group members, then the move writes the dragged task");
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(3000);
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "C").SortOrder.Should().Be(2500);
    }

    [Fact]
    public async Task ReorderTaskAsync_GoogleSubtreeDoesNotBlockLocalRootReorder()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var anchor = CreateSampleTask(name: "L");
        anchor.CreatedAt = new DateTime(2025, 12, 31);
        var googleParent = CreateSampleTask(name: "GP");
        googleParent.ParentTaskId = anchor.Id;
        googleParent.GoogleTaskId = "gp";
        var googleChild = CreateSampleTask(name: "GS");
        googleChild.GoogleTaskId = "gs";
        googleChild.GoogleParentTaskId = "gp";

        var service = await CreateInitializedServiceAsync(a, b, anchor, googleParent, googleChild);
        MockTaskRepository.Invocations.Clear();

        var result = await service.ReorderTaskAsync(a.Id, b.Id, insertBefore: true);

        result.Should().BeTrue(
            "a Google subtask nested under a Google parent is not a root sibling and must not block local root reorders");
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(0);
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(1000);
        service.AllTasks.First(t => t.Name == "L").SortOrder.Should().Be(3000);
        service.AllTasks.First(t => t.Name == "GS").SortOrder.Should().Be(0,
            "tasks outside the reordered group are untouched");
    }

    [Fact]
    public async Task ReorderTaskAsync_NoOpOnFirstInteractionNormalizesGroup()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);

        var service = await CreateInitializedServiceAsync(a, b, c);
        MockTaskRepository.Invocations.Clear();

        var result = await service.ReorderTaskAsync(b.Id, a.Id, insertBefore: true);

        result.Should().BeTrue("dropping at the current position is a no-op, not a failure");
        service.AllTasks.First(t => t.Name == "C").SortOrder.Should().Be(1000);
        service.AllTasks.First(t => t.Name == "B").SortOrder.Should().Be(2000);
        service.AllTasks.First(t => t.Name == "A").SortOrder.Should().Be(3000);
        MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Exactly(3),
            "first interaction normalizes the group even when the drop changes nothing");
        service.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => t.Name)
            .Should().Equal(["C", "B", "A"],
                "relative order must be unchanged by a no-op drop");
    }

    [Fact]
    public void WithUpdates_CopiesSortOrder()
    {
        var task = CreateSampleTask(name: "A");
        task.SortOrder = 4200;

        var copy = task.WithUpdates(c => c.Name = "Renamed");

        copy.SortOrder.Should().Be(4200,
            "WithUpdates must copy SortOrder or every update/import path silently zeroes it");
    }
}
