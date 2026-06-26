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
        _mockIndexedDb.Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(true);
        _mockIndexedDb.Setup(x => x.GetAsync<GoogleTasksSettings>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((GoogleTasksSettings?)null);
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

    private static void SetCachedGoogleLists(TaskService sut, List<GoogleListCacheEntry> lists)
    {
        sut.GetType().GetField("_cachedGoogleLists", NonPublicInstance)!.SetValue(sut, lists);
    }

    private static List<GoogleListCacheEntry> GetCachedGoogleLists(TaskService sut)
    {
        return (List<GoogleListCacheEntry>)sut.GetType().GetField("_cachedGoogleLists", NonPublicInstance)!.GetValue(sut)!;
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
    public async Task AddTaskAsync_GoogleList_InsertsViaGoogleTasksService()
    {
        var inserted = new GoogleTask { Id = "gt-1", Title = "task name", ETag = "etag-1", Updated = "2024-01-01T00:00:00Z" };
        _mockGoogleTasksService.Setup(x => x.InsertTaskAsync("google-list-id", It.IsAny<GoogleTask>()))
            .ReturnsAsync(inserted);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.AddTaskAsync("task name", "google-list-id");

        _mockGoogleTasksService.Verify(x => x.InsertTaskAsync("google-list-id", It.IsAny<GoogleTask>()), Times.Once);
        _appState.Tasks.Should().Contain(t =>
            t.GoogleTaskId == "gt-1" &&
            t.GoogleListId == "google-list-id" &&
            t.Name == "task name" &&
            t.ETag == "etag-1");
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

        var googleLists = new List<GoogleListCacheEntry> { new("glist-1", "My Google List", "var(--pomodoro-color)", true) };

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
        SetCachedGoogleLists(sut, [new GoogleListCacheEntry("glist-1", "List", "var(--pomodoro-color)", true)]);

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

    [Fact]
    public async Task RefreshGoogleListsAsync_RemotePresentUndeletesSoftDeletedTask()
    {
        var localDeleted = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Old",
            GoogleTaskId = "remote-1",
            GoogleListId = "glist-1",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localDeleted];

        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync(
            [new GoogleTask { Id = "remote-1", Title = "Restored", Status = "needsAction", Updated = "2025-06-20T10:00:00Z" }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync(
            new List<TaskItem>());
        _mockTaskRepo.Setup(x => x.SaveAsync(It.Is<TaskItem>(t =>
            t.Id == localDeleted.Id && !t.IsDeleted))).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t =>
            t.Id == localDeleted.Id)), Times.Once);

        var inMemory = _appState.FindTaskById(localDeleted.Id);
        inMemory.Should().NotBeNull();
        inMemory!.IsDeleted.Should().BeFalse();
        inMemory.Name.Should().Be("Restored");
        inMemory.GoogleListId.Should().Be("glist-1");
    }

    [Fact]
    public async Task GetTasksForListAsync_LocalList_ReturnsNonGoogleTasks()
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

        var sut = CreateSut();
        var result = await sut.GetTasksForListAsync(Constants.TaskLists.LocalPomodoroListId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(localTask.Id);
    }

    [Fact]
    public async Task GetTasksForListAsync_ScheduleList_ReturnsScheduledTasks()
    {
        var localTask = new TaskItem { Id = Guid.NewGuid(), Name = "Local", CreatedAt = DateTime.UtcNow };
        var scheduledTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Scheduled",
            CreatedAt = DateTime.UtcNow,
            ScheduledDate = DateTime.UtcNow.Date
        };
        _appState.Tasks = [localTask, scheduledTask];

        var sut = CreateSut();
        var result = await sut.GetTasksForListAsync(Constants.TaskLists.ScheduleListId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(scheduledTask.Id);
    }

    [Fact]
    public async Task GetTasksForListAsync_GoogleList_NoGoogleTasks_SkipsSidecarHydration()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Local",
            CreatedAt = DateTime.UtcNow,
            GoogleListId = "glist-1"
        };
        _appState.Tasks = [localTask];

        var sut = CreateSut();
        var result = await sut.GetTasksForListAsync("glist-1");

        result.Should().HaveCount(1);
        _mockSidecarRepo.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_GoogleTask_WithExistingSidecar_IncrementsCounts()
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
        _mockSidecarRepo.Setup(x => x.GetAsync("gtask-1")).ReturnsAsync(
            new PomodoroMeta("gtask-1", 3, 75, Priority.Med));
        _mockSidecarRepo.Setup(x => x.SaveAsync(It.IsAny<PomodoroMeta>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, 25);

        _mockSidecarRepo.Verify(x => x.SaveAsync(It.Is<PomodoroMeta>(m =>
            m.PomodoroCount == 4 && m.TotalFocusMinutes == 100 && m.Priority == Priority.Med)), Times.Once);
    }

    [Fact]
    public async Task AddTimeToTaskAsync_LocalTask_NotFoundInAppState_DoesNotSave()
    {
        var taskId = Guid.NewGuid();
        _appState.Tasks = [];

        var sut = CreateSut();
        await sut.AddTimeToTaskAsync(taskId, 25);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public void CurrentList_ReturnsMatchingList()
    {
        _appState.CurrentListId = "glist-1";
        var googleTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google",
            CreatedAt = DateTime.UtcNow,
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1"
        };
        _appState.Tasks = [googleTask];

        var googleLists = new List<GoogleListCacheEntry> { new("glist-1", "My Google List", "var(--pomodoro-color)", true) };
        var sut = CreateSut();
        SetCachedGoogleLists(sut, googleLists);

        var currentList = sut.CurrentList;
        currentList.Should().NotBeNull();
        currentList!.Id.Should().Be("glist-1");
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_DeletesOrphans_RemoteRemoved()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Orphan",
            CreatedAt = DateTime.UtcNow,
            GoogleTaskId = "orphan-1",
            GoogleListId = "glist-1"
        };
        _appState.Tasks = [localTask];

        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.Id == localTask.Id && t.IsDeleted)), Times.Once);
        var inMemory = _appState.FindTaskById(localTask.Id);
        inMemory.Should().NotBeNull();
        inMemory!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_PerListError_SkipsList_Continues()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "List 1" }, new GoogleTaskList { Id = "glist-2", Title = "List 2" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("API error"));
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-2", It.IsAny<string?>())).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync(It.IsAny<string>())).ReturnsAsync([]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        var cache = GetCachedGoogleLists(sut);
        cache.Should().HaveCount(2);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_GetTaskListsFails_LogsAndRetainsCache()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var sut = CreateSut();
        SetCachedGoogleLists(sut, [new GoogleListCacheEntry("old-list", "Old", "var(--pomodoro-color)", true)]);

        await sut.RefreshGoogleListsAsync();

        GetCachedGoogleLists(sut).Should().HaveCount(1);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_GetTaskListsReturnsNull_ClearsCache()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync((List<GoogleTaskList>?)null);

        var sut = CreateSut();
        SetCachedGoogleLists(sut, [new GoogleListCacheEntry("old-list", "Old", "var(--pomodoro-color)", true)]);

        await sut.RefreshGoogleListsAsync();

        GetCachedGoogleLists(sut).Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_GetTasksReturnsNull_SkipsList()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync((List<GoogleTask>?)null);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task AddTaskAsync_WithLocalPomodoroListId_DelegatesToSingleParam()
    {
        var task = new TaskItem { Name = "New Task", CreatedAt = DateTime.UtcNow };
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.AddTaskAsync("New Task", Constants.TaskLists.LocalPomodoroListId);

        _appState.Tasks.Should().Contain(t => t.Name == "New Task");
    }

    [Fact]
    public async Task InitializeAsync_WhenConnected_RefreshesGoogleLists()
    {
        var record = new AppStateRecord
        {
            Id = Constants.Storage.DefaultSettingsId,
            CurrentTaskId = Guid.NewGuid(),
            CurrentListId = "glist-1"
        };
        _mockIndexedDb.Setup(x => x.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(record);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync([]);

        var sut = CreateSut();
        await sut.InitializeAsync();

        _appState.CurrentListId.Should().Be("glist-1");
    }

    [Fact]
    public void CurrentListId_ReturnsAppStateValue()
    {
        var sut = CreateSut();
        _appState.CurrentListId = "test-list";
        sut.CurrentListId.Should().Be("test-list");
    }

    [Fact]
    public async Task GetTasksForListAsync_GoogleTaskWithoutSidecarMeta_ReturnsUnchanged()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "No Meta",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockSidecarRepo.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        var sut = CreateSut();
        var result = await sut.GetTasksForListAsync("glist-1");

        result.Should().HaveCount(1);
        result[0].PomodoroCount.Should().Be(0);
        result[0].TotalFocusMinutes.Should().Be(0);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_InvalidDates_SetsNullUpdatedAndDueDate()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync(
            [new GoogleTask { Id = "remote-1", Title = "Bad Dates", Status = "needsAction", Updated = "not-a-date", Due = "also-invalid" }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t =>
            t.UpdatedAt == null && t.DueDate == null)), Times.Once);
    }

    [Fact]
    public async Task CompleteTaskAsync_GoogleTask_PushesPatchAndSavesLocally()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync((GoogleTask?)null);

        var sut = CreateSut();
        await sut.CompleteTaskAsync(task.Id);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.IsCompleted)), Times.Once);
        _mockGoogleTasksService.Verify(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()), Times.Once);
        _appState.FindTaskById(task.Id)!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UncompleteTaskAsync_GoogleTask_PushesPatchAndSavesLocally()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow,
            IsCompleted = true
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync((GoogleTask?)null);

        var sut = CreateSut();
        await sut.UncompleteTaskAsync(task.Id);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => !t.IsCompleted)), Times.Once);
        _mockGoogleTasksService.Verify(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()), Times.Once);
        _appState.FindTaskById(task.Id)!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTaskAsync_GoogleTask_DeletesViaGoogleAndSoftDeletesLocally()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _appState.CurrentTaskId = task.Id;
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.DeleteTaskAsync("glist-1", "gtask-1")).Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.DeleteTaskAsync(task.Id);

        _mockGoogleTasksService.Verify(x => x.DeleteTaskAsync("glist-1", "gtask-1"), Times.Once);
        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.IsDeleted)), Times.Once);
        _appState.FindTaskById(task.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateListVisibilityAsync_UpdatesCacheAndSavesSettings()
    {
        var sut = CreateSut();
        SetCachedGoogleLists(sut, [new GoogleListCacheEntry("glist-1", "List", "#4285F4", true)]);

        await sut.UpdateListVisibilityAsync("glist-1", false);

        var cache = GetCachedGoogleLists(sut);
        cache.Should().HaveCount(1);
        cache[0].IsVisible.Should().BeFalse();

        _mockIndexedDb.Verify(x => x.PutAsync(
            Constants.Storage.GoogleTasksSettingsStore,
            It.IsAny<GoogleTasksSettings>()), Times.Once);
    }

    [Fact]
    public async Task UpdateListVisibilityAsync_ExistingListInSettings_UsesSettingsColor()
    {
        var sut = CreateSut();
        SetCachedGoogleLists(sut, [new GoogleListCacheEntry("glist-1", "List", "var(--pomodoro-color)", true)]);

        var settingsField = typeof(TaskService).GetField("_googleTasksSettings", NonPublicInstance)!;
        var settings = (GoogleTasksSettings)settingsField.GetValue(sut)!;
        var lists = new Dictionary<string, ListSetting>(settings.Lists)
        {
            ["glist-1"] = new ListSetting(true, "#FF0000", null)
        };
        settingsField.SetValue(sut, new GoogleTasksSettings(lists));

        await sut.UpdateListVisibilityAsync("glist-1", false);

        var cache = GetCachedGoogleLists(sut);
        cache[0].IsVisible.Should().BeFalse();
        settingsField = typeof(TaskService).GetField("_googleTasksSettings", NonPublicInstance)!;
        var updatedSettings = (GoogleTasksSettings)settingsField.GetValue(sut)!;
        updatedSettings.Lists["glist-1"].Color.Should().Be("#FF0000");
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_WithSettingsEntry_UsesSettingsColor()
    {
        var settings = new GoogleTasksSettings(new Dictionary<string, ListSetting>
        {
            ["glist-1"] = new ListSetting(false, "#AB12CD", DateTime.UtcNow)
        });

        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "Settings List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([]);

        var sut = CreateSut();
        var settingsField = typeof(TaskService).GetField("_googleTasksSettings", NonPublicInstance)!;
        settingsField.SetValue(sut, settings);

        await sut.RefreshGoogleListsAsync();

        var cache = GetCachedGoogleLists(sut);
        cache.Should().HaveCount(1);
        cache[0].Color.Should().Be("#AB12CD");
        cache[0].IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_ValidDueDate_ParsesCorrectly()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync(
            [new GoogleTaskList { Id = "glist-1", Title = "My List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", It.IsAny<string?>())).ReturnsAsync(
            [new GoogleTask
            {
                Id = "remote-1",
                Title = "Due Task",
                Status = "needsAction",
                Updated = "2025-06-20T10:00:00Z",
                Due = "2025-07-01"
            }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t =>
            t.DueDate != null && t.DueDate.Value.Date == new DateTime(2025, 7, 1))), Times.Once);
    }

    [Fact]
    public async Task LoadGoogleTasksSettingsAsync_LoadsExistingSettings()
    {
        var expectedSettings = new GoogleTasksSettings(new Dictionary<string, ListSetting>
        {
            ["glist-1"] = new ListSetting(true, "#4285F4", null)
        });
        _mockIndexedDb.Setup(x => x.GetAsync<GoogleTasksSettings>(
                Constants.Storage.GoogleTasksSettingsStore, Constants.Storage.DefaultSettingsId))
            .ReturnsAsync(expectedSettings);

        var sut = CreateSut();
        await sut.InitializeAsync();

        var settingsField = typeof(TaskService).GetField("_googleTasksSettings", NonPublicInstance)!;
        var actual = (GoogleTasksSettings)settingsField.GetValue(sut)!;
        actual.Lists.Should().ContainKey("glist-1");
        actual.Lists["glist-1"].Color.Should().Be("#4285F4");
    }

    [Fact]
    public async Task GetSidecarCacheAsync_ReturnsCachedOnSecondCall()
    {
        var task1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task 1",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task1];
        _mockSidecarRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(
            [new PomodoroMeta("gtask-1", 5, 125, Priority.High)]);

        var sut = CreateSut();

        await sut.GetTasksForListAsync("glist-1");
        await sut.GetTasksForListAsync("glist-1");

        _mockSidecarRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_GoogleTask_PushesPatchWithChangedFields()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            CreatedAt = DateTime.UtcNow,
            Notes = "old notes"
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        var patched = new GoogleTask { Id = "gtask-1", Title = "New Name", ETag = "etag-2", Updated = "2024-01-01T00:00:00Z" };
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync(patched);

        var sut = CreateSut();
        await sut.UpdateTaskAsync(task.WithUpdates(c => c.Name = "New Name"));

        _mockGoogleTasksService.Verify(x => x.PatchTaskAsync("glist-1", "gtask-1",
            It.Is<GoogleTaskPatch>(p => p.Title == "New Name"), "etag-1"), Times.Once);
        _appState.FindTaskById(task.Id)!.Name.Should().Be("New Name");
        _appState.FindTaskById(task.Id)!.ETag.Should().Be("etag-2");
    }

    [Fact]
    public async Task UpdateTaskAsync_LocalTask_MarksDirty()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Local Task",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        var mockCloudSync = new Mock<ICloudSyncService>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICloudSyncService))).Returns(mockCloudSync.Object);

        var sut = CreateSut();
        await sut.UpdateTaskAsync(task.WithUpdates(c => c.Name = "Updated"));

        mockCloudSync.Verify(x => x.ScheduleSyncAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_GoogleTask_DoesNotMarkDirty()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync((GoogleTask?)null);
        var mockCloudSync = new Mock<ICloudSyncService>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICloudSyncService))).Returns(mockCloudSync.Object);

        var sut = CreateSut();
        await sut.UpdateTaskAsync(task.WithUpdates(c => c.Name = "Updated"));

        mockCloudSync.Verify(x => x.ScheduleSyncAsync(), Times.Never);
    }

    [Fact]
    public async Task CompleteTaskAsync_GoogleTask_PushFailure_SetsIsLocalDirty()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Network error"));

        var sut = CreateSut();
        await sut.CompleteTaskAsync(task.Id);

        _appState.FindTaskById(task.Id)!.IsLocalDirty.Should().BeTrue();
        _appState.FindTaskById(task.Id)!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTaskAsync_GoogleTask_PushFailure_MarksDirtyAndStillSoftDeletes()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.DeleteTaskAsync("glist-1", "gtask-1"))
            .ThrowsAsync(new Exception("Network error"));

        var sut = CreateSut();
        await sut.DeleteTaskAsync(task.Id);

        _mockGoogleTasksService.Verify(x => x.DeleteTaskAsync("glist-1", "gtask-1"), Times.Once);
        _appState.FindTaskById(task.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTaskAsync_GoogleTask_PushFailure_MarksIsLocalDirty()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.DeleteTaskAsync("glist-1", "gtask-1"))
            .ThrowsAsync(new Exception("Network error"));

        var sut = CreateSut();
        await sut.DeleteTaskAsync(task.Id);

        _appState.FindTaskById(task.Id)!.IsLocalDirty.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_SkipsOverwriteWhenIsLocalDirty()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Local Modified",
            IsCompleted = true,
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            IsLocalDirty = true,
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gtask-1", Title = "Remote Title", Status = "needsAction", ETag = "etag-2", Updated = "2024-01-01T00:00:00Z" }]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.Name.Should().Be("Local Modified");
        _appState.FindTaskById(localTask.Id)!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_ClearsDirtyFlagWhenLocalMatchesRemote()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Matched",
            IsCompleted = false,
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            IsLocalDirty = true,
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gtask-1", Title = "Matched", Status = "needsAction", ETag = "etag-2", Updated = "2024-01-01T00:00:00Z" }]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.IsLocalDirty.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_HiddenCompletedTask_SoftDeletesLocally()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Hidden Task",
            IsCompleted = true,
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gtask-1", Title = "Hidden Task", Status = "completed", ETag = "etag-1", Updated = "2024-01-01T00:00:00Z", Hidden = true }]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_LocalTask_MarksDirty()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Local Task",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        var mockCloudSync = new Mock<ICloudSyncService>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICloudSyncService))).Returns(mockCloudSync.Object);

        var sut = CreateSut();
        await sut.CompleteTaskAsync(task.Id);

        mockCloudSync.Verify(x => x.ScheduleSyncAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTaskAsync_GoogleList_EmptyName_ReturnsEarly()
    {
        var sut = CreateSut();

        await sut.AddTaskAsync("", "google-list-id");

        _mockGoogleTasksService.Verify(x => x.InsertTaskAsync(It.IsAny<string>(), It.IsAny<GoogleTask>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskAsync_GoogleTask_PushesDueDateClearing()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Google Task",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            CreatedAt = DateTime.UtcNow,
            DueDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        var patched = new GoogleTask { Id = "gt-1", Title = "Google Task", ETag = "etag-2", Updated = "2026-01-01T00:00:00Z" };
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync(patched);

        var sut = CreateSut();
        var updatedTask = task.WithUpdates(c =>
        {
            c.DueDate = null;
        });
        await sut.UpdateTaskAsync(updatedTask);

        _mockGoogleTasksService.Verify(x => x.PatchTaskAsync("glist-1", "gtask-1",
            It.Is<GoogleTaskPatch>(p => p.Due == ""),
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_GoogleTask_EtagConflict_TriggersPull()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            GoogleTaskId = "gtask-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [task];
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([task]);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gtask-1", Title = "Remote Name", Status = "needsAction", ETag = "etag-remote", Updated = "2026-01-01T00:00:00Z" }]);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync("glist-1", "gtask-1", It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("412 ETag mismatch"));
        _mockIndexedDb.Setup(x => x.GetAsync<GoogleTasksSettings>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((GoogleTasksSettings?)null);

        var sut = CreateSut();
        await sut.UpdateTaskAsync(task.WithUpdates(c => c.Name = "New Name"));

        _mockGoogleTasksService.Verify(x => x.GetTaskListsAsync(), Times.AtLeastOnce);
        _appState.FindTaskById(task.Id)!.Name.Should().Be("Remote Name");
    }

    [Fact]
    public async Task UpdateListVisibilityAsync_HidingCurrentList_FallsBackToFirstVisible()
    {
        _appState.CurrentListId = "glist-2";
        var sut = CreateSut();
        SetCachedGoogleLists(sut, [
            new GoogleListCacheEntry("glist-1", "List 1", "#4285F4", true),
            new GoogleListCacheEntry("glist-2", "List 2", "#0B8043", true)
        ]);

        await sut.UpdateListVisibilityAsync("glist-2", false);

        _appState.CurrentListId.Should().Be("glist-1");
    }

    [Fact]
    public async Task UpdateListVisibilityAsync_HidingCurrentList_NoVisibleGoogleLists_FallsBackToLocal()
    {
        _appState.CurrentListId = "glist-1";
        var sut = CreateSut();
        SetCachedGoogleLists(sut, [
            new GoogleListCacheEntry("glist-1", "List 1", "#4285F4", true)
        ]);

        await sut.UpdateListVisibilityAsync("glist-1", false);

        _appState.CurrentListId.Should().Be(Constants.TaskLists.LocalPomodoroListId);
    }

    [Fact]
    public async Task GetTasksForListAsync_GoogleList_ReturnsFilteredTasks()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Local", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Google", GoogleTaskId = "gt-1", GoogleListId = "glist-1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Other Google", GoogleTaskId = "gt-2", GoogleListId = "glist-2", CreatedAt = DateTime.UtcNow }
        };
        _appState.Tasks = tasks;
        _mockSidecarRepo.Setup(x => x.GetAllAsync()).ReturnsAsync([]);
        var sut = CreateSut();

        var result = await sut.GetTasksForListAsync("glist-1");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Google");
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringGoogleTask_PushesCompletionToGoogle()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Recurring Google",
            GoogleTaskId = "gt-1",
            GoogleListId = "glist-1",
            ETag = "etag-1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        };
        _appState.Tasks = [task];
        _appState.CurrentTaskId = taskId;
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.PatchTaskAsync("glist-1", "gt-1",
                It.IsAny<GoogleTaskPatch>(), It.IsAny<string?>()))
            .ReturnsAsync((string _, string _, GoogleTaskPatch _, string? _) => null!);

        var sut = CreateSut();
        await sut.CompleteTaskAsync(taskId);

        _appState.FindTaskById(taskId)!.IsCompleted.Should().BeTrue();
        _appState.FindTaskById(taskId)!.Repeat!.NextOccurrence.Should().NotBeNull();
        _mockGoogleTasksService.Verify(x => x.PatchTaskAsync("glist-1", "gt-1",
            It.Is<GoogleTaskPatch>(p => p.Status == "completed"), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_RemoteListRemoved_CurrentGoogleList_FallsBackToLocal()
    {
        _appState.CurrentListId = "glist-1";
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-2", Title = "Other List" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-2", null))
            .ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync(It.IsAny<string>())).ReturnsAsync([]);
        var sut = CreateSut();
        SetCachedGoogleLists(sut, [
            new GoogleListCacheEntry("glist-1", "List 1", "#4285F4", true)
        ]);

        await sut.RefreshGoogleListsAsync();

        _appState.CurrentListId.Should().Be(Constants.TaskLists.LocalPomodoroListId);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_DirtyDeletedTask_SkippedWithoutOverwrite()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Task",
            GoogleTaskId = "gt-1",
            GoogleListId = "glist-1",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            IsLocalDirty = true,
            ETag = "old",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gt-1", Title = "Deleted Task", Status = "needsAction", ETag = "etag-new", Updated = "2026-01-01T00:00:00Z" }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        var sut = CreateSut();

        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.IsLocalDirty.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_DirtyTask_MatchesRemote_ClearsDirtyAndPersists()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Match",
            GoogleTaskId = "gt-1",
            GoogleListId = "glist-1",
            IsCompleted = false,
            IsLocalDirty = true,
            ETag = "old",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gt-1", Title = "Match", Status = "needsAction", ETag = "etag-new", Updated = "2026-01-01T00:00:00Z" }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        _mockTaskRepo.Setup(x => x.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        var sut = CreateSut();

        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.IsLocalDirty.Should().BeFalse();
        _appState.FindTaskById(localTask.Id)!.ETag.Should().Be("etag-new");
        _mockTaskRepo.Verify(x => x.SaveAsync(It.Is<TaskItem>(t => t.ETag == "etag-new")), Times.Once);
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_DirtyTask_DiffersFromRemote_KeepsDirty()
    {
        var localTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Local Name",
            GoogleTaskId = "gt-1",
            GoogleListId = "glist-1",
            IsCompleted = false,
            IsLocalDirty = true,
            ETag = "old",
            CreatedAt = DateTime.UtcNow
        };
        _appState.Tasks = [localTask];
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync())
            .ReturnsAsync([new GoogleTaskList { Id = "glist-1", Title = "List 1" }]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("glist-1", null))
            .ReturnsAsync([new GoogleTask { Id = "gt-1", Title = "Remote Name", Status = "needsAction", ETag = "etag-new", Updated = "2026-01-01T00:00:00Z" }]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync("glist-1")).ReturnsAsync([localTask]);
        var sut = CreateSut();

        await sut.RefreshGoogleListsAsync();

        _appState.FindTaskById(localTask.Id)!.IsLocalDirty.Should().BeTrue();
        _appState.FindTaskById(localTask.Id)!.Name.Should().Be("Local Name");
    }

    [Fact]
    public async Task InitializeAsync_RestoresCachedListsFromSettings()
    {
        var settings = new GoogleTasksSettings(
            new Dictionary<string, ListSetting>
            {
                ["list-1"] = new ListSetting(true, "#4285F4", null),
                ["list-2"] = new ListSetting(true, "#0B8043", null)
            },
            ["list-1", "list-2"]);
        _mockIndexedDb.Setup(x => x.GetAsync<GoogleTasksSettings>("googleTasksSettings", "default"))
            .ReturnsAsync(settings);

        var sut = CreateSut();
        await sut.InitializeAsync();

        var cache = GetCachedGoogleLists(sut);
        cache.Should().HaveCount(2);
        cache[0].Id.Should().Be("list-1");
        cache[1].Id.Should().Be("list-2");
    }

    [Fact]
    public async Task RefreshGoogleListsAsync_SavesListIdsToSettings()
    {
        _mockGoogleTasksService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockGoogleTasksService.Setup(x => x.GetTaskListsAsync()).ReturnsAsync([
            new GoogleTaskList { Id = "list-1", Title = "Personal" },
            new GoogleTaskList { Id = "list-2", Title = "Work" }
        ]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("list-1", null)).ReturnsAsync([]);
        _mockGoogleTasksService.Setup(x => x.GetTasksAsync("list-2", null)).ReturnsAsync([]);
        _mockTaskRepo.Setup(x => x.GetByGoogleListIdAsync(It.IsAny<string>())).ReturnsAsync([]);

        var sut = CreateSut();
        await sut.RefreshGoogleListsAsync();

        _mockIndexedDb.Verify(x => x.PutAsync("googleTasksSettings",
            It.Is<GoogleTasksSettings>(s => s.ListIds != null && s.ListIds.Count == 2)),
            Times.AtLeastOnce);
    }
}
