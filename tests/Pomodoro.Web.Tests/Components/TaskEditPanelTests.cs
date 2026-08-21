using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public partial class TaskEditPanelTests : TestContext
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

        var select = cut.Find("select.tep-select");
        select.GetAttribute("value").Should().Be(RepeatType.Daily.ToString());
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
    public void OnInitialized_WithQuarterlyRepeat_ShowsGroupSelectAndDayInputWithValue()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Quarterly,
            QuarterlyDay = 15,
            QuarterlyMonth = 2
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("of month");
        cut.Markup.Should().Contain("Feb, May, Aug, Nov");
        var groupSelect = cut.Find("select.tep-input");
        groupSelect.GetAttribute("value").Should().Be("2");
        var input = cut.Find("input[type=\"number\"]");
        input.GetAttribute("value").Should().Be("15");
    }

    [Fact]
    public void OnInitialized_WithYearlyRepeat_ShowsDayInputAndMonthSelect()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Yearly,
            YearlyDay = 10,
            YearlyMonth = 3
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("of month");
        cut.Markup.Should().Contain("March");
        var inputs = cut.FindAll("input[type=\"number\"]");
        inputs.Count.Should().Be(1);
        inputs[0].GetAttribute("value").Should().Be("10");
        var monthSelect = cut.Find("select.tep-input");
        monthSelect.GetAttribute("value").Should().Be("3");
    }

    [Fact]
    public void OnInitialized_WithNullQuarterlyYearlyFields_DefaultsToAnchor()
    {
        var anchor = new DateTime(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc);
        var task = CreateTask(t =>
        {
            t.CreatedAt = anchor;
            t.Repeat = new RepeatRule { Type = RepeatType.Quarterly };
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var input = cut.Find("input[type=\"number\"]");
        input.GetAttribute("value").Should().Be("7");
        var groupSelect = cut.Find("select.tep-input");
        groupSelect.GetAttribute("value").Should().Be("2");
    }

    [Fact]
    public void HandleSave_SelectQuarterly_WritesQuarterlyDay()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Quarterly");
        var input = cut.Find("input[type=\"number\"]");
        input.Input("20");
        cut.Find("select.tep-input").Change("3");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Quarterly);
        savedTask.Repeat.QuarterlyDay.Should().Be(20);
        savedTask.Repeat.QuarterlyMonth.Should().Be(3);
        savedTask.Repeat.YearlyDay.Should().Be(DateTime.Now.Day);
        savedTask.Repeat.YearlyMonth.Should().Be(DateTime.Now.Month);
    }

    [Fact]
    public void HandleSave_SelectYearly_WritesDayAndMonth()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        cut.Find("input[type=\"number\"]").Input("9");
        cut.Find("select.tep-input").Change("11");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Yearly);
        savedTask.Repeat.YearlyDay.Should().Be(9);
        savedTask.Repeat.YearlyMonth.Should().Be(11);
    }

    [Fact]
    public void HandleSave_FromNullRepeatWithQuarterly_CreatesRuleWithQuarterlyDay()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Quarterly");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Quarterly);
        savedTask.Repeat.QuarterlyDay.Should().BeGreaterThan(0);
        savedTask.Repeat.QuarterlyMonth.Should().BeInRange(1, 3);
    }

    [Fact]
    public void HandleSave_FromNullRepeatWithYearly_CreatesRuleWithDayAndMonth()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Yearly);
        savedTask.Repeat.YearlyDay.Should().BeGreaterThan(0);
        savedTask.Repeat.YearlyMonth.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Render_QuarterlyDayAbove28_ShowsClampHint()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Quarterly");
        cut.Find("input[type=\"number\"]").Input("31");

        cut.FindAll(".tep-hint").Count.Should().Be(1);
        cut.Markup.Should().Contain("Runs on the last day of shorter months.");
    }

    [Fact]
    public void Render_QuarterlyDayAtOrBelow28_HidesClampHint()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Quarterly");
        cut.Find("input[type=\"number\"]").Input("15");

        cut.FindAll(".tep-hint").Should().BeEmpty();
    }

    [Fact]
    public void Render_YearlyDayAbove28_ShowsClampHint()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        cut.Find("input[type=\"number\"]").Input("29");

        cut.FindAll(".tep-hint").Count.Should().Be(1);
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
