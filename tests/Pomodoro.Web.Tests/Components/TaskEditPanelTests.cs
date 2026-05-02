using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TaskEditPanelTests : TestContext
{
    private static TaskItem CreateTask(Action<TaskItem>? configure = null)
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test Task" };
        configure?.Invoke(task);
        return task;
    }

    [Fact]
    public void OnInitialized_WithNullRepeat_SetsDefaults()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("None");
        cut.Markup.Should().Contain("Schedule");
        cut.Markup.Should().NotContain("tep-weekdays");
        cut.Markup.Should().NotContain("Pause");
    }

    [Fact]
    public void OnInitialized_WithDailyRepeat_ShowsDailySelected()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule { Type = RepeatType.Daily });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("Daily");
        cut.Markup.Should().Contain("Pause");
    }

    [Fact]
    public void ToggleWeekday_TogglesDaySelection()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [DayOfWeek.Monday]
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var buttons = cut.FindAll(".tep-weekday-btn");
        buttons.Count.Should().Be(7);
        cut.FindAll(".tep-weekday-btn.active").Count.Should().Be(1);
    }

    [Fact]
    public void OnInitialized_WithCustomRepeat_ShowsCustomDaysInput()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Custom,
            CustomDays = 3
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("Every");
        cut.Markup.Should().Contain("days");
    }

    [Fact]
    public void OnInitialized_WithMonthlyRepeat_ShowsMonthlyDayInput()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Monthly,
            MonthlyDay = 15
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("Day");
        cut.Markup.Should().Contain("of month");
    }

    [Fact]
    public void OnInitialized_WithScheduledDate_ShowsDate()
    {
        var date = new DateTime(2026, 6, 15);
        var task = CreateTask(t => t.ScheduledDate = date);
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("2026-06-15");
    }

    [Fact]
    public void OnInitialized_WithPausedRepeat_ShowsPauseActive()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            IsPaused = true
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Find(".tep-toggle").ClassList.Should().Contain("active");
    }

    [Fact]
    public void HandleSave_WithNoneType_SetsRepeatToNull()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule { Type = RepeatType.Daily });
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("None");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().BeNull();
    }

    [Fact]
    public void HandleSave_WithExistingRepeat_PreservesMetadata()
    {
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);
        var lastCompleted = new DateTime(2026, 5, 1);
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Daily,
            StartDate = startDate,
            EndDate = endDate,
            LastCompletedDate = lastCompleted,
            CustomDays = 5
        });
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.StartDate.Should().Be(startDate);
        savedTask.Repeat.EndDate.Should().Be(endDate);
        savedTask.Repeat.LastCompletedDate.Should().Be(lastCompleted);
        savedTask.Repeat.NextOccurrence.Should().BeNull();
    }

    [Fact]
    public void HandleSave_WithNewRepeat_CreatesFreshRule()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Weekly");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Weekly);
    }

    [Fact]
    public void HandleSave_SetsScheduledDate()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        cut.Find("input[type=\"date\"]").Change("2026-07-04");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.ScheduledDate.Should().Be(new DateTime(2026, 7, 4));
    }

    [Fact]
    public void HandleCancel_InvokesOnCancel()
    {
        var task = CreateTask();
        var cancelled = false;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        cut.Find(".tep-cancel-btn").Click();

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void OnInitialized_WithZeroCustomDays_UsesDefault()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Custom,
            CustomDays = 0
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var input = cut.Find("input[type=\"number\"]");
        int.TryParse(input.GetAttribute("value"), out var days).Should().BeTrue();
        days.Should().Be(Constants.Repeat.DefaultCustomDays);
    }

    [Fact]
    public void HandleSave_WithPausedRepeat_SetsIsPaused()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule { Type = RepeatType.Daily });
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        cut.Find(".tep-toggle").Click();
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.IsPaused.Should().BeTrue();
    }

    [Fact]
    public void ToggleWeekday_ClickAddsDay()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [DayOfWeek.Monday]
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var buttons = cut.FindAll(".tep-weekday-btn");
        var tuesdayBtn = buttons.First(b => b.TextContent.Contains("Tu"));
        tuesdayBtn.Click();

        cut.FindAll(".tep-weekday-btn.active").Count.Should().Be(2);
    }

    [Fact]
    public void ToggleWeekday_ClickRemovesDay()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Weekly,
            Weekdays = [DayOfWeek.Monday]
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var buttons = cut.FindAll(".tep-weekday-btn");
        var mondayBtn = buttons.First(b => b.TextContent.Contains("Mo"));
        mondayBtn.Click();

        cut.FindAll(".tep-weekday-btn.active").Count.Should().Be(0);
    }
}
