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
    public void SubtaskMode_RendersFullPanelWithoutRepeatControls()
    {
        // Arrange - IsSubtask true (has ParentTaskId).
        var task = CreateTask(t =>
        {
            t.ParentTaskId = Guid.NewGuid();
            t.Repeat = new RepeatRule { Type = RepeatType.Daily };
        });

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - unified panel renders Name + Schedule + actions; no repeat/list/pause controls.
        cut.FindAll("textarea[aria-label=\"Task name\"]").Should().HaveCount(1);
        cut.FindAll("input[type=\"date\"]").Should().HaveCount(1);
        cut.FindAll(".tep-save-btn").Should().HaveCount(1);
        cut.FindAll(".tep-select").Should().HaveCount(0);
        cut.FindAll(".tep-toggle").Should().HaveCount(0);
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
        // Arrange
        var task = CreateTask(t => t.ParentTaskId = Guid.NewGuid());

        // Act
        var cut = RenderComponent<TaskEditPanel>(p => p.Add(x => x.Task, task));

        // Assert - subtasks get no repeat/list/pause controls (scope guard for #161).
        cut.FindAll(".tep-select").Should().HaveCount(0);
        cut.FindAll(".tep-toggle").Should().HaveCount(0);
    }

    #endregion
}
