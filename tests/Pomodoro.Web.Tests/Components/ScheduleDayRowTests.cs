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
        var day = new ScheduleDay
        {
            Date = DateTime.Today,
            DayLabel = "Today",
            Items = []
        };

        var cut = RenderComponent<ScheduleDayRow>(p => p.Add(x => x.Day, day));

        cut.FindAll(".day-empty").Should().HaveCount(1);
    }

    [Fact]
    public void Render_LocalSubtasks_ShowsSubItemsAndCollapseToggle()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var all = new List<TaskItem> { root, child };

        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        cut.FindAll(".row-toggle").Should().HaveCount(1);
        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public void Render_GoogleSubtasks_ResolvesByGoogleParentTaskId()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "GRoot", CreatedAt = DateTime.UtcNow, GoogleTaskId = "g-root" };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "GChild", CreatedAt = DateTime.UtcNow, GoogleParentTaskId = "g-root" };
        var all = new List<TaskItem> { root, child };

        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        cut.Markup.Should().Contain("GChild");
    }

    [Fact]
    public void Render_Subtasks_ShowsChildInMarkup()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var grand = new TaskItem { Id = Guid.NewGuid(), Name = "Grand", CreatedAt = DateTime.UtcNow, ParentTaskId = child.Id };
        var all = new List<TaskItem> { root, child, grand };

        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, all));

        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public void Render_TaskWithEditButton()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Local", CreatedAt = DateTime.UtcNow };

        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root }));

        cut.FindAll("button[aria-label=\"Edit task\"]").Should().HaveCount(1);
    }

    [Fact]
    public void ToggleCollapse_Click_HidesThenShowsSubitems()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow, ParentTaskId = root.Id };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root, child }));

        cut.Find(".row-toggle").Click();
        cut.Render();

        cut.Markup.Should().NotContain("Child");

        cut.Find(".row-toggle").Click();
        cut.Render();

        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public void StartEdit_ThenSave_InvokesOnEditTask()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        TaskItem? edited = null;
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnEditTask, EventCallback.Factory.Create<TaskItem>(this, t => edited = t)));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Render();
        cut.Find(".tep-save-btn").Click();
        cut.Render();

        edited.Should().NotBeNull();
        cut.FindAll(".task-edit-panel").Should().HaveCount(0);
    }

    [Fact]
    public void StartEdit_ThenCancel_ClearsEditing()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnEditTask, EventCallback.Factory.Create<TaskItem>(this, _ => { })));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Render();
        cut.Find(".tep-cancel-btn").Click();
        cut.Render();

        cut.FindAll(".task-edit-panel").Should().HaveCount(0);
    }

    [Fact]
    public void StartAddSubtask_WithName_InvokesOnAddSubtask()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        AddSubtaskRequest? captured = null;
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));

        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find("input[aria-label=\"New subtask name\"]").Input("Kid");
        cut.Find("button[aria-label=\"Add\"]").Click();
        cut.Render();

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Kid");
        captured.ParentTaskId.Should().Be(root.Id);
    }

    [Fact]
    public void HandleSubtaskKeyPress_Escape_ClearsAddSubtaskForm()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, _ => { })));

        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find("input[aria-label=\"New subtask name\"]").KeyDown(Key.Escape);
        cut.Render();

        cut.FindAll(".add-subtask-form").Should().HaveCount(0);
    }

    [Fact]
    public void HandleSubtaskKeyPress_Enter_SubmitsSubtask()
    {
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        AddSubtaskRequest? captured = null;
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(x => x.Day, DayWithRoot(root))
            .Add(x => x.AllTasks, new List<TaskItem> { root })
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));

        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        var input = cut.Find("input[aria-label=\"New subtask name\"]");
        input.Input("Kid");
        input.KeyDown(Key.Enter);
        cut.Render();

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Kid");
        captured.ParentTaskId.Should().Be(root.Id);
    }
}
