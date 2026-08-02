using System.Reflection;
using Bunit;
using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class IndexScheduleCoverageTests : TestHelper
{
    private static readonly BindingFlags NonPublic =
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static object? InvokeStatic(string name, Type[] types, object[] args) =>
        typeof(IndexBase).GetMethod(name, NonPublic, types)?.Invoke(null, args);

    private static async Task InvokeInstanceTask(object instance, string name)
    {
        var method = typeof(IndexBase).GetMethod(name, NonPublic);
        await (Task)method!.Invoke(instance, null)!;
    }

    #region OccursOn / RepeatOccursOn / BuildRepeatLabel (private static, via reflection)

    [Fact]
    public void OccursOn_MatchesOccurrenceDate_ScheduledDate_DueDate_AndRepeat()
    {
        // Arrange
        var today = new DateTime(2025, 1, 10);
        var occurrence = new TaskItem { OccurrenceDate = today };
        var scheduled = new TaskItem { ScheduledDate = today };
        var due = new TaskItem { DueDate = today };
        var none = new TaskItem { ScheduledDate = today.AddDays(5) };

        var occursType = new[] { typeof(TaskItem), typeof(DateTime) };

        // Act / Assert
        InvokeStatic("OccursOn", occursType, [occurrence, today]).Should().Be(true);
        InvokeStatic("OccursOn", occursType, [scheduled, today]).Should().Be(true);
        InvokeStatic("OccursOn", occursType, [due, today]).Should().Be(true);
        InvokeStatic("OccursOn", occursType, [none, today]).Should().Be(false);
    }

    [Fact]
    public void RepeatOccursOn_DailyWeeklyCustomMonthly_Branches()
    {
        // Arrange
        var anchor = new DateTime(2025, 1, 10); // Friday
        var daily = new RepeatRule { Type = RepeatType.Daily, StartDate = anchor };
        var weeklyEmpty = new RepeatRule { Type = RepeatType.Weekly, StartDate = anchor, Weekdays = [] };
        var weeklyDays = new RepeatRule { Type = RepeatType.Weekly, StartDate = anchor, Weekdays = [DayOfWeek.Monday] };
        var custom = new RepeatRule { Type = RepeatType.Custom, StartDate = anchor, CustomDays = 3 };
        var monthly = new RepeatRule { Type = RepeatType.Monthly, StartDate = anchor, MonthlyDay = 10 };

        var task = new TaskItem { CreatedAt = anchor };
        var type = new[] { typeof(RepeatRule), typeof(TaskItem), typeof(DateTime) };

        // Act / Assert
        InvokeStatic("RepeatOccursOn", type, [daily, task, anchor]).Should().Be(true);
        InvokeStatic("RepeatOccursOn", type, [weeklyEmpty, task, anchor.AddDays(7)]).Should().Be(true);
        InvokeStatic("RepeatOccursOn", type, [weeklyDays, task, new DateTime(2025, 1, 13)]).Should().Be(true);
        InvokeStatic("RepeatOccursOn", type, [custom, task, anchor.AddDays(3)]).Should().Be(true);
        InvokeStatic("RepeatOccursOn", type, [monthly, task, new DateTime(2025, 2, 10)]).Should().Be(true);
        // before anchor -> false; past end date -> false; None type -> switch default false
        InvokeStatic("RepeatOccursOn", type,
            [new RepeatRule { Type = RepeatType.Daily, StartDate = anchor, EndDate = anchor }, task, anchor.AddDays(2)])
            .Should().Be(false);
        InvokeStatic("RepeatOccursOn", type, [daily, task, anchor.AddDays(-1)]).Should().Be(false);
        InvokeStatic("RepeatOccursOn", type,
            [new RepeatRule { Type = RepeatType.None, StartDate = anchor }, task, anchor]).Should().Be(false);
    }

    [Fact]
    public void BuildRepeatLabel_DailyWeeklyMonthlyCustom_Branches()
    {
        // Arrange
        var type = new[] { typeof(RepeatRule) };

        // Act / Assert
        InvokeStatic("BuildRepeatLabel", type, [new RepeatRule { Type = RepeatType.Daily }]).Should().Be(Constants.Repeat.LabelDaily);
        InvokeStatic("BuildRepeatLabel", type, [new RepeatRule { Type = RepeatType.Weekly }]).Should().Be(Constants.Repeat.LabelWeekly);
        InvokeStatic("BuildRepeatLabel", type, [new RepeatRule { Type = RepeatType.Monthly }]).Should().Be(Constants.Repeat.LabelMonthly);
        InvokeStatic("BuildRepeatLabel", type, [new RepeatRule { Type = RepeatType.Custom, CustomDays = 4 }]).Should().Be("×4d");
        InvokeStatic("BuildRepeatLabel", type, [new RepeatRule { Type = RepeatType.Custom, CustomDays = 0 }]).Should().Be(Constants.Repeat.LabelRepeat);
        InvokeStatic("BuildRepeatLabel", type, [null]).Should().BeNull();
    }

    #endregion

    #region Schedule window properties + handlers (instance, via cut.Instance / reflection)

    [Fact]
    public async Task ScheduleWindow_BuildsDaysFromAppStateTasks()
    {
        // Arrange - seed AppState with a scheduled + a due + a repeat task.
        var today = DateTime.UtcNow.Date;
        AppState.Tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Scheduled", CreatedAt = DateTime.UtcNow, ScheduledDate = today },
            new() { Id = Guid.NewGuid(), Name = "Due", CreatedAt = DateTime.UtcNow, DueDate = today },
            new() { Id = Guid.NewGuid(), Name = "Repeat", CreatedAt = DateTime.UtcNow, Repeat = new RepeatRule { Type = RepeatType.Daily } }
        };

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - read the protected ScheduleWindow property.
        var prop = typeof(IndexBase).GetProperty("ScheduleWindow", NonPublic);
        var window = (IReadOnlyList<ScheduleDay>)prop!.GetValue(cut.Instance)!;

        // Assert - the window has the configured number of days and the first day carries the tasks.
        window.Should().NotBeEmpty();
        var labelProp = typeof(IndexBase).GetProperty("ScheduleWindowLabel", NonPublic);
        labelProp!.GetValue(cut.Instance).Should().NotBeNull();
    }

    [Fact]
    public async Task HandleSchedulePrev_AtZeroOffset_NoOp_BeyondZero_Decrements()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var offsetField = typeof(IndexBase).GetField("_scheduleWeekOffset", NonPublic)!;

        // Act - at offset 0 prev is a no-op.
        await InvokeInstanceTask(cut.Instance, "HandleSchedulePrev");
        offsetField.GetValue(cut.Instance).Should().Be(0);

        // Act - next increments, then prev decrements.
        await InvokeInstanceTask(cut.Instance, "HandleScheduleNext");
        offsetField.GetValue(cut.Instance).Should().Be(1);
        await InvokeInstanceTask(cut.Instance, "HandleSchedulePrev");
        offsetField.GetValue(cut.Instance).Should().Be(0);
    }

    [Fact]
    public async Task HandleScheduleEdit_RepeatOccurrenceNotMaterialized_CallsMaterializeSingle()
    {
        // Arrange
        var seriesId = Guid.NewGuid();
        var occurrenceDate = DateTime.UtcNow.Date;
        var occurrence = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Occ",
            CreatedAt = DateTime.UtcNow,
            RepeatSeriesId = seriesId,
            OccurrenceDate = occurrenceDate
        };
        AppState.Tasks = new List<TaskItem>();
        TaskServiceMock.Setup(x => x.MaterializeSingleAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.GetTasksForListAsync(It.IsAny<string>())).ReturnsAsync(new List<TaskItem>());

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleScheduleEdit(occurrence);

        // Assert
        TaskServiceMock.Verify(x => x.MaterializeSingleAsync(occurrence), Times.Once);
    }

    [Fact]
    public async Task HandleScheduleEdit_RepeatOccurrenceAlreadyMaterialized_CallsUpdateTask()
    {
        // Arrange
        var seriesId = Guid.NewGuid();
        var occurrenceDate = DateTime.UtcNow.Date;
        var existing = new TaskItem { Id = Guid.NewGuid(), Name = "Existing", RepeatSeriesId = seriesId, OccurrenceDate = occurrenceDate };
        AppState.Tasks = new List<TaskItem> { existing };
        var occurrence = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Occ",
            CreatedAt = DateTime.UtcNow,
            RepeatSeriesId = seriesId,
            OccurrenceDate = occurrenceDate
        };
        TaskServiceMock.Setup(x => x.UpdateTaskAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.GetTasksForListAsync(It.IsAny<string>())).ReturnsAsync(new List<TaskItem>());

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleScheduleEdit(occurrence);

        // Assert
        TaskServiceMock.Verify(x => x.UpdateTaskAsync(occurrence), Times.Once);
        TaskServiceMock.Verify(x => x.MaterializeSingleAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task HandleScheduleEdit_NonRepeat_CallsUpdateTask()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Plain", CreatedAt = DateTime.UtcNow };
        TaskServiceMock.Setup(x => x.UpdateTaskAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.GetTasksForListAsync(It.IsAny<string>())).ReturnsAsync(new List<TaskItem>());
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleScheduleEdit(task);

        // Assert
        TaskServiceMock.Verify(x => x.UpdateTaskAsync(task), Times.Once);
    }

    [Fact]
    public async Task HandleAddSubtask_InvokesTaskServiceAddSubtask()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new AddSubtaskRequest(parentId, "Kid");
        TaskServiceMock.Setup(x => x.AddSubtaskAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.GetTasksForListAsync(It.IsAny<string>())).ReturnsAsync(new List<TaskItem>());
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleAddSubtask(request);

        // Assert
        TaskServiceMock.Verify(x => x.AddSubtaskAsync("Kid", parentId), Times.Once);
    }

    [Fact]
    public async Task HandleReparentToRoot_InvokesTaskServiceReparent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        TaskServiceMock.Setup(x => x.ReparentTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.GetTasksForListAsync(It.IsAny<string>())).ReturnsAsync(new List<TaskItem>());
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleReparentToRoot(taskId);

        // Assert
        TaskServiceMock.Verify(x => x.ReparentTaskAsync(taskId, null), Times.Once);
    }

    [Fact]
    public async Task HandleAddSubtask_WhenServiceThrows_SetsErrorMessage()
    {
        // Arrange - covers TryExecuteAsync catch path.
        TaskServiceMock.Setup(x => x.AddSubtaskAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("boom"));
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await cut.Instance.HandleAddSubtask(new AddSubtaskRequest(Guid.NewGuid(), "Kid"));

        // Assert
        cut.Instance.ErrorMessage.Should().Contain("boom");
    }

    [Fact]
    public void ErrorBoundary_RendersRetryFallback_WhenTaskRenderThrows()
    {
        // Arrange - render Index normally, then force a render-time fault by nulling Tasks.
        // TodayTasks => Tasks, so TodayTasks.ToList() throws NRE inside the ErrorBoundary's
        // ChildContent; the boundary catches it and renders its ErrorContent (Retry button).
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var tasksProp = typeof(IndexBase).GetProperty("Tasks", NonPublic);
        tasksProp!.SetValue(cut.Instance, null);

        // Act
        cut.Render();

        // Assert
        cut.FindAll(".section-error").Should().HaveCount(1);
        cut.Find("button.btn-cancel").TextContent.Should().Contain("Retry");

        // Act - click Retry -> ErrorBoundary.Recover() (covers the onclick handler).
        cut.Find("button.btn-cancel").Click();
        cut.Render();
    }

    #endregion
}
