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
    public async Task DemoteTaskAsync_DemotesUnderTargetSibling()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.CreatedAt = new DateTime(2026, 1, 2);
        var c = CreateSampleTask(name: "C");
        c.CreatedAt = new DateTime(2026, 1, 3);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b, c });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(c.Id, a.Id);

        var cResult = service.AllTasks.First(t => t.Name == "C");
        cResult.ParentTaskId.Should().Be(a.Id,
            "demoting C under A must make C a child of A");
    }

    [Fact]
    public async Task DemoteTaskAsync_SameIdDoesNothing()
    {
        var a = CreateSampleTask(name: "A");
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(a.Id, a.Id);

        var aResult = service.AllTasks.First(t => t.Name == "A");
        aResult.ParentTaskId.Should().BeNull("demoting a task onto itself must be a no-op");
    }

    [Fact]
    public async Task DemoteTaskAsync_NonSiblingDoesNothing()
    {
        var root = CreateSampleTask(name: "Root");
        root.CreatedAt = new DateTime(2026, 1, 1);
        var child = CreateSampleTask(name: "Child");
        child.ParentTaskId = root.Id;
        child.CreatedAt = new DateTime(2026, 1, 2);
        var other = CreateSampleTask(name: "Other");
        other.CreatedAt = new DateTime(2026, 1, 3);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, child, other });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(other.Id, child.Id);

        var otherResult = service.AllTasks.First(t => t.Name == "Other");
        otherResult.ParentTaskId.Should().BeNull(
            "demoting onto a non-sibling (different parent) must be a no-op");
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

        await service.DemoteTaskAsync(target.Id, sibling.Id);

        var targetResult = service.AllTasks.First(t => t.Name == "Target");
        targetResult.ParentTaskId.Should().Be(root.Id,
            "demote must be a no-op when the moved subtree would exceed MaxSubtaskDepth");
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

        await service.DemoteTaskAsync(b.Id, a.Id);

        var bResult = service.AllTasks.First(t => t.Name == "B");
        bResult.ParentTaskId.Should().Be(a.Id);
        bResult.TotalFocusMinutes.Should().Be(45);
        bResult.PomodoroCount.Should().Be(3);
    }

    [Fact]
    public async Task DemoteTaskAsync_UnknownTaskIdDoesNothing()
    {
        var a = CreateSampleTask(name: "A");
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.DemoteTaskAsync(Guid.NewGuid(), a.Id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DemoteTaskAsync_DemotesSubtreeUnderTargetSibling()
    {
        var a = CreateSampleTask(name: "A");
        a.CreatedAt = new DateTime(2026, 1, 1);
        var b = CreateSampleTask(name: "B");
        b.ParentTaskId = a.Id;
        b.CreatedAt = new DateTime(2026, 1, 2);
        var bChild = CreateSampleTask(name: "BChild");
        bChild.ParentTaskId = b.Id;
        bChild.CreatedAt = new DateTime(2026, 1, 3);
        var c = CreateSampleTask(name: "C");
        c.ParentTaskId = a.Id;
        c.CreatedAt = new DateTime(2026, 1, 4);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b, bChild, c });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.DemoteTaskAsync(b.Id, c.Id);

        var bResult = service.AllTasks.First(t => t.Name == "B");
        bResult.ParentTaskId.Should().Be(c.Id,
            "demoting B under C must make B a child of C");

        var bChildResult = service.AllTasks.First(t => t.Name == "BChild");
        bChildResult.ParentTaskId.Should().Be(b.Id,
            "B's own children must stay under B");
    }
}
