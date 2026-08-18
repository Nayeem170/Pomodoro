using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public partial class TaskEditPanelTests
{
    #region Subtask mode (coverage)

    [Fact]
    public void SubtaskMode_RendersFullPanelWithoutListControls()
    {
        // Arrange - IsSubtask true (has ParentTaskId).
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.Repeat = new RepeatRule { Type = RepeatType.Daily };
        });

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - unified panel renders Name + Schedule + repeat select + actions; no list selector.
        cut.FindAll("textarea[aria-label=\"Task name\"]").Should().HaveCount(1);
        cut.FindAll("input[type=\"date\"]").Should().HaveCount(1);
        cut.FindAll(".tep-save-btn").Should().HaveCount(1);
        cut.FindAll(".tep-select").Should().HaveCount(1);
        cut.FindAll("select[aria-label=\"Task list\"]").Should().HaveCount(0);
    }

    [Fact]
    public void SubtaskMode_Enter_InvokesSave_AndPreservesRepeatAndSchedule()
    {
        // Arrange - subtask carries a parent-copied repeat rule and a scheduled date.
        var repeat = new RepeatRule { Type = RepeatType.Daily };
        var scheduled = new DateTime(2025, 1, 10);
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.Repeat = repeat;
            t.ScheduledDate = scheduled;
        });
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("textarea[aria-label=\"Task name\"]").KeyDown(Key.Enter);

        // Assert - save preserves the parent-copied repeat rule and scheduled date.
        saved.Should().NotBeNull();
        saved!.Repeat.Should().BeSameAs(repeat);
        saved.ScheduledDate.Should().Be(scheduled);
    }

    [Fact]
    public void SubtaskMode_Escape_InvokesCancel()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());
        var cancelled = false;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        // Act
        cut.Find("textarea[aria-label=\"Task name\"]").KeyDown(Key.Escape);

        // Assert
        cancelled.Should().BeTrue();
    }

    [Fact]
    public void SubtaskMode_Input_UpdatesEditNameAndPersistsOnSave()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act - type into the name input, then save via the Save button.
        cut.Find("textarea[aria-label=\"Task name\"]").Input("Renamed sub");
        cut.Find(".tep-save-btn").Click();

        // Assert - the typed name is captured on save.
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Renamed sub");
    }

    [Fact]
    public void RootMode_EnterOnName_DoesNotInvokeSave()
    {
        // Arrange
        var task = CreateTask();
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("textarea[aria-label=\"Task name\"]").KeyDown(Key.Enter);

        // Assert - Enter-to-save is subtask-only; roots use the Save button.
        saved.Should().BeNull();
    }

    #endregion

    #region Bug repro: subtask edit property parity (#161)

    [Fact]
    public void SubtaskMode_RendersScheduleInput()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - subtask edit exposes the Schedule date input.
        cut.FindAll("input[type=\"date\"]").Should().HaveCount(1);
    }

    [Fact]
    public void SubtaskMode_RendersSaveAndCancelActions()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - subtask edit has explicit Save and Cancel actions.
        cut.FindAll(".tep-save-btn").Should().HaveCount(1);
        cut.FindAll(".tep-cancel-btn").Should().HaveCount(1);
    }

    [Fact]
    public void SubtaskMode_Save_PersistsScheduledDate()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("input[type=\"date\"]").Change("2026-07-04");
        cut.Find(".tep-save-btn").Click();

        // Assert - the edited schedule date is persisted on save.
        saved.Should().NotBeNull();
        saved!.ScheduledDate.Should().Be(new DateTime(2026, 7, 4));
    }

    [Fact]
    public void SubtaskMode_DoesNotRenderRepeatOrListControls()
    {
        // Arrange - subtask following its parent's repeat.
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = true;
        });

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - scope guard for #162: repeat select present, no list selector, pause hidden while following.
        cut.FindAll(".tep-select").Should().HaveCount(1);
        cut.FindAll("select[aria-label=\"Task list\"]").Should().HaveCount(0);
        cut.FindAll(".tep-toggle").Should().HaveCount(0);
    }

    #endregion

    #region Scope extension: subtask repeat + list selector (#162)

    [Fact]
    public void SubtaskMode_RepeatSelect_RendersFollowParentOption()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - subtask repeat select exists with the leading Follow parent option.
        cut.FindAll("select.tep-select").Should().HaveCount(1);
        cut.Find("select.tep-select").TextContent.Should().Contain("Follow parent");
    }

    [Fact]
    public void SubtaskMode_FollowingParent_Preselected()
    {
        // Arrange
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = true;
        });

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert
        cut.Find("select.tep-select").GetAttribute("value").Should().Be(Constants.Repeat.FollowParentChoice);
    }

    [Fact]
    public void SubtaskMode_ChooseDaily_SetsOwnRepeatAndClearsFollow()
    {
        // Arrange
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = true;
        });
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("select.tep-select").Change("Daily");
        cut.Find(".tep-save-btn").Click();

        // Assert - one action sets the own rule and detaches from the parent's repeat.
        saved.Should().NotBeNull();
        saved!.Repeat.Should().NotBeNull();
        saved.Repeat!.Type.Should().Be(RepeatType.Daily);
        saved.FollowsParentRepeat.Should().BeFalse();
    }

    [Fact]
    public void SubtaskMode_FollowParentSave_PreservesRepeatRuleAndFlag()
    {
        // Arrange - follow-parent subtask carrying the parent-copied rule.
        var repeat = new RepeatRule { Type = RepeatType.Weekly };
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = true;
            t.Repeat = repeat;
        });
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find(".tep-save-btn").Click();

        // Assert
        saved.Should().NotBeNull();
        saved!.FollowsParentRepeat.Should().BeTrue();
        saved.Repeat.Should().BeSameAs(repeat);
    }

    [Fact]
    public void SubtaskMode_NoneSave_IsOneTime()
    {
        // Arrange
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = false;
            t.Repeat = new RepeatRule { Type = RepeatType.Daily };
        });
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("select.tep-select").Change("None");
        cut.Find(".tep-save-btn").Click();

        // Assert - None means a one-time subtask.
        saved.Should().NotBeNull();
        saved!.Repeat.Should().BeNull();
        saved.FollowsParentRepeat.Should().BeFalse();
    }

    [Fact]
    public void SubtaskMode_Pause_ShownWithOwnRepeat_HiddenWhileFollowing()
    {
        // Arrange - following parent: no pause control.
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.FollowsParentRepeat = true;
        });
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));
        cut.FindAll(".tep-toggle").Should().HaveCount(0);

        // Act - choose an own repeat.
        cut.Find("select.tep-select").Change("Daily");

        // Assert - pause unlocks for independently-repeating subtasks.
        cut.FindAll(".tep-toggle").Should().HaveCount(1);
    }

    [Fact]
    public void RootMode_NoFollowParentOption()
    {
        // Arrange
        var task = CreateTask();

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - Follow parent is a subtask-only concept.
        cut.Find("select.tep-select").TextContent.Should().NotContain("Follow parent");
    }

    [Fact]
    public void RootMode_ListSelect_UpdatesGoogleListId()
    {
        // Arrange
        var task = CreateTask(t => t.GoogleListId = "list-a");
        var lists = new List<TaskListRef>
        {
            new("list-a", "Work", "#fff", 0, true, false),
            new("list-b", "Personal", "#fff", 0, true, false)
        };
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.GoogleLists, lists)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("select[aria-label=\"Task list\"]").Change("list-b");
        cut.Find(".tep-save-btn").Click();

        // Assert
        saved.Should().NotBeNull();
        saved!.GoogleListId.Should().Be("list-b");
    }

    [Fact]
    public void RootMode_LocalListSelection_SavesNullGoogleListId()
    {
        // Arrange - task currently on a Google list.
        var task = CreateTask(t => t.GoogleListId = "list-a");
        var lists = new List<TaskListRef>
        {
            new("list-a", "Work", "#fff", 0, true, false)
        };
        TaskItem? saved = null;
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.GoogleLists, lists)
            .Add(x => x.OnSave, EventCallback.Factory.Create<TaskItem>(this, t => saved = t)));

        // Act
        cut.Find("select[aria-label=\"Task list\"]").Change(Constants.TaskLists.LocalPomodoroListId);
        cut.Find(".tep-save-btn").Click();

        // Assert - selecting the local list clears the Google list binding.
        saved.Should().NotBeNull();
        saved!.GoogleListId.Should().BeNull();
    }

    [Fact]
    public void SubtaskMode_NoListSelect()
    {
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());
        var lists = new List<TaskListRef>
        {
            new("list-a", "Work", "#fff", 0, true, false)
        };

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.GoogleLists, lists));

        // Assert - subtasks inherit the parent's list; no selector.
        cut.FindAll("select[aria-label=\"Task list\"]").Should().HaveCount(0);
    }

    #endregion
}
