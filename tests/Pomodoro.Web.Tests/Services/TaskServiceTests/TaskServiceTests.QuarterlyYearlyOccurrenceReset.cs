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
    public async Task InitializeAsync_Quarterly_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = today.AddMonths(-6);
        task.Repeat = new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = today.Day };
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

    [Fact]
    public async Task InitializeAsync_Quarterly_ProgressOnCurrentOccurrence_IsNotReset()
    {
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = today.AddMonths(-6);
        task.Repeat = new RepeatRule { Type = RepeatType.Quarterly, QuarterlyDay = today.Day };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = today;

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
        Assert.NotNull(service.AllTasks[0].LastWorkedOn);
    }

    [Fact]
    public async Task InitializeAsync_Yearly_StaleUncompletedProgress_ResetsAsNewOccurrence()
    {
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = today.AddMonths(-12);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Yearly,
            YearlyDay = today.Day,
            YearlyMonth = today.Month
        };
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

    [Fact]
    public async Task InitializeAsync_Yearly_ProgressOnCurrentOccurrence_IsNotReset()
    {
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.CreatedAt = today.AddMonths(-12);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Yearly,
            YearlyDay = today.Day,
            YearlyMonth = today.Month
        };
        task.PomodoroCount = 2;
        task.TotalFocusMinutes = 50;
        task.LastWorkedOn = today;

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
        Assert.NotNull(service.AllTasks[0].LastWorkedOn);
    }
}
