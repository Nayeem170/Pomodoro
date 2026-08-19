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
        cut.FindAll(".day-badge")[0].TextContent.Trim().Should().Be(DateTime.Today.AddDays(1).Day.ToString());
        cut.FindAll(".day-weekday-full")[0].TextContent.Trim().Should().Be(DateTime.Today.AddDays(1).ToString("dddd"));
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
        cut.Markup.Should().Contain("repeat-badge");
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

        edited.Should().NotBeNull();
        edited!.Id.Should().Be(task.Id);
        edited.Name.Should().Be(task.Name);
    }

    #region Bug repro: schedule view reorder wiring (#165)

    private static ScheduleDay TaskBackedDay(params TaskItem[] tasks)
    {
        var older = DateTime.Now.AddDays(-10);
        for (var i = 0; i < tasks.Length; i++)
            tasks[i].CreatedAt = older.AddDays(-i);
        return new ScheduleDay
        {
            Date = DateTime.Today.AddDays(1),
            DayLabel = "Tomorrow",
            Items = tasks.Select(t => new ScheduleItem { Title = t.Name, Task = t }).ToList()
        };
    }

    [Fact]
    public void DayWithTwoTaskBackedItems_RootRowsAreDragReorderable()
    {
        // Arrange
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "First" };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "Second" };
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(a, b)])
            .Add(c => c.AllTasks, new List<TaskItem> { a, b }));

        // Act
        var rows = cut.FindAll(".task-row");

        // Assert - schedule rows expose the same drag affordance as the Tasks view.
        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(r => r.GetAttribute("draggable") == "true");
    }

    [Fact]
    public async Task ReorderWiring_KeyboardAltArrowDown_InvokesOnReorder()
    {
        // Arrange
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "First" };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "Second" };
        ReorderRequest? received = null;
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(a, b)])
            .Add(c => c.AllTasks, new List<TaskItem> { a, b })
            .Add(c => c.OnReorder, EventCallback.Factory.Create<ReorderRequest>(this, r => received = r)));

        // Act - Alt+ArrowDown on the first row (keyboard reorder path).
        await cut.InvokeAsync(() => cut.FindAll(".task-row")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs
        {
            Key = "ArrowDown",
            AltKey = true
        }));

        // Assert - the request reaches the page-level handler with move-after-next semantics.
        received.Should().NotBeNull();
        received!.TaskId.Should().Be(a.Id);
        received.TargetId.Should().Be(b.Id);
        received.InsertBefore.Should().BeFalse();
    }

    [Fact]
    public void SingleTaskDay_RendersNotDraggable()
    {
        // Arrange
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "Only" };
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(a)])
            .Add(c => c.AllTasks, new List<TaskItem> { a }));

        // Act
        var rows = cut.FindAll(".task-row");

        // Assert - a one-item day group is an edge no-op: no drag affordance.
        rows.Should().HaveCount(1);
        rows[0].GetAttribute("draggable").Should().Be("false");
    }

    [Fact]
    public void SubtaskRows_AreDraggableWithinRenderedSiblingGroup()
    {
        // Arrange - two subtasks under the day's single root.
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = new DateTime(2026, 1, 1) };
        var subA = new TaskItem { Id = Guid.NewGuid(), Name = "SubA", ParentTaskId = root.Id, CreatedAt = new DateTime(2026, 1, 2) };
        var subB = new TaskItem { Id = Guid.NewGuid(), Name = "SubB", ParentTaskId = root.Id, CreatedAt = new DateTime(2026, 1, 3) };
        var all = new List<TaskItem> { root, subA, subB };
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(root)])
            .Add(c => c.AllTasks, all));

        // Act
        var rows = cut.FindAll(".task-row");

        // Assert - the root is a one-item day group (edge: not draggable),
        // but both subtask rows reorder within their rendered sibling group.
        rows.Should().HaveCount(3);
        rows[0].GetAttribute("draggable").Should().Be("false");
        rows[1].GetAttribute("draggable").Should().Be("true");
        rows[2].GetAttribute("draggable").Should().Be("true");
    }

    [Fact]
    public void SubtaskRows_RespectSortOrderOverCreationOrder()
    {
        // Arrange - subB created later but sorted first via SortOrder.
        var root = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = new DateTime(2026, 1, 1) };
        var subA = new TaskItem { Id = Guid.NewGuid(), Name = "SubA", ParentTaskId = root.Id, SortOrder = 2000, CreatedAt = new DateTime(2026, 1, 2) };
        var subB = new TaskItem { Id = Guid.NewGuid(), Name = "SubB", ParentTaskId = root.Id, SortOrder = 1000, CreatedAt = new DateTime(2026, 1, 3) };
        var all = new List<TaskItem> { root, subA, subB };
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(root)])
            .Add(c => c.AllTasks, all));

        // Act
        var names = cut.FindAll(".task-row")
            .Select(r => r.QuerySelector(".task-text")!.TextContent.Trim())
            .ToList();

        // Assert - subB (lower SortOrder) renders before SubA despite being created later.
        names.Should().Equal(["Root", "SubB", "SubA"]);
    }

    [Fact]
    public async Task DragStartOnDayItem_ActivatesDropZones_OnSiblingRows()
    {
        // Arrange
        var a = new TaskItem { Id = Guid.NewGuid(), Name = "First" };
        var b = new TaskItem { Id = Guid.NewGuid(), Name = "Second" };
        var cut = RenderComponent<ScheduleAgenda>(p => p
            .Add(c => c.Days, [TaskBackedDay(a, b)])
            .Add(c => c.AllTasks, new List<TaskItem> { a, b }));

        cut.FindAll(".drop-zone").Should().BeEmpty();

        // Act - start dragging the first row; the non-dragged sibling shows drop zones.
        await cut.FindAll(".task-row")[0].TriggerEventAsync("ondragstart", new Microsoft.AspNetCore.Components.Web.DragEventArgs());
        cut.FindAll(".drop-zone").Should().NotBeEmpty();

        // Act - ending the drag clears the active state.
        await cut.FindAll(".task-row")[0].TriggerEventAsync("ondragend", new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        // Assert
        cut.FindAll(".drop-zone").Should().BeEmpty();
    }

    [Fact]
    public void ResolveParentKey_ReturnsNull_WhenTaskHasNoParentIndicators()
    {
        // Arrange - defensive fallback: a task with neither ParentTaskId nor a
        // resolvable GoogleParentTaskId falls back to the root group.
        var orphan = new TaskItem { Id = Guid.NewGuid(), Name = "Orphan" };
        var cut = RenderComponent<ScheduleDayRow>(p => p
            .Add(c => c.Day, TaskBackedDay(orphan))
            .Add(c => c.AllTasks, new List<TaskItem> { orphan }));

        var method = typeof(ScheduleDayRow).GetMethod(
            "ResolveParentKey",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var result = (Guid?)method!.Invoke(cut.Instance, new object[] { orphan });

        result.Should().BeNull();
    }

    #endregion
}
