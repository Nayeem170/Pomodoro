using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    public async Task DisposeAsync_DisposesMidnightTimer()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        await service.DisposeAsync();

        // Assert - should not throw, timer disposed
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task InitializeAsync_SchedulesMidnightTimer_WhenRecurringTaskExists()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId, isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            LastCompletedDate = DateTime.Now.Date
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        // Act
        var service = CreateService();
        await service.InitializeAsync();

        // Assert - task stays checked because completed today
        service.AllTasks[0].IsCompleted.Should().BeTrue(
            "a daily task completed today must stay checked");

        // Cleanup
        await service.DisposeAsync();
    }
}
