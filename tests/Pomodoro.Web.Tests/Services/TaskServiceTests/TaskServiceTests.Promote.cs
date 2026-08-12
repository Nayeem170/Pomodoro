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
    public async Task PromoteTaskAsync_DeepSubtask_MovesToGrandparent()
    {
        var rootA = CreateSampleTask(name: "A");
        var childB = CreateSampleTask(name: "B");
        childB.ParentTaskId = rootA.Id;
        var grandchildD = CreateSampleTask(name: "D");
        grandchildD.ParentTaskId = childB.Id;
        var childC = CreateSampleTask(name: "C");
        childC.ParentTaskId = rootA.Id;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { rootA, childB, childC, grandchildD });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(grandchildD.Id);

        var d = service.AllTasks.First(t => t.Name == "D");
        d.ParentTaskId.Should().Be(rootA.Id,
            "promoting D (child of B) must reparent D to A (B's parent)");
    }

    [Fact]
    public async Task PromoteTaskAsync_RootTask_DoesNothing()
    {
        var root = CreateSampleTask(name: "Root");
        root.ParentTaskId = null;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(root.Id);

        var task = service.AllTasks.First(t => t.Name == "Root");
        task.ParentTaskId.Should().BeNull(
            "promoting a root task with no parent must be a no-op");
    }

    [Fact]
    public async Task PromoteTaskAsync_DepthOneSubtask_MovesToRoot()
    {
        var root = CreateSampleTask(name: "Root");
        var subtask = CreateSampleTask(name: "Sub");
        subtask.ParentTaskId = root.Id;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, subtask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(subtask.Id);

        var sub = service.AllTasks.First(t => t.Name == "Sub");
        sub.ParentTaskId.Should().BeNull(
            "promoting a depth-1 subtask (parent is root) must move it to root");
    }

    [Fact]
    public async Task PromoteTaskAsync_PreservesTaskData()
    {
        var root = CreateSampleTask(name: "Root");
        var parent = CreateSampleTask(name: "Parent");
        parent.ParentTaskId = root.Id;
        var task = CreateSampleTask(name: "Task");
        task.ParentTaskId = parent.Id;
        task.TotalFocusMinutes = 30;
        task.PomodoroCount = 2;
        task.IsCompleted = false;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, parent, task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(task.Id);

        var promoted = service.AllTasks.First(t => t.Name == "Task");
        promoted.ParentTaskId.Should().Be(root.Id);
        promoted.TotalFocusMinutes.Should().Be(30);
        promoted.PomodoroCount.Should().Be(2);
    }

    [Fact]
    public async Task PromoteTaskAsync_UnknownTaskId_DoesNothing()
    {
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem>());
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.PromoteTaskAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PromoteTaskAsync_ThreeLevelTree_MovesUpOneLevel()
    {
        var a = CreateSampleTask(name: "A");
        var b = CreateSampleTask(name: "B");
        b.ParentTaskId = a.Id;
        var c = CreateSampleTask(name: "C");
        c.ParentTaskId = a.Id;
        var d = CreateSampleTask(name: "D");
        d.ParentTaskId = b.Id;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { a, b, c, d });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(d.Id);

        var dResult = service.AllTasks.First(t => t.Name == "D");
        dResult.ParentTaskId.Should().Be(a.Id,
            "D promoted from child of B must become child of A (B's parent)");

        var bResult = service.AllTasks.First(t => t.Name == "B");
        bResult.ParentTaskId.Should().Be(a.Id,
            "B must remain child of A");

        var cResult = service.AllTasks.First(t => t.Name == "C");
        cResult.ParentTaskId.Should().Be(a.Id,
            "C must remain child of A");
    }

    [Fact]
    public async Task PromoteTaskAsync_CopiesParentRepeatAndSchedule()
    {
        var root = CreateSampleTask(name: "Root");
        root.Repeat = new RepeatRule { Type = RepeatType.Daily };
        root.ScheduledDate = new DateTime(2026, 1, 15);
        var subtask = CreateSampleTask(name: "Sub");
        subtask.ParentTaskId = root.Id;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, subtask });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.PromoteTaskAsync(subtask.Id);

        var promoted = service.AllTasks.First(t => t.Name == "Sub");
        promoted.ParentTaskId.Should().BeNull("promoted to root");
        promoted.ScheduledDate.Should().Be(new DateTime(2026, 1, 15),
            "promoted task must copy parent's ScheduledDate");
        promoted.Repeat.Should().NotBeNull();
        promoted.Repeat!.Type.Should().Be(RepeatType.Daily,
            "promoted task must copy parent's Repeat type");
    }
}
