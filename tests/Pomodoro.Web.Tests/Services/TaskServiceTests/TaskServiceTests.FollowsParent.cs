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
    public async Task AddSubtaskAsync_NewSubtaskDefaultsToFollowsParentTrue()
    {
        var root = CreateSampleTask(name: "Root");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.AddSubtaskAsync("New Sub", root.Id);

        var sub = service.AllTasks.First(t => t.Name == "New Sub");
        sub.FollowsParentRepeat.Should().BeTrue(
            "newly created subtasks must default to FollowsParentRepeat = true");
    }

    [Fact]
    public async Task SetFollowsParentRepeatAsync_SetsValueForSubtask()
    {
        var root = CreateSampleTask(name: "Root");
        var sub = CreateSampleTask(name: "Sub");
        sub.ParentTaskId = root.Id;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, sub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.SetFollowsParentRepeatAsync(sub.Id, false);

        var result = service.AllTasks.First(t => t.Name == "Sub");
        result.FollowsParentRepeat.Should().BeFalse(
            "SetFollowsParentRepeatAsync must update the flag");
    }

    [Fact]
    public async Task SetFollowsParentRepeatAsync_RootTaskDoesNothing()
    {
        var root = CreateSampleTask(name: "Root");

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        await service.SetFollowsParentRepeatAsync(root.Id, false);

        var result = service.AllTasks.First(t => t.Name == "Root");
        result.FollowsParentRepeat.Should().BeTrue(
            "root tasks are unaffected by SetFollowsParentRepeatAsync");
    }

    [Fact]
    public async Task InitializeAsync_RecurringRootReactivates_ResetsFollowsParentSubtask()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: true);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var followingSub = CreateSampleTask(name: "FollowSub", isCompleted: true);
        followingSub.ParentTaskId = root.Id;
        followingSub.FollowsParentRepeat = true;
        followingSub.TotalFocusMinutes = 25;
        followingSub.PomodoroCount = 1;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, followingSub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var subResult = service.AllTasks.First(t => t.Name == "FollowSub");
        subResult.IsCompleted.Should().BeFalse(
            "a FollowsParentRepeat subtask must reset when its root reactivates");
        subResult.TotalFocusMinutes.Should().Be(0);
        subResult.PomodoroCount.Should().Be(0);
    }

    [Fact]
    public async Task InitializeAsync_RecurringRootReactivates_KeepsOneTimeSubtaskCompleted()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: true);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var oneTimeSub = CreateSampleTask(name: "OneTime", isCompleted: true);
        oneTimeSub.ParentTaskId = root.Id;
        oneTimeSub.FollowsParentRepeat = false;
        oneTimeSub.TotalFocusMinutes = 30;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, oneTimeSub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var subResult = service.AllTasks.First(t => t.Name == "OneTime");
        subResult.IsCompleted.Should().BeTrue(
            "a one-time subtask (FollowsParentRepeat=false) must stay completed when root reactivates");
        subResult.TotalFocusMinutes.Should().Be(30);
    }

    [Fact]
    public async Task InitializeAsync_RecurringRootReactivates_CascadesToGrandchildren()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: true);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var child = CreateSampleTask(name: "Child", isCompleted: true);
        child.ParentTaskId = root.Id;
        child.FollowsParentRepeat = true;

        var grandchild = CreateSampleTask(name: "Grandchild", isCompleted: true);
        grandchild.ParentTaskId = child.Id;
        grandchild.FollowsParentRepeat = true;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, child, grandchild });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var childResult = service.AllTasks.First(t => t.Name == "Child");
        childResult.IsCompleted.Should().BeFalse(
            "child with FollowsParentRepeat must reset when root reactivates");

        var grandResult = service.AllTasks.First(t => t.Name == "Grandchild");
        grandResult.IsCompleted.Should().BeFalse(
            "grandchild with FollowsParentRepeat must reset when its parent chain resets");
    }

    [Fact]
    public async Task InitializeAsync_RecurringRootReactivates_StopsAtNonFollowingSubtree()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: true);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var permanent = CreateSampleTask(name: "Permanent", isCompleted: true);
        permanent.ParentTaskId = root.Id;
        permanent.FollowsParentRepeat = false;

        var permanentChild = CreateSampleTask(name: "PermanentChild", isCompleted: true);
        permanentChild.ParentTaskId = permanent.Id;
        permanentChild.FollowsParentRepeat = true;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, permanent, permanentChild });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var permResult = service.AllTasks.First(t => t.Name == "Permanent");
        permResult.IsCompleted.Should().BeTrue(
            "a non-following subtask must stay completed");

        var permChildResult = service.AllTasks.First(t => t.Name == "PermanentChild");
        permChildResult.IsCompleted.Should().BeTrue(
            "children under a non-following subtask must not be touched");
    }

    [Fact]
    public async Task InitializeAsync_AlreadyActiveRoot_ResetsStaleFollowsParentSubtask()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: false);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var staleSub = CreateSampleTask(name: "StaleSub", isCompleted: true);
        staleSub.ParentTaskId = root.Id;
        staleSub.FollowsParentRepeat = true;
        staleSub.CompletedAt = yesterday.AddHours(2);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, staleSub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var subResult = service.AllTasks.First(t => t.Name == "StaleSub");
        subResult.IsCompleted.Should().BeFalse(
            "a FollowsParentRepeat subtask completed before the current cycle must reset " +
            "even when the root already reactivated (stuck state from pre-feature deployment)");
    }

    [Fact]
    public async Task InitializeAsync_AlreadyActiveRoot_PreservesCurrentCycleSubtask()
    {
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: false);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var todaySub = CreateSampleTask(name: "TodaySub", isCompleted: true);
        todaySub.ParentTaskId = root.Id;
        todaySub.FollowsParentRepeat = true;
        todaySub.CompletedAt = today.AddHours(3);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, todaySub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var subResult = service.AllTasks.First(t => t.Name == "TodaySub");
        subResult.IsCompleted.Should().BeTrue(
            "a FollowsParentRepeat subtask completed in the current cycle must stay completed");
    }

    [Fact]
    public async Task InitializeAsync_AlreadyActiveRoot_NullCompletedAtResets()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var root = CreateSampleTask(name: "Root", isCompleted: false);
        root.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = yesterday };

        var oldSub = CreateSampleTask(name: "OldSub", isCompleted: true);
        oldSub.ParentTaskId = root.Id;
        oldSub.FollowsParentRepeat = true;
        oldSub.CompletedAt = null;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { root, oldSub });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        var subResult = service.AllTasks.First(t => t.Name == "OldSub");
        subResult.IsCompleted.Should().BeFalse(
            "a subtask with null CompletedAt (pre-existing data) must reset " +
            "when its root is in an active recurring cycle");
    }
}
