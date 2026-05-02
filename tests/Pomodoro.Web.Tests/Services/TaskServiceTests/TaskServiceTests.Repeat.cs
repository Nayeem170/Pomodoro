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
    [Fact]
    public async Task CompleteTaskAsync_RecurringDaily_SetsNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.True(service.AllTasks[0].IsCompleted);
        Assert.NotNull(service.AllTasks[0].Repeat);
        Assert.NotNull(service.AllTasks[0].Repeat.NextOccurrence);
        Assert.Equal(DateTime.UtcNow.Date.AddDays(1), service.AllTasks[0].Repeat.NextOccurrence.Value.Date);
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringWeekly_SetsNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Weekly, Weekdays = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday] };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.NotNull(service.AllTasks[0].Repeat.NextOccurrence);
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringCustom_SetsNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Custom, CustomDays = 3 };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.Equal(DateTime.UtcNow.Date.AddDays(3), service.AllTasks[0].Repeat.NextOccurrence.Value.Date);
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringMonthly_SetsNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Monthly, MonthlyDay = 15 };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        var next = service.AllTasks[0].Repeat.NextOccurrence!.Value;
        Assert.Equal(15, next.Day);
        Assert.True(next > DateTime.UtcNow);
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringPaused_DoesNotSetNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = true };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.True(service.AllTasks[0].IsCompleted);
        Assert.Null(service.AllTasks[0].Repeat.NextOccurrence);
    }

    [Fact]
    public async Task CompleteTaskAsync_RecurringExpiredEnd_DoesNotSetNextOccurrence()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, EndDate = DateTime.UtcNow.Date.AddDays(-1) };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.True(service.AllTasks[0].IsCompleted);
        Assert.Null(service.AllTasks[0].Repeat.NextOccurrence);
    }

    [Fact]
    public async Task CompleteTaskAsync_NonRecurring_CompletesNormally()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = null;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.CompleteTaskAsync(taskId);

        Assert.True(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task InitializeAsync_ActivatesDueRecurringTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = DateTime.UtcNow.Date.AddDays(-1) };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.False(service.AllTasks[0].IsCompleted);
        Assert.Equal(0, service.AllTasks[0].TotalFocusMinutes);
    }

    [Fact]
    public async Task InitializeAsync_ActivatesScheduledTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.ScheduledDate = DateTime.UtcNow.Date;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.False(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotActivateFutureScheduledTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.ScheduledDate = DateTime.UtcNow.Date.AddDays(1);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.True(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotActivatePausedRecurringTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = DateTime.UtcNow.Date, IsPaused = true };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.True(service.AllTasks[0].IsCompleted);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotActivateFutureRecurringTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily, LastCompletedDate = DateTime.UtcNow.Date };
        task.Repeat.NextOccurrence = DateTime.UtcNow.Date.AddDays(1);

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        Assert.True(service.AllTasks[0].IsCompleted);
    }
}
