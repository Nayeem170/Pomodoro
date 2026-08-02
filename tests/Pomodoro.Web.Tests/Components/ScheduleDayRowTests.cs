using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.Schedule;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class ScheduleDayRowTests : TestContext
{
    private static ScheduleDay DayWithRoot(TaskItem root, string title = "Root") =>
        new()
        {
            Date = DateTime.Today.AddDays(1),
            DayLabel = "Tomorrow",
            Items =
            [
                new ScheduleItem { Title = title, Task = root, TaskId = root.Id }
            ]
        };

    [Fact]
    public void Render_EmptyDay_ShowsDayEmptyPlaceholder()
    {
        // Arrange
        var day = new ScheduleDay
        {
            Date = DateTime.Today,
            DayLabel = "Today",
            Items = []
        };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p.Add(x => x.Day, day));

        // Assert
        cut.FindAll(".day-empty").Should().HaveCount(1);
    }

    [Fact]
    public void Render_LocalSubtasks_ShowsSubItemsAndCollapseToggle()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var all = new List<TaskItem> { root, child };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        // Assert
        cut.FindAll(".day-subitem").Should().HaveCount(1);
        cut.FindAll(".row-toggle").Should().HaveCount(1);
        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public void Render_GoogleSubtasks_ResolvesByGoogleParentTaskId()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "GRoot", CreatedAt = DateTime.UtcNow, GoogleTaskId = "g-root" };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "GChild", CreatedAt = DateTime.UtcNow, GoogleParentTaskId = "g-root" };
        var all = new List<TaskItem> { root, child };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        // Assert
        cut.FindAll(".day-subitem").Should().HaveCount(1);
        cut.Markup.Should().Contain("GChild");
    }

    [Fact]
    public void Render_GrandSubtasks_ShowsNestedSubItems()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var grand = new TaskItem { Id = Guid.NewGuid(), Name = "Grand", CreatedAt = DateTime.UtcNow, ParentTaskId = child.Id };
        var great = new TaskItem { Id = Guid.NewGuid(), Name = "Great", CreatedAt = DateTime.UtcNow, ParentTaskId = grand.Id };
        var all = new List<TaskItem> { root, child, grand, great };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        // Assert - grand and great-grand render in nested subitem rows.
        cut.FindAll(".day-subitem").Count.Should().BeGreaterThanOrEqualTo(3);
        cut.Markup.Should().Contain("Grand");
        cut.Markup.Should().Contain("Great");
    }

    [Fact]
    public void Render_GoogleTask_HidesAddSubtaskButton()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "G", CreatedAt = DateTime.UtcNow, GoogleTaskId = "g-1", GoogleListId = "glist-1" };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, _ => { })));

        // Assert
        cut.FindAll("button[aria-label=\"Add subtask\"]").Should().HaveCount(0);
    }

    [Fact]
    public void Render_NonGoogleTaskWithDelegate_ShowsAddSubtaskButton()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Local", CreatedAt = DateTime.UtcNow };

        // Act
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, _ => { })));

        // Assert
        cut.FindAll("button[aria-label=\"Add subtask\"]").Should().HaveCount(1);
    }

    [Fact]
    public void ToggleCollapse_Click_HidesSubitems()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root, child }));

        // Act
        cut.Find(".row-toggle").Click();
        cut.Render();

        // Assert
        cut.Markup.Should().NotContain("Child");
    }

    [Fact]
    public void StartEdit_ThenSave_InvokesOnEditTask()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        TaskItem? edited = null;
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnEditTask, EventCallback.Factory.Create<TaskItem>(this, t => edited = t)));

        // Act - open edit panel, then save (non-subtask Save button).
        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Render();
        cut.Find(".tep-save-btn").Click();
        cut.Render();

        // Assert
        edited.Should().NotBeNull();
        cut.FindAll(".day-edit").Should().HaveCount(0);
    }

    [Fact]
    public void StartEdit_ThenCancel_ClearsEditing()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnEditTask, EventCallback.Factory.Create<TaskItem>(this, _ => { })));

        // Act
        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Render();
        cut.Find(".tep-cancel-btn").Click();
        cut.Render();

        // Assert
        cut.FindAll(".day-edit").Should().HaveCount(0);
    }

    [Fact]
    public void StartAddSubtask_WithName_InvokesOnAddSubtask()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        AddSubtaskRequest? captured = null;
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));

        // Act
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find(".subtask-input").Input("Kid");
        cut.Find("button.subtask-btn.add").Click();
        cut.Render();

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Kid");
        captured.ParentTaskId.Should().Be(root.Id);
    }

    [Fact]
    public void HandleSubtaskKeyPress_Escape_ClearsAddSubtaskForm()
    {
        // Arrange
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, _ => { })));

        // Act
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find(".subtask-input").KeyDown(Key.Escape);
        cut.Render();

        // Assert
        cut.FindAll(".day-subtask-form").Should().HaveCount(0);
    }
}
