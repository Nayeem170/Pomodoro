using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Schedule;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class ScheduleAgendaTests : TestContext
{
    private static IReadOnlyList<ScheduleDay> SampleDays() =>
    [
        new ScheduleDay
        {
            Date = DateTime.Today.AddDays(1),
            DayLabel = "Tue 29 Jul",
            Items =
            [
                new ScheduleItem { Title = "Dentist" },
                new ScheduleItem { Title = "Standup", IsRepeat = true, RepeatLabel = "Daily" },
                new ScheduleItem { Title = "Sync", IsGoogle = true },
                new ScheduleItem { Title = "Done", IsCompleted = true }
            ]
        },
        new ScheduleDay
        {
            Date = DateTime.Today.AddDays(2),
            DayLabel = "Wed 30 Jul",
            Items = []
        }
    ];

    [Fact]
    public void Renders_DayHeader_PerDay()
    {
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, SampleDays())
            .Add(c => c.WindowLabel, "29 Jul - 4 Aug"));

        cut.FindAll(".day-header").Should().HaveCount(2);
        cut.FindAll(".day-header")[0].TextContent.Should().Contain("Tue 29 Jul");
    }

    [Fact]
    public void Renders_DoneAndEmptyStates()
    {
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, SampleDays())
            .Add(c => c.WindowLabel, "x"));

        cut.FindAll(".day-item.done").Should().HaveCount(1);
        cut.FindAll(".day-empty").Should().HaveCount(1);
    }

    [Fact]
    public void NextButton_InvokesOnNext()
    {
        var called = false;
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, SampleDays())
            .Add(c => c.OnNext, EventCallback.Factory.Create(this, () => called = true)));

        cut.Find("button[aria-label=\"Next week\"]").Click();

        called.Should().BeTrue();
    }

    [Fact]
    public void PrevButton_Disabled_WhenCanGoPrevFalse()
    {
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, SampleDays())
            .Add(c => c.CanGoPrev, false));

        var prev = cut.Find("button[aria-label=\"Previous week\"]");
        prev.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void PrevButton_InvokesOnPrev_WhenEnabled()
    {
        var called = false;
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, SampleDays())
            .Add(c => c.CanGoPrev, true)
            .Add(c => c.OnPrev, EventCallback.Factory.Create(this, () => called = true)));

        cut.Find("button[aria-label=\"Previous week\"]").Click();

        called.Should().BeTrue();
    }

    [Fact]
    public void EmptyDays_ShowsEmptyMessage()
    {
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, new List<ScheduleDay>())
            .Add(c => c.WindowLabel, "x"));

        cut.FindAll(".sched-empty").Should().HaveCount(1);
    }

    [Fact]
    public void ItemWithTask_RendersTaskRow()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Pay bills" };
        var days = new List<ScheduleDay>
        {
            new()
            {
                Date = DateTime.Today.AddDays(1),
                DayLabel = "Tomorrow",
                Items = [new ScheduleItem { Title = "Pay bills", Task = task }]
            }
        };

        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, days)
            .Add(c => c.WindowLabel, "x"));

        cut.FindAll("button[aria-label=\"Edit task\"]").Should().HaveCount(1);
        cut.Markup.Should().Contain("Pay bills");
    }

    [Fact]
    public void RepeatItem_RendersBadgeAndEditButton()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Standup",
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        };
        var days = new List<ScheduleDay>
        {
            new()
            {
                Date = DateTime.Today.AddDays(1),
                DayLabel = "Tomorrow",
                Items = [new ScheduleItem { Title = "Daily standup", IsRepeat = true, RepeatLabel = "Daily", Task = task }]
            }
        };

        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, days)
            .Add(c => c.WindowLabel, "x"));

        cut.FindAll("button[aria-label=\"Edit task\"]").Should().HaveCount(1);
        cut.Markup.Should().Contain("task-repeat");
    }

    [Fact]
    public void EditButton_OpensInlineEditPanel()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Pay bills" };
        var days = new List<ScheduleDay>
        {
            new()
            {
                Date = DateTime.Today.AddDays(1),
                DayLabel = "Tomorrow",
                Items = [new ScheduleItem { Title = "Pay bills", Task = task }]
            }
        };

        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, days)
            .Add(c => c.WindowLabel, "x"));

        cut.Find("button[aria-label=\"Edit task\"]").Click();

        cut.Find(".task-edit-panel").Should().NotBeNull();
    }

    [Fact]
    public void SaveButton_InvokesOnEditTask()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Pay bills" };
        TaskItem? edited = null;
        var days = new List<ScheduleDay>
        {
            new()
            {
                Date = DateTime.Today.AddDays(1),
                DayLabel = "Tomorrow",
                Items = [new ScheduleItem { Title = "Pay bills", Task = task }]
            }
        };

        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, days)
            .Add(c => c.OnEditTask, EventCallback.Factory.Create<TaskItem>(this, t => edited = t)));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Find(".tep-save-btn").Click();

        edited.Should().Be(task);
    }
}
