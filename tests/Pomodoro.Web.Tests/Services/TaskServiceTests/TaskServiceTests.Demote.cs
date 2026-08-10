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
    [Fact]
    public async Task DemoteTaskAsync_SecondRootBecomesChildOfFirst()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(b.Id);

        var bResult = service.AllTasks.First(t => t.Name == "B");
        bResult.ParentTaskId.Should().Be(a.Id,
            "demoting B (second root) must make it a child of A (first root)");
    }

    [Fact]
    public async Task DemoteTaskAsync_FirstRootDoesNothing()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(a.Id);

        var aResult = service.AllTasks.First(t => t.Name == "A");
        aResult.ParentTaskId.Should().BeNull(
            "demoting the first root (no previous sibling) must be a no-op");
    }

    [Fact]
    public async Task DemoteTaskAsync_SecondChildBecomesChildOfFirstSibling()
    {
        var root = CreateSampleTask(name: "Root");
        root.CreatedAt = new DateTime(2026, 1, 1);
        var child1 = CreateSampleTask(name: "Child1");
        child1.ParentTaskId = root.Id;
        child1.CreatedAt = new DateTime(2026, 1, 2);
        var child2 = CreateSampleTask(name: "Child2");
        child2.ParentTaskId = root.Id;
        child2.CreatedAt = new DateTime(2026, 1, 3);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, child1, child2 });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(child2.Id);

        var child2Result = service.AllTasks.First(t => t.Name == "Child2");
        child2Result.ParentTaskId.Should().Be(child1.Id,
            "demoting Child2 must make it a child of Child1 (its previous sibling)");
    }

    [Fact]
    public async Task DemoteTaskAsync_ExceedingMaxDepthDoesNothing()
    {
        var root = CreateSampleTask(name: "Root");
        root.CreatedAt = new DateTime(2026, 1, 1);

        var sibling = CreateSampleTask(name: "Sibling");
        sibling.ParentTaskId = root.Id;
        sibling.CreatedAt = new DateTime(2026, 1, 2);

        var target = CreateSampleTask(name: "Target");
        target.ParentTaskId = root.Id;
        target.CreatedAt = new DateTime(2026, 1, 3);

        var tc1 = CreateSampleTask(name: "TC1");
        tc1.ParentTaskId = target.Id;
        tc1.CreatedAt = new DateTime(2026, 1, 4);

        var tc2 = CreateSampleTask(name: "TC2");
        tc2.ParentTaskId = tc1.Id;
        tc2.CreatedAt = new DateTime(2026, 1, 5);

        var tc3 = CreateSampleTask(name: "TC3");
        tc3.ParentTaskId = tc2.Id;
        tc3.CreatedAt = new DateTime(2026, 1, 6);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, sibling, target, tc1, tc2, tc3 });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(target.Id);

        var targetResult = service.AllTasks.First(t => t.Name == "Target");
        targetResult.ParentTaskId.Should().Be(root.Id,
            "demote must be a no-op when the moved subtree (3 levels deep) plus sibling depth (1) + 1 would exceed MaxSubtaskDepth (4)");
    }

    [Fact]
    public async Task DemoteTaskAsync_PreservesTaskData()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        b.TotalFocusMinutes = 45;
        b.PomodoroCount = 3;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(b.Id);

        var bResult = service.AllTasks.First(t => t.Name == "B");
        bResult.ParentTaskId.Should().Be(a.Id);
        bResult.TotalFocusMinutes.Should().Be(45);
        bResult.PomodoroCount.Should().Be(3);
    }

    [Fact]
    public async Task DemoteTaskAsync_UnknownTaskIdDoesNothing()
    {
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.DemoteTaskAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DemoteTaskAsync_DemotesToImmediatePreviousSiblingNotOldest()
    {
        var root = CreateSampleTask(name: "Root");
        root.CreatedAt = new DateTime(2026, 1, 1);
        var childA = CreateSampleTask(name: "A");
        childA.ParentTaskId = root.Id;
        childA.CreatedAt = new DateTime(2026, 1, 2);
        var childB = CreateSampleTask(name: "B");
        childB.ParentTaskId = root.Id;
        childB.CreatedAt = new DateTime(2026, 1, 3);
        var childC = CreateSampleTask(name: "C");
        childC.ParentTaskId = root.Id;
        childC.CreatedAt = new DateTime(2026, 1, 4);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, childA, childB, childC });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(childC.Id);

        var cResult = service.AllTasks.First(t => t.Name == "C");
        cResult.ParentTaskId.Should().Be(childB.Id,
            "demoting C must make it a child of B (immediate previous sibling), not A (oldest sibling)");
    }
}
