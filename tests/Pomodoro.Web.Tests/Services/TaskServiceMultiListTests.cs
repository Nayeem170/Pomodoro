using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class TaskServiceMultiListTests
{
    private readonly Mock<ITaskRepository> _mockTaskRepo = new();
    private readonly Mock<IIndexedDbService> _mockIndexedDb = new();
    private readonly AppState _appState = new();
    private readonly Mock<IServiceProvider> _mockServiceProvider = new();
    private readonly Mock<IPomodoroMetaRepository> _mockSidecarRepo = new();
    private readonly Mock<IGoogleTasksService> _mockGoogleTasksService = new();
    private readonly Mock<ILogger<TaskService>> _mockLogger = new();
    private static readonly System.Reflection.BindingFlags NonPublicInstance =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    public TaskServiceMultiListTests()
    {
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICloudSyncService))).Returns(null!);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        _mockTaskRepo.Setup(x => x.GetAllIncludingDeletedAsync()).ReturnsAsync([]);
        _mockIndexedDb.Setup(x => x.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
    }

    private TaskService CreateSut()
    {
        return new TaskService(
            _mockTaskRepo.Object,
            _mockIndexedDb.Object,
            _appState,
            _mockServiceProvider.Object,
            _mockSidecarRepo.Object,
            _mockGoogleTasksService.Object,
            _mockLogger.Object);
    }

    private static void SetCachedGoogleLists(TaskService sut, List<GoogleTaskList> lists)
    {
        sut.GetType().GetField("_cachedGoogleLists", NonPublicInstance)!.SetValue(sut, lists);
    }

    private static List<GoogleTaskList> GetCachedGoogleLists(TaskService sut)
    {
        return (List<GoogleTaskList>)sut.GetType().GetField("_cachedGoogleLists", NonPublicInstance)!.GetValue(sut)!;
    }

    [Fact]
    public async Task AddTimeToTaskAsync_GoogleTask_WritesToSidecarOnly()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockSidecarRepo.Setup(x => x.GetAsync("gtask-1")).ReturnsAsync((PomodoroMeta?)null);
        _mockSidecarRepo.Setup(x => x.SaveAsync(It.IsAny<PomodoroMeta>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, 25);

        _mockSidecarRepo.Verify(x => x.SaveAsync(It.Is<PomodoroMeta>(m =>
            m.GoogleTaskId == "gtask-1" && m.PomodoroCount == 1 && m.TotalFocusMinutes == 25)), Times.Once);
        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_LocalTask_WritesToRepository()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Local Task",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, 25);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Once);
        _mockSidecarRepo.Verify(x => x.SaveAsync(It.IsAny<PomodoroMeta>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddTimeToTaskAsync_NonPositiveMinutes_ReturnsEarly(int minutes)
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Task", CreatedAt = DateTime.UtcNow };
        _appState.Tasks = [task];

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, minutes);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        _mockSidecarRepo.Verify(x => x.SaveAsync(It.IsAny<PomodoroMeta>()), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_GoogleTask_OnlyUpdatesLastWorkedOn()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow,
            PomodoroCount = 3,
            TotalFocusMinutes = 75
        };
        _appState.Tasks = [task];
        _mockSidecarRepo.Setup(x => x.GetAsync("gtask-1")).ReturnsAsync((PomodoroMeta?)null);
        _mockSidecarRepo.Setup(x => x.SaveAsync(It.IsAny<PomodoroMeta>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, 25);

        var inMemoryTask = _appState.FindTaskById(taskId);
        inMemoryTask.Should().NotBeNull();
        inMemoryTask!.PomodoroCount.Should().Be(3);
        inMemoryTask.TotalFocusMinutes.Should().Be(75);
        inMemoryTask.LastWorkedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task SelectListAsync_PersistsAndNotifies()
    {
        _mockIndexedDb.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        var sut = CreateSut();
        var notified = false;
        sut.OnChange += () => notified = true;

        await sut.SelectListAsync("some-list-id");

        _appState.CurrentListId.Should().Be("some-list-id");
        _mockIndexedDb.Verify(x => x.PutAsync(Constants.Storage.AppStateStore,
            It.Is<AppStateRecord>(r => r.CurrentListId == "some-list-id")), Times.Once);
        notified.Should().BeTrue();
    }

    [Fact]
    public async Task AddTaskAsync_GoogleList_ThrowsNotSupportedException()
    {
        var sut = CreateSut();

        var act = () => sut.AddTaskAsync("task name", "google-list-id");

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task GetTasksForListAsync_HydratesSidecarFromGoogleTasks()
    {
        var task1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task 1",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        var task2 = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task 2",
            GoogleTaskId = "gtask-2",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task1, task2];

        var metaList = new List<PomodoroMeta>
        {
            new("gtask-1", 5, 125, Priority.High),
            new("gtask-2", 2, 50, Priority.Med)
        };
        _mockSidecarRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(metaList);

        var sut = CreateSut();
        var result = await sut.GetTasksForListAsync("glist-1");

        result.Should().HaveCount(2);
        result.First(t => t.GoogleTaskId == "gtask-1").PomodoroCount.Should().Be(5);
        result.First(t => t.GoogleTaskId == "gtask-1").TotalFocusMinutes.Should().Be(125);
        result.First(t => t.GoogleTaskId == "gtask-1").Priority.Should().Be(Priority.High);
        result.First(t => t.GoogleTaskId == "gtask-2").PomodoroCount.Should().Be(2);
        result.First(t => t.GoogleTaskId == "gtask-2").TotalFocusMinutes.Should().Be(50);
        result.First(t => t.GoogleTaskId == "gtask-2").Priority.Should().Be(Priority.Med);
    }

    [Fact]
    public async Task GetTasksForListAsync_ClonesTasksForHydration()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow,
            PomodoroCount = 0,
            TotalFocusMinutes = 0,
            Priority = Priority.None
        };
        _appState.Tasks = [task];

        _mockSidecarRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(
            [new PomodoroMeta("gtask-1", 3, 75, Priority.High)]);

        var sut = CreateSut();
        await sut.GetTasksForListAsync("glist-1");

        var originalTask = _appState.FindTaskById(task.Id);
        originalTask.Should().NotBeNull();
        originalTask!.PomodoroCount.Should().Be(0);
        originalTask.TotalFocusMinutes.Should().Be(0);
        originalTask.Priority.Should().Be(Priority.None);
    }

    [Fact]
    public void TaskLists_IncludesLocalAndScheduleAndGoogleLists()
    {
        var localTask = new TaskItem { Id = Guid.NewGuid(), Name = "Local", CreatedAt = DateTime.UtcNow };
        var scheduledTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Scheduled",
            CreatedAt = DateTime.UtcNow,
            ScheduledDate = DateTime.UtcNow.Date
        };
        var googleTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google",
            CreatedAt = DateTime.UtcNow,
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1"
        };
        _appState.Tasks = [localTask, scheduledTask, googleTask];

        var googleLists = new List<GoogleTaskList> { new() { Id = "glist-1", Title = "My Google List" } };

        var sut = CreateSut();
        SetCachedGoogleLists(sut, googleLists);

        var lists = sut.TaskLists;

        lists.Should().HaveCount(3);
        lists.Should().Contain(l => l.Id == Constants.TaskLists.LocalPomodoroListId && l.Title == "Tasks");
        lists.Should().Contain(l => l.Id == Constants.TaskLists.ScheduleListId && l.Title == "Schedule");
        lists.Should().Contain(l => l.Id == "glist-1" && l.Title == "My Google List" && l.Count == 1);
        lists.First(l => l.Id == Constants.TaskLists.LocalPomodoroListId).Count.Should().Be(1);
        lists.First(l => l.Id == Constants.TaskLists.ScheduleListId).Count.Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_RestoresCurrentListId()
    {
        var record = new AppStateRecord
        {
            Id = Constants.Storage.DefaultSettingsId,
            CurrentListId = "restored-list-id"
        };
        _mockIndexedDb.Setup(x => x.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(record);

        var sut = CreateSut();
        await sut.InitializeAsync();

        _appState.CurrentListId.Should().Be("restored-list-id");
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_NotConnected_ClearsCache()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);

        var sut = CreateSut();
        SetCachedGoogleLists(sut, [new GoogleTaskList { Id = "glist-1", Title = "List" }]);

        await sut.RefreshGoogleListsAsync();

        GetCachedGoogleLists(sut).Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_Connected_UpsertsTasks()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);

        var googleTasks = new List<GoogleTask>
        {
            new() { Id = "remote-1", Title = "Remote Task 1", Status = "needsAction", Updated = "2025-06-20T10:00:00Z" }
        };
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync(googleTasks);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.GoogleTaskId == "remote-1" && t.Name == "Remote Task 1")), Times.Once);

        var cache = GetCachedGoogleLists(sut);
        cache.Should().HaveCount(1);
        cache[0].Id.Should().Be("glist-1");
    }
}
