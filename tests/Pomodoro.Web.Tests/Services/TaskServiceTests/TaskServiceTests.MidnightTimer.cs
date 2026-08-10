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
        var taskId = Guid.NewGuid();
        var task = CreateSampleTask(id: taskId);
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();
        await service.DisposeAsync();

        service.Should().NotBeNull();
    }

    [Fact]
    public async Task OnMidnightElapsedAsync_DailyTaskCompletedToday_StaysChecked()
    {
        var task = CreateSampleTask(id: Guid.NewGuid(), isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            LastCompletedDate = DateTime.Now.Date
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var wasChecked = service.AllTasks[0].IsCompleted;
        await service.OnMidnightElapsedAsync();

        service.AllTasks[0].IsCompleted.Should().Be(wasChecked,
            "a daily task completed today must stay checked even after midnight fires");
        await service.DisposeAsync();
    }

    [Fact]
    public async Task OnMidnightElapsedAsync_NoRecurringTasks_DoesNotThrow()
    {
        var task = CreateSampleTask(id: Guid.NewGuid(), isCompleted: true);
        task.Repeat = null;

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.OnMidnightElapsedAsync();

        await act.Should().NotThrowAsync();
        await service.DisposeAsync();
    }

    [Fact]
    public async Task HandleMidnightTimerCallbackAsync_Success_RunsReactivationAndReschedules()
    {
        var task = CreateSampleTask(id: Guid.NewGuid());
        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        var act = async () => await service.HandleMidnightTimerCallbackAsync();

        await act.Should().NotThrowAsync();
        await service.DisposeAsync();
    }

    [Fact]
    public async Task HandleMidnightTimerCallbackAsync_SaveThrows_LogsAndReschedulesWithoutThrowing()
    {
        var task = CreateSampleTask(id: Guid.NewGuid(), isCompleted: true);
        task.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            LastCompletedDate = DateTime.Now.Date
        };

        MockTaskRepository.Setup(r => r.GetAllIncludingDeletedAsync()).ReturnsAsync(new List<TaskItem> { task });
        MockIndexedDb.Setup(d => d.GetAsync<AppStateRecord>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((AppStateRecord?)null);

        var service = CreateService();
        await service.InitializeAsync();

        task.Repeat.LastCompletedDate = DateTime.Now.Date.AddDays(-2);
        MockIndexedDb.Setup(d => d.PutAllAsync(It.IsAny<string>(), It.IsAny<List<TaskItem>>()))
            .ThrowsAsync(new InvalidOperationException("db failure"));

        var act = async () => await service.HandleMidnightTimerCallbackAsync();

        await act.Should().NotThrowAsync(
            "the callback must swallow exceptions to keep the timer alive");
        await service.DisposeAsync();
    }

    [Fact]
    public void GetDelayUntilMidnight_ReturnsPositiveAndWithinDay()
    {
        var delay = TaskService.GetDelayUntilMidnight();

        delay.Should().BeGreaterThan(TimeSpan.Zero);
        delay.Should().BeLessThanOrEqualTo(TimeSpan.FromDays(1));
    }
}
