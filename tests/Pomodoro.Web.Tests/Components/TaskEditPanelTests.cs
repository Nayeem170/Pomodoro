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
    public void OnInitialized_WithYearlyRepeat_ShowsMonthSelectBeforeDaySelect()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Yearly,
            YearlyDay = 10,
            YearlyMonth = 3
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.Markup.Should().Contain("March");
        var selects = cut.FindAll("select.tep-input");
        selects.Count.Should().Be(2);
        selects[0].GetAttribute("value").Should().Be("3",
            "month select must be the first select (month-before-day order)");
        selects[1].GetAttribute("value").Should().Be("10",
            "day select must follow the month select");
        cut.FindAll("input[type=\"number\"]").Should().BeEmpty();
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
        cut.FindAll("select.tep-input")[0].Change("11");
        cut.FindAll("select.tep-input")[1].Change("9");
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
    public void Render_YearlyFebruaryDay29_ShowsLeapHint()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        var monthAndDay = cut.FindAll("select.tep-input");
        monthAndDay[0].Change("2");
        cut.FindAll("select.tep-input")[1].Change("29");

        cut.FindAll(".tep-hint").Count.Should().Be(1);
        cut.Markup.Should().Contain("Runs Feb 29 in leap years, Feb 28 otherwise.");
    }

    [Fact]
    public void Render_MonthlySelected_ShowsBySelectDefaultingToDayOfMonth()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Monthly");

        cut.Markup.Should().Contain("Day of month");
        cut.Markup.Should().Contain("Day of week");
        cut.FindAll("input[type=\"number\"]").Count.Should().Be(1);
        cut.Markup.Should().NotContain("First",
            "week ordinal select must be hidden in day-of-month mode");
    }

    [Fact]
    public void Render_MonthlyDayOfWeekMode_ShowsWeekSelectAndWeekdayButtons()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Monthly");
        cut.FindAll("select.tep-select")[1].Change("true");

        var weekSelect = cut.Find("select.tep-input");
        weekSelect.QuerySelectorAll("option").Should().HaveCount(5);
        cut.Markup.Should().Contain("First");
        cut.Markup.Should().Contain("Last");
        cut.FindAll(".tep-weekday-btn").Should().HaveCount(7);
        cut.FindAll("input[type=\"number\"]").Should().BeEmpty();
    }

    [Fact]
    public void HandleSave_MonthlyWeekdayMode_WritesWeekOfMonthAndWeekdays()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Monthly");
        cut.FindAll("select.tep-select")[1].Change("true");
        cut.Find("select.tep-input").Change("2");
        cut.FindAll(".tep-weekday-btn")[1].Click();
        cut.FindAll(".tep-weekday-btn")[3].Click();
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Monthly);
        savedTask.Repeat.WeekOfMonth.Should().Be(2);
        savedTask.Repeat.Weekdays.Should().BeEquivalentTo([DayOfWeek.Tuesday, DayOfWeek.Thursday]);
    }

    [Fact]
    public void HandleSave_QuarterlyWeekdayMode_WritesGroupWeekAndWeekdays()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Quarterly");
        cut.FindAll("select.tep-select")[1].Change("true");
        cut.FindAll("select.tep-input")[0].Change("3");
        cut.FindAll("select.tep-input")[1].Change("5");
        cut.FindAll(".tep-weekday-btn")[4].Click();
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Quarterly);
        savedTask.Repeat.QuarterlyMonth.Should().Be(3);
        savedTask.Repeat.WeekOfMonth.Should().Be(RepeatRule.LastWeekOfMonth);
        savedTask.Repeat.Weekdays.Should().BeEquivalentTo([DayOfWeek.Friday]);
    }

    [Fact]
    public void HandleSave_YearlyWeekdayMode_WritesMonthWeekAndWeekdays()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        cut.FindAll("select.tep-select")[1].Change("true");
        cut.FindAll("select.tep-input")[0].Change("3");
        cut.FindAll("select.tep-input")[1].Change("1");
        cut.FindAll(".tep-weekday-btn")[0].Click();
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Yearly);
        savedTask.Repeat.YearlyMonth.Should().Be(3);
        savedTask.Repeat.WeekOfMonth.Should().Be(1);
        savedTask.Repeat.Weekdays.Should().BeEquivalentTo([DayOfWeek.Monday]);
    }

    [Fact]
    public void OnInitialized_WithWeekdayModeRule_LoadsByWeekdayState()
    {
        var task = CreateTask(t => t.Repeat = new RepeatRule
        {
            Type = RepeatType.Quarterly,
            QuarterlyMonth = 2,
            WeekOfMonth = 2,
            Weekdays = [DayOfWeek.Friday]
        });
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        cut.FindAll("select.tep-select")[1].GetAttribute("value").Should().Be("true",
            "By select must show Day of week for a weekday-mode rule");
        var groupAndWeek = cut.FindAll("select.tep-input");
        groupAndWeek[1].GetAttribute("value").Should().Be("2");
        var active = cut.FindAll(".tep-weekday-btn.active");
        active.Should().HaveCount(1);
        active[0].TextContent.Should().Contain("Fr");
    }

    [Fact]
    public void HandleSave_WeekdayModeWithNoWeekdays_SavesNullWeekOfMonth()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Monthly");
        cut.FindAll("select.tep-select")[1].Change("true");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.WeekOfMonth.Should().BeNull(
            "day-of-week mode with no selected weekdays must degrade to day-of-month mode");
    }

    [Fact]
    public void HandleSave_WeekdayModeSwitchedToWeekly_SavesNullWeekOfMonth()
    {
        var task = CreateTask();
        TaskItem? savedTask = null;
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters
                .Add(p => p.Task, task)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => savedTask = t)));

        var select = cut.Find("select.tep-select");
        select.Change("Monthly");
        cut.FindAll("select.tep-select")[1].Change("true");
        cut.FindAll(".tep-weekday-btn")[0].Click();
        select.Change("Weekly");
        cut.Find(".tep-save-btn").Click();

        savedTask.Should().NotBeNull();
        savedTask!.Repeat.Should().NotBeNull();
        savedTask.Repeat!.Type.Should().Be(RepeatType.Weekly);
        savedTask.Repeat.WeekOfMonth.Should().BeNull(
            "switching to Weekly must clear the weekday-mode ordinal");
        savedTask.Repeat.Weekdays.Should().BeEquivalentTo([DayOfWeek.Monday]);
    }

    [Fact]
    public void Render_YearlyApril_ShowsThirtyDayOptionsAndNoHint()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        cut.FindAll("select.tep-input")[0].Change("4");

        cut.FindAll("select.tep-input")[1].QuerySelectorAll("option").Should().HaveCount(30);
        cut.FindAll(".tep-hint").Should().BeEmpty();
    }

    [Fact]
    public void Render_YearlyDayClamps_WhenMonthSwitchesToShorterMonth()
    {
        var task = CreateTask();
        var cut = RenderComponent<TaskEditPanel>(parameters =>
            parameters.Add(p => p.Task, task));

        var select = cut.Find("select.tep-select");
        select.Change("Yearly");
        var monthAndDay = cut.FindAll("select.tep-input");
        monthAndDay[0].Change("1");
        cut.FindAll("select.tep-input")[1].Change("31");
        cut.FindAll("select.tep-input")[0].Change("4");

        cut.FindAll("select.tep-input")[1].GetAttribute("value").Should().Be("30",
            "day must clamp to the shorter month's max when the month changes");
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
