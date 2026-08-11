using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

/// <summary>
/// Tests for TaskItemComponent.
/// </summary>
[Trait("Category", "Component")]
public class TaskItemComponentTests : TestContext
{
    [Fact]
    public void TaskItemComponent_RendersCorrectly()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("Test Task", cut.Markup);
        Assert.Contains("task-row", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysTaskName()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "My Important Task",
            TotalFocusMinutes = 30,
            PomodoroCount = 1,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("My Important Task", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysTotalFocusMinutes()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 45,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("45m", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysPomodoroCount()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 3,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("3", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_WhenCompleted_ShowsUndoButton()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = true
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("completed", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_WhenNotCompleted_ShowsCompleteButton()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.DoesNotContain("completed", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_AlwaysShowsDeleteButton()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.NotNull(cut.Find("button[aria-label=\"Delete\"]"));
    }

    [Fact]
    public void TaskItemComponent_WhenNotSelected_DoesNotApplySelectedClass()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.IsSelected, false));

        // Assert
        Assert.DoesNotContain("selected", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_WhenSelected_ShowsSelectedBadge()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.IsSelected, true));

        // Assert
    }

    [Fact]
    public void TaskItemComponent_WhenNotSelected_DoesNotShowSelectedBadge()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.IsSelected, false));

        // Assert
        Assert.DoesNotContain("selected", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_HasTaskActionsElement()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.NotNull(cut.Find(".task-actions"));
    }

    [Fact]
    public void TaskItemComponent_DisplaysAllThreeStats()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 45,
            PomodoroCount = 3,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("Test Task", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysCorrectTimeFormatForMinutes()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("25m", cut.Markup);
        Assert.DoesNotContain("0h", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysCorrectTimeFormatForHours()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 120, // 2 hours
            PomodoroCount = 4,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("2h", cut.Markup);
        Assert.Contains("0m", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysCorrectTimeFormatForHoursAndMinutes()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 150, // 2 hours 30 minutes
            PomodoroCount = 3,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("2h", cut.Markup);
        Assert.Contains("30m", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysDefaultEmojiForNewTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 0,
            PomodoroCount = 0,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("Test Task", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysHasPomodorosEmojiForTaskWithPomodoros()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("Test Task", cut.Markup);
    }

    [Fact]
    public void TaskItemComponent_DisplaysCompletedEmojiForCompletedTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = true
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        // Assert
        Assert.Contains("completed", cut.Markup);
    }

    [Fact]
    public async Task TaskItemComponent_ClickComplete_InvokesOnCompleteCallback()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        Guid? completedId = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnComplete, EventCallback.Factory.Create<Guid>(this, id => completedId = id)));

        cut.Find("button[aria-label=\"Complete\"]").Click();

        Assert.Equal(taskId, completedId);
    }

    [Fact]
    public async Task TaskItemComponent_ClickDelete_InvokesOnDeleteCallback()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        Guid? deletedId = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnDelete, EventCallback.Factory.Create<Guid>(this, id => deletedId = id)));

        cut.Find("button[aria-label=\"Delete\"]").Click();

        Assert.Equal(taskId, deletedId);
    }

    [Fact]
    public async Task TaskItemComponent_ClickUncomplete_InvokesOnUncompleteCallback()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = true
        };

        Guid? uncompletedId = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnUncomplete, EventCallback.Factory.Create<Guid>(this, id => uncompletedId = id)));

        cut.Find("button[aria-label=\"Undo\"]").Click();

        Assert.Equal(taskId, uncompletedId);
    }

    [Fact]
    public async Task TaskItemComponent_HandleKeyDown_Enter_InvokesOnSelectCallback()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        Guid? selectedId = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selectedId = id)));

        var taskItem = cut.Find(".task-row");
        taskItem.KeyDown("Enter");

        Assert.Equal(taskId, selectedId);
    }

    [Fact]
    public async Task TaskItemComponent_HandleKeyDown_Space_InvokesOnSelectCallback()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        Guid? selectedId = null;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selectedId = id)));

        var taskItem = cut.Find(".task-row");
        taskItem.KeyDown(" ");

        Assert.Equal(taskId, selectedId);
    }

    [Fact]
    public async Task TaskItemComponent_HandleKeyDown_OtherKey_DoesNotInvokeOnSelectCallback()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false
        };

        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, _ => selected = true)));

        var taskItem = cut.Find(".task-row");
        taskItem.KeyDown("Tab");

        Assert.False(selected);
    }

    [Fact]
    public async Task TaskItemComponent_HandleKeyDown_Enter_WhenCompleted_DoesNotInvokeOnSelectCallback()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = true
        };

        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters
                .Add(p => p.Item, task)
                .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, _ => selected = true)));

        var taskItem = cut.Find(".task-row");
        taskItem.KeyDown("Enter");

        Assert.False(selected);
    }

    [Fact]
    public void TaskItemComponent_NonPausedRepeat_ShowsTooltipWithoutPaused()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false,
            Repeat = new RepeatRule { Type = RepeatType.Daily }
        };

        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        var badge = cut.Find(".task-badge");
        badge.GetAttribute("title").Should().Be("Daily");
        badge.ClassList.Should().Contain("repeat-badge");
    }

    [Fact]
    public void TaskItemComponent_WithNullRepeat_DoesNotShowBadge()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false,
            Repeat = null
        };

        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        cut.FindAll(".task-badge").Count.Should().Be(0);
    }

    [Fact]
    public void TaskItemComponent_WithPausedRepeat_ShowsPausedBadge()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            TotalFocusMinutes = 25,
            PomodoroCount = 2,
            IsCompleted = false,
            Repeat = new RepeatRule { Type = RepeatType.Daily, IsPaused = true }
        };

        var cut = RenderComponent<TaskItemComponent>(parameters =>
            parameters.Add(p => p.Item, task));

        var badge = cut.Find(".paused-badge");
        badge.ClassList.Should().Contain("paused-badge");
    }

    #region Subtask / reparent / collapse handlers (coverage)

    [Fact]
    public void GoogleBadgeTooltip_WithGoogleListTitle_IncludesTitle()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "G Task",
            GoogleTaskId = "g-1",
            GoogleListId = "glist-1"
        };

        // Act
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.GoogleListTitle, "Personal"));

        // Assert
        var gtag = cut.Find(".google-badge");
        gtag.GetAttribute("title").Should().Contain("Personal");
    }

    [Fact]
    public void StartAddSubtask_OpensForm_ThenCancelClearsIt()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));

        // Act - open
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.FindAll(".add-subtask-form").Should().HaveCount(1);

        // Act - cancel
        cut.Find("button[aria-label=\"Cancel\"]").Click();
        cut.Render();

        // Assert
        cut.FindAll(".add-subtask-form").Should().HaveCount(0);
    }

    [Fact]
    public void HandleAddSubtask_WithName_InvokesCallbackAndClosesForm()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task", CreatedAt = DateTime.UtcNow };
        AddSubtaskRequest? captured = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));

        // Act
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find("input[aria-label=\"New subtask name\"]").Input("Kid");
        cut.Find("button[aria-label=\"Add\"]").Click();
        cut.Render();

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Kid");
        captured.ParentTaskId.Should().Be(task.Id);
        cut.FindAll(".add-subtask-form").Should().HaveCount(0);
    }

    [Fact]
    public void HandleSubtaskKeyPress_Enter_SubmitsSubtask()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task", CreatedAt = DateTime.UtcNow };
        AddSubtaskRequest? captured = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();

        // Act
        var input = cut.Find("input[aria-label=\"New subtask name\"]");
        input.Input("Via enter");
        input.KeyDown(Key.Enter);
        cut.Render();

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Via enter");
    }

    [Fact]
    public void HandleSubtaskKeyPress_Escape_ClosesForm()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<TaskItemComponent>(p => p.Add(x => x.Item, task));
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();

        // Act
        cut.Find("input[aria-label=\"New subtask name\"]").KeyDown(Key.Escape);
        cut.Render();

        // Assert
        cut.FindAll(".add-subtask-form").Should().HaveCount(0);
    }

    [Fact]
    public void HandleReparentToRoot_InvokesCallback()
    {
        // Arrange - Depth > 0 renders the "Promote" button.
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Child task", CreatedAt = DateTime.UtcNow };
        Guid? moved = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.Depth, 1)
            .Add(x => x.OnReparentToRoot, EventCallback.Factory.Create<Guid>(this, id => moved = id)));

        // Act
        cut.Find("button[aria-label=\"Promote\"]").Click();

        // Assert
        moved.Should().Be(task.Id);
    }

    [Fact]
    public void HandleDemote_InvokesCallback()
    {
        // Arrange - task with siblings renders the "Demote" button.
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task A", CreatedAt = DateTime.UtcNow };
        var sibling = new TaskItem { Id = Guid.NewGuid(), Name = "Task B", CreatedAt = DateTime.UtcNow.AddSeconds(1) };
        DemoteRequest? captured = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.Depth, 0)
            .Add(x => x.Siblings, new List<TaskItem> { sibling })
            .Add(x => x.OnDemote, EventCallback.Factory.Create<DemoteRequest>(this, r => captured = r)));

        // Act - open dropdown then select sibling.
        cut.Find("button[aria-label=\"Demote\"]").Click();
        cut.Find(".demote-option").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.TaskId.Should().Be(task.Id);
        captured.TargetSiblingId.Should().Be(sibling.Id);
    }

    [Fact]
    public void HandleToggleCollapse_InvokesCallback()
    {
        // Arrange - HasChildren renders the row-toggle button.
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Parent", CreatedAt = DateTime.UtcNow };
        Guid? toggled = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.HasChildren, true)
            .Add(x => x.OnToggleCollapse, EventCallback.Factory.Create<Guid>(this, id => toggled = id)));

        // Act
        cut.Find(".row-toggle").Click();

        // Assert
        toggled.Should().Be(task.Id);
    }

    #endregion

    #region Delete Confirmation

    [Fact]
    public void HandleDelete_ShowsConfirmation_WhenHasChildren()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Parent", IsCompleted = false };
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.HasChildren, true));

        cut.Find("button[aria-label='Delete']").Click();

        cut.Markup.Should().Contain("delete-confirm");
    }

    [Fact]
    public void CancelDelete_HidesConfirmation()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Parent", IsCompleted = false };
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.HasChildren, true));

        cut.Find("button[aria-label='Delete']").Click();
        cut.Find(".delete-confirm-cancel").Click();

        cut.Markup.Should().NotContain("delete-confirm");
    }

    [Fact]
    public void ConfirmDelete_InvokesOnDelete()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Parent", IsCompleted = false };
        Guid? deletedId = null;
        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.HasChildren, true)
            .Add(x => x.OnDelete, EventCallback.Factory.Create<Guid>(this, id => deletedId = id)));

        cut.Find("button[aria-label='Delete']").Click();
        cut.Find(".delete-confirm-go").Click();

        deletedId.Should().Be(task.Id);
    }

    #endregion

    #region Repeat Label Edge Cases

    [Fact]
    public void RendersWithoutError_WhenRepeatTypeIsUnexpected()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Edge",
            Repeat = new RepeatRule { Type = (RepeatType)999 }
        };

        var cut = RenderComponent<TaskItemComponent>(p => p
            .Add(x => x.Item, task)
            .Add(x => x.HasChildren, false));

        cut.Markup.Should().Contain("Edge");
    }

    #endregion
}

