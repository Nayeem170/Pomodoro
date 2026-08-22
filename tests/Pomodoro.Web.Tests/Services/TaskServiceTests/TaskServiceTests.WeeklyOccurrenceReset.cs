using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using AppStateRecord = Pomodoro.Web.Services.TaskService.AppStateRecord;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    private static DateTime PreviousOccurrenceAtLeastDaysBack(int minDaysBack)
    {
        var date = DateTime.Now.Date.AddDays(-minDaysBack);
        while (date.DayOfWeek is not (DayOfWeek.Monday or DayOfWeek.Thursday))
        {
            date = date.AddDays(-1);
        }
        return date;
    }

    private static DateTime MostRecentOccurrenceOnOrBeforeToday()
    {
        var date = DateTime.Now.Date;
        while (date.DayOfWeek is not (DayOfWeek.Monday or DayOfWeek.Thursday))
        {
            date = date.AddDays(-1);
        }
        return date;
    }

    [Fact]
    public async Task InitializeAsync_WeeklyByDays_UncompletedTaskWithStaleProgress_ResetsAsNewOccurrence()
    {
        var staleOccurrence = PreviousOccurrenceAtLeastDaysBack(9);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = staleOccurrence.AddDays(-7);
        task.Repeat = new RepeatRule { Type = RepeatType.Weekly, Weekdays = [DayOfWeek.Monday, DayOfWeek.Thursday] };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = staleOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_WeeklyByDays_CompletedBeforeOccurrence_ReactivatesAndClearsProgress()
    {
        var staleOccurrence = PreviousOccurrenceAtLeastDaysBack(9);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [DayOfWeek.Monday, DayOfWeek.Thursday],
            LastCompletedDate = staleOccurrence
        };
        task.CompletedAt = staleOccurrence;
        task.PomodoroCount = 3;
        task.TotalFocusMinutes = 75;
        task.LastWorkedOn = staleOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_WeeklyByDays_ProgressOnCurrentOccurrence_IsNotReset()
    {
        var currentOccurrence = MostRecentOccurrenceOnOrBeforeToday();
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = currentOccurrence.AddDays(-7);
        task.Repeat = new RepeatRule { Type = RepeatType.Weekly, Weekdays = [DayOfWeek.Monday, DayOfWeek.Thursday] };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = currentOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(2, service.AllTasks[0].PomodoroCount);
        Assert.Equal(50, service.AllTasks[0].TotalFocusMinutes);
        Assert.Equal(currentOccurrence, service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_WeeklyByDays_CompletedOnCurrentOccurrence_IsNotReset()
    {
        var currentOccurrence = MostRecentOccurrenceOnOrBeforeToday();
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.CreatedAt = currentOccurrence.AddDays(-7);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [DayOfWeek.Monday, DayOfWeek.Thursday],
            LastCompletedDate = currentOccurrence
        };
        task.CompletedAt = currentOccurrence;
        task.PomodoroCount = 1;
        task.TotalFocusMinutes = 25;
        task.LastWorkedOn = currentOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.True(service.AllTasks[0].IsCompleted);
        Assert.Equal(1, service.AllTasks[0].PomodoroCount);
        Assert.Equal(25, service.AllTasks[0].TotalFocusMinutes);
        Assert.NotNull(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_WeeklyByDays_StaleParent_ResetsFollowsParentSubtasks()
    {
        var staleOccurrence = PreviousOccurrenceAtLeastDaysBack(9);
        var currentOccurrence = MostRecentOccurrenceOnOrBeforeToday();
        var parentId = Guid.NewGuid();
        var parent = CreateSampleTask(id: parentId, isCompleted: false);
        parent.CreatedAt = staleOccurrence.AddDays(-7);
        parent.Repeat = new RepeatRule { Type = RepeatType.Weekly, Weekdays = [DayOfWeek.Monday, DayOfWeek.Thursday] };
        parent.PomodoroCount = 2;
        parent.TotalFocusMinutes = 50;
        parent.LastWorkedOn = staleOccurrence;

        var staleChild = CreateSampleTask(isCompleted: true);
        staleChild.ParentTaskId = parentId;
        staleChild.CompletedAt = staleOccurrence;
        staleChild.PomodoroCount = 1;
        staleChild.TotalFocusMinutes = 25;
        staleChild.LastWorkedOn = staleOccurrence;

        var currentChild = CreateSampleTask(isCompleted: true);
        currentChild.ParentTaskId = parentId;
        currentChild.CompletedAt = currentOccurrence;
        currentChild.PomodoroCount = 1;
        currentChild.TotalFocusMinutes = 25;
        currentChild.LastWorkedOn = currentOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync())
            .ReturnsAsync(new List<TaskItem> { parent, staleChild, currentChild });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.Equal(0, service.AllTasks.First(t => t.Id == parentId).PomodoroCount);
        Assert.Null(service.AllTasks.First(t => t.Id == parentId).LastWorkedOn);

        Assert.False(service.AllTasks.First(t => t.Id == staleChild.Id).IsCompleted);
        Assert.Equal(0, service.AllTasks.First(t => t.Id == staleChild.Id).PomodoroCount);

        Assert.True(service.AllTasks.First(t => t.Id == currentChild.Id).IsCompleted);
        Assert.Equal(1, service.AllTasks.First(t => t.Id == currentChild.Id).PomodoroCount);
    }

    [Fact]
    public async Task InitializeAsync_WeeklyAnchorDay_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var staleOccurrence = PreviousOccurrenceAtLeastDaysBack(9);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = staleOccurrence.AddDays(-7);
        task.Repeat = new RepeatRule { Type = RepeatType.Weekly };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = staleOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_Daily_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var staleOccurrence = DateTime.Now.Date.AddDays(-3);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = staleOccurrence.AddDays(-7);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily };
        task.PomodoroCount = 4;
        task.TotalFocusMinutes = 100;
        task.LastWorkedOn = staleOccurrence;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_Daily_ProgressOnCurrentOccurrence_IsNotReset()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = DateTime.Now.Date.AddDays(-7);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = DateTime.Now.Date;

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(2, service.AllTasks[0].PomodoroCount);
        Assert.Equal(50, service.AllTasks[0].TotalFocusMinutes);
        Assert.Equal(DateTime.Now.Date, service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_CustomEveryThreeDays_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = DateTime.Now.Date.AddDays(-17);
        task.Repeat = new RepeatRule { Type = RepeatType.Custom, CustomDays = 3 };
        task.PomodoroCount = 1;
        task.TotalFocusMinutes = 25;
        task.LastWorkedOn = DateTime.Now.Date.AddDays(-10);

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_Monthly_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = today.AddDays(-35);
        task.Repeat = new RepeatRule { Type = RepeatType.Monthly, MonthlyDay = today.Day };
        task.PomodoroCount = 3;
        task.TotalFocusMinutes = 75;
        task.LastWorkedOn = today.AddDays(-10);

        // Arrange
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].PomodoroCount);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
        Assert.Null(service.AllTasks[0].LastWorkedOn);
    }
}
