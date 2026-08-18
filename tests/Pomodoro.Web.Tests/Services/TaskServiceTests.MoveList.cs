using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public partial class TaskServiceTests
{
    private TaskService CreateService(Mock<IGoogleTasksService> googleTasksService)
    {
        return new TaskService(
            MockTaskRepository.Object,
            MockIndexedDb.Object,
            AppState,
            new ServiceCollection().BuildServiceProvider(),
            Mock.Of<IPomodoroMetaRepository>(),
            googleTasksService.Object,
            Mock.Of<ILogger<TaskService>>()
        );
    }
}

[Trait("Category", "Service")]
public class TaskServiceMoveListTests
{
    private readonly Mock<ITaskRepository> _mockTaskRepo = new();
    private readonly Mock<IIndexedDbService> _mockIndexedDb = new();
    private readonly AppState _appState = new();
    private readonly Mock<IGoogleTasksService> _mockGoogle = new();

    public TaskServiceMoveListTests()
    {
        _mockGoogle.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockTaskRepo.Setup(x => x.GetAllIncludingDeletedAsync()).ReturnsAsync([]);
        _mockIndexedDb.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
    }

    private TaskService CreateSut()
    {
        return new TaskService(
            _mockTaskRepo.Object,
            _mockIndexedDb.Object,
            _appState,
            new ServiceCollection().BuildServiceProvider(),
            Mock.Of<IPomodoroMetaRepository>(),
            _mockGoogle.Object,
            Mock.Of<ILogger<TaskService>>());
    }

    private TaskItem SeedTask(TaskItem task)
    {
        var list = _appState.Tasks.ToList();
        list.Add(task);
        _appState.Tasks = list;
        return task;
    }

    private static GoogleTask GoogleResult(string id, string? parent = null, string etag = "etag-1", string position = "0001")
    {
        return new GoogleTask
        {
            Id = id,
            Title = "Task",
            Status = "needsAction",
            Updated = "2026-08-18T00:00:00Z",
            Parent = parent,
            ETag = etag,
            Position = position
        };
    }

    [Fact]
    public async Task MoveTaskToListAsync_SameList_ReturnsFalseWithoutGoogleCalls()
    {
        // Arrange
        var task = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Local" });
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(task.Id, Constants.TaskLists.LocalPomodoroListId);

        // Assert
        moved.Should().BeFalse();
        _mockGoogle.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MoveTaskToListAsync_ScheduleListTarget_ReturnsFalse()
    {
        // Arrange
        var task = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Local" });
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(task.Id, Constants.TaskLists.ScheduleListId);

        // Assert
        moved.Should().BeFalse();
    }

    [Fact]
    public async Task MoveTaskToListAsync_LocalToGoogle_InsertsRootAndSubtasksWithParent()
    {
        // Arrange
        var root = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Root" });
        var sub = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Sub", ParentTaskId = root.Id });
        _mockGoogle.Setup(x => x.InsertTaskAsync("glist-b", It.IsAny<GoogleTask>(), null))
            .ReturnsAsync(GoogleResult("g-root"));
        _mockGoogle.Setup(x => x.InsertTaskAsync("glist-b", It.IsAny<GoogleTask>(), "g-root"))
            .ReturnsAsync(GoogleResult("g-sub", "g-root"));
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(root.Id, "glist-b");

        // Assert
        moved.Should().BeTrue();
        root.GoogleTaskId.Should().Be("g-root");
        root.GoogleListId.Should().Be("glist-b");
        root.ETag.Should().Be("etag-1");
        sub.GoogleTaskId.Should().Be("g-sub");
        sub.GoogleListId.Should().Be("glist-b");
        sub.GoogleParentTaskId.Should().Be("g-root");
        _mockGoogle.Verify(x => x.InsertTaskAsync("glist-b", It.Is<GoogleTask>(t => t.Title == "Root"), null), Times.Once);
        _mockGoogle.Verify(x => x.InsertTaskAsync("glist-b", It.Is<GoogleTask>(t => t.Title == "Sub"), "g-root"), Times.Once);
        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.Id == root.Id && t.GoogleTaskId == "g-root")), Times.Once);
        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.Id == sub.Id && t.GoogleTaskId == "g-sub")), Times.Once);
    }

    [Fact]
    public async Task MoveTaskToListAsync_GoogleToLocal_DeletesRemoteAndClearsFields()
    {
        // Arrange
        var root = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            GoogleTaskId = "g-root",
            GoogleListId = "glist-a",
            ETag = "old-etag",
            GooglePosition = "0001"
        });
        var sub = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Sub",
            ParentTaskId = root.Id,
            GoogleTaskId = "g-sub",
            GoogleListId = "glist-a",
            GoogleParentTaskId = "g-root",
            ETag = "old-etag-sub"
        });
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(root.Id, Constants.TaskLists.LocalPomodoroListId);

        // Assert
        moved.Should().BeTrue();
        _mockGoogle.Verify(x => x.DeleteTaskAsync("glist-a", "g-root"), Times.Once);
        _mockGoogle.Verify(x => x.DeleteTaskAsync("glist-a", "g-sub"), Times.Once);
        root.GoogleTaskId.Should().BeNull();
        root.GoogleListId.Should().BeNull();
        root.ETag.Should().BeNull();
        root.GooglePosition.Should().BeNull();
        sub.GoogleTaskId.Should().BeNull();
        sub.GoogleParentTaskId.Should().BeNull();
        sub.GoogleListId.Should().BeNull();
    }

    [Fact]
    public async Task MoveTaskToListAsync_GoogleToGoogle_MovesWithTargetListIdAndCascades()
    {
        // Arrange
        var root = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            GoogleTaskId = "g-root",
            GoogleListId = "glist-a"
        });
        var sub = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Sub",
            ParentTaskId = root.Id,
            GoogleTaskId = "g-sub",
            GoogleListId = "glist-a",
            GoogleParentTaskId = "g-root"
        });
        _mockGoogle.Setup(x => x.MoveTaskAsync("glist-a", "g-root", null, "glist-b"))
            .ReturnsAsync(GoogleResult("g-root", null, "etag-root-2", "0001"));
        _mockGoogle.Setup(x => x.MoveTaskAsync("glist-a", "g-sub", "g-root", "glist-b"))
            .ReturnsAsync(GoogleResult("g-sub", "g-root", "etag-sub-2", "0002"));
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(root.Id, "glist-b");

        // Assert
        moved.Should().BeTrue();
        root.GoogleListId.Should().Be("glist-b");
        root.ETag.Should().Be("etag-root-2");
        sub.GoogleListId.Should().Be("glist-b");
        sub.GoogleParentTaskId.Should().Be("g-root");
        sub.ETag.Should().Be("etag-sub-2");
    }

    [Fact]
    public async Task MoveTaskToListAsync_RecurringGoogleTask_Throws()
    {
        // Arrange
        var root = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            GoogleTaskId = "g-root",
            GoogleListId = "glist-a",
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        });
        var sut = CreateSut();

        // Act
        var act = () => sut.MoveTaskToListAsync(root.Id, "glist-b");

        // Assert - the Google API rejects cross-list moves of recurrent tasks; surface the reason.
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(Constants.Messages.RecurringTaskCannotChangeList);
        _mockGoogle.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MoveTaskToListAsync_SubtaskId_ReturnsFalse()
    {
        // Arrange
        var root = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Root" });
        var sub = SeedTask(new TaskItem { Id = Guid.NewGuid(), Name = "Sub", ParentTaskId = root.Id });
        var sut = CreateSut();

        // Act
        var moved = await sut.MoveTaskToListAsync(sub.Id, "glist-b");

        // Assert - subtasks inherit the parent's list; direct moves are rejected.
        moved.Should().BeFalse();
        _mockGoogle.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTaskAsync_PersistsFollowsParentRepeat()
    {
        // Arrange - follow-parent subtask edited to an independent repeat via the panel.
        var task = SeedTask(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Sub",
            ParentTaskId = Guid.NewGuid(),
            FollowsParentRepeat = true
        });
        var sut = CreateSut();
        var edited = task.WithUpdates(c =>
        {
            c.Name = "Sub";
            c.FollowsParentRepeat = false;
            c.Repeat = new RepeatRule { Type = RepeatType.Daily };
        });

        // Act
        await sut.UpdateTaskAsync(edited);

        // Assert
        task.FollowsParentRepeat.Should().BeFalse();
        task.Repeat.Should().NotBeNull();
        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.Id == task.Id && !t.FollowsParentRepeat)), Times.Once);
    }
}
