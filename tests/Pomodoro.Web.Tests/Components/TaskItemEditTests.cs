using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TaskItemEditTests : TestContext
{
    [Fact]
    public void HandleEdit_ShowsEditPanel()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test" };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find("button[aria-label=\"Edit task\"]").Click();

        cut.Markup.Should().Contain("task-edit-panel");
    }

    [Fact]
    public void HandleEditSave_InvokesOnEditAndClosesPanel()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test" };
        TaskItem? editedTask = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnEdit, EventCallback.Factory.Create<TaskItem>(this, t => editedTask = t)));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Find(".tep-save-btn").Click();

        editedTask.Should().NotBeNull();
        editedTask!.Id.Should().Be(task.Id);
        cut.Markup.Should().NotContain("task-edit-panel");
    }

    [Fact]
    public void HandleEditCancel_ClosesPanelWithoutInvokingOnEdit()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test" };
        var editFired = false;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnEdit, EventCallback.Factory.Create<TaskItem>(this, _ => editFired = true)));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Find(".tep-cancel-btn").Click();

        editFired.Should().BeFalse();
        cut.Markup.Should().NotContain("task-edit-panel");
    }

    [Fact]
    public void DoubleClick_StartsInlineEdit()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "My Task" };
        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));

        cut.Find(".task-text").DoubleClick();

        cut.Markup.Should().Contain("task-text-input");
        cut.Markup.Should().NotContain("task-edit-panel");
    }

    [Fact]
    public void Subtask_EditButton_ShowsEditPanelNotInlineEdit()
    {
        // Arrange - subtask row (Depth 1, ParentTaskId set).
        var parent = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Sub", ParentTaskId = parent };
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.Depth, 1));

        // Act
        cut.Find("button[aria-label=\"Edit task\"]").Click();

        // Assert - pencil opens the full edit panel for subtasks; double-click remains the inline rename.
        cut.Markup.Should().Contain("task-edit-panel");
        cut.Markup.Should().NotContain("task-text-input");
    }

    [Fact]
    public void InlineEdit_Enter_SavesAndInvokesOnEdit()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Old" };
        TaskItem? edited = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.OnEdit, EventCallback.Factory.Create<TaskItem>(this, t => edited = t)));

        cut.Find(".task-text").DoubleClick();
        cut.Find(".task-text-input").Input("New Name");
        cut.Find(".task-text-input").KeyDown(Key.Enter);

        edited.Should().NotBeNull();
        edited!.Name.Should().Be("New Name");
        cut.Markup.Should().NotContain("task-text-input");
    }

    [Fact]
    public void InlineEdit_Escape_CancelsWithoutInvokingOnEdit()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Original" };
        var editFired = false;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.OnEdit, EventCallback.Factory.Create<TaskItem>(this, _ => editFired = true)));

        cut.Find(".task-text").DoubleClick();
        cut.Find(".task-text-input").Input("Changed");
        cut.Find(".task-text-input").KeyDown(Key.Escape);

        editFired.Should().BeFalse();
        cut.Markup.Should().NotContain("task-text-input");
        task.Name.Should().Be("Original");
    }

    [Fact]
    public void InlineEdit_EmptyName_CancelsWithoutInvokingOnEdit()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Keep" };
        var editFired = false;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.OnEdit, EventCallback.Factory.Create<TaskItem>(this, _ => editFired = true)));

        cut.Find(".task-text").DoubleClick();
        cut.Find(".task-text-input").Input("   ");
        cut.Find(".task-text-input").KeyDown(Key.Enter);

        editFired.Should().BeFalse();
        cut.Markup.Should().NotContain("task-text-input");
    }

    [Fact]
    public void Render_WithRecurringTask_ShowsRepeatBadge()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Recurring Task",
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Markup.Should().Contain("repeat-badge");
    }

    [Fact]
    public void Render_WithPausedRepeat_ShowsPausedClass()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Paused Task",
            Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = true }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Markup.Should().Contain("repeat-badge");
        cut.Markup.Should().Contain("paused-badge");
    }

    [Fact]
    public void GetRepeatTooltip_Daily()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find(".repeat-badge").GetAttribute("title").Should().Be("Daily");
    }

    [Fact]
    public void GetRepeatTooltip_Weekly()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Repeat = new RepeatRule { Type = RepeatType.Weekly }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find(".repeat-badge").GetAttribute("title").Should().Be("Weekly");
    }

    [Fact]
    public void GetRepeatTooltip_Custom()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Repeat = new RepeatRule { Type = RepeatType.Custom, CustomDays = 5 }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find(".repeat-badge").GetAttribute("title").Should().Be("Every 5 days");
    }

    [Fact]
    public void GetRepeatTooltip_Monthly()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Repeat = new RepeatRule { Type = RepeatType.Monthly, MonthlyDay = 15 }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find(".repeat-badge").GetAttribute("title").Should().Be("Monthly (day 15)");
    }

    [Fact]
    public void GetRepeatTooltip_PausedSuffix()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = true }
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Find(".repeat-badge").GetAttribute("title").Should().Be("Daily (paused)");
    }

    [Fact]
    public void Render_WithScheduledDate_ShowsScheduleBadge()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Scheduled Task",
            ScheduledDate = new DateTime(2026, 6, 15)
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Markup.Should().Contain("schedule-badge");
    }

    [Fact]
    public void Render_WithRecurringAndScheduled_ShowsBothBadges()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Both Task",
            Repeat = new RepeatRule { Type = RepeatType.Daily },
            ScheduledDate = new DateTime(2026, 6, 15)
        };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Markup.Should().Contain("repeat-badge");
        cut.Markup.Should().Contain("schedule-badge");
    }

    [Fact]
    public void Render_WithoutRepeatOrSchedule_ShowsNoBadges()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Plain Task" };
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.Markup.Should().NotContain("repeat-badge");
        cut.Markup.Should().NotContain("schedule-badge");
    }

}
