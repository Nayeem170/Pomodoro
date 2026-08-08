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
    public async Task CompleteTaskAsync_RecurringTask_StoresDateOnlyLastCompletedDate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: false);
        task.Repeat = new RepeatRule { Type = RepeatType.Daily };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockTaskRepository.Setup(r => r.SaveAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.CompleteTaskAsync(taskId);

        // Assert
        var storedDate = service.AllTasks[0].Repeat!.LastCompletedDate;
        storedDate.Should().NotBeNull();
        storedDate!.Value.TimeOfDay.Should().Be(TimeSpan.Zero,
            "LastCompletedDate must be a date (midnight) so that ComputeNextOccurrence produces a comparable date");
    }

    [Fact]
    public async Task InitializeAsync_RecurringTaskCompletedYesterdayWithTimeComponent_Unchecks()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            LastCompletedDate = DateTime.Now.Date.AddDays(-1).AddHours(12)
        };

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
        service.AllTasks[0].IsCompleted.Should().BeFalse(
            "a daily task completed yesterday must uncheck on the next day's app load, "
            + "even when LastCompletedDate was stored with a non-midnight time component");
    }
}
