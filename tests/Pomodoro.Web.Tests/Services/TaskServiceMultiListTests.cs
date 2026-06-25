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
    public async Task CompleteTaskAsync_GoogleTask_IsNoOp()
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

        var sut = CreateSut();
        await sut.CompleteTaskAsync(task.Id);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        _appState.FindTaskById(task.Id)!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task UncompleteTaskAsync_GoogleTask_IsNoOp()
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

        var sut = CreateSut();
        await sut.UncompleteTaskAsync(task.Id);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTaskAsync_GoogleTask_IsNoOp()
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

        var sut = CreateSut();
        await sut.DeleteTaskAsync(task.Id);

        _mockTaskRepo.Verify(x => x.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        _appState.FindTaskById(task.Id)!.IsDeleted.Should().BeFalse();
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
}
