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
    public async Task InitializeAsync_WeeklyTaskCompletedBeforeNextWeekday_UnchecksOnWeekday()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [today.DayOfWeek],
            LastCompletedDate = yesterday
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeFalse(
            "a weekly task completed before its weekday must uncheck when that weekday arrives");
    }

    [Fact]
    public async Task InitializeAsync_MonthlyTaskCompletedLastMonth_UnchecksOnMonthlyDay()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var lastMonth = today.AddMonths(-1);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Monthly,
            MonthlyDay = today.Day,
            LastCompletedDate = lastMonth
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeFalse(
            "a monthly task completed last month must uncheck when the monthly day arrives");
    }

    [Fact]
    public async Task InitializeAsync_CustomTaskCompletedAfterInterval_Unchecks()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var fiveDaysAgo = today.AddDays(-5);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Custom,
            CustomDays = 3,
            LastCompletedDate = fiveDaysAgo
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeFalse(
            "a custom-interval task completed beyond its interval must uncheck");
    }

    [Fact]
    public async Task InitializeAsync_CustomTaskCompletedWithinInterval_StaysChecked()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Custom,
            CustomDays = 5,
            LastCompletedDate = yesterday
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeTrue(
            "a custom-interval task completed within its interval must stay checked");
    }

    [Fact]
    public async Task InitializeAsync_RecurringTaskPastEndDate_StaysCompleted()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            EndDate = today.AddDays(-1),
            LastCompletedDate = today.AddDays(-2)
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeTrue(
            "a recurring task whose end date has passed must stay completed");
    }

    [Fact]
    public async Task InitializeAsync_NonRecurringCompletedTask_StaysCompletedForever()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = null;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeTrue(
            "a non-repeating task stays completed forever");
    }

    [Fact]
    public async Task InitializeAsync_RecurringTaskUnchecked_ResetsFocusAndPomodoro()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.TotalFocusMinutes = 50;
        task.PomodoroCount = 2;
        task.LastWorkedOn = today.AddDays(-1);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            LastCompletedDate = today.AddDays(-1)
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        await service.InitializeAsync();

        // Assert
        service.AllTasks[0].IsCompleted.Should().BeFalse();
        service.AllTasks[0].TotalFocusMinutes.Should().Be(0);
        service.AllTasks[0].PomodoroCount.Should().Be(0);
        service.AllTasks[0].LastWorkedOn.Should().BeNull();
    }
}
