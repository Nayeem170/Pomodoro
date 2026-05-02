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
        badge.ClassList.Should().NotContain("repeat-paused");
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

        var badge = cut.Find(".task-badge");
        badge.ClassList.Should().Contain("repeat-paused");
    }
}

