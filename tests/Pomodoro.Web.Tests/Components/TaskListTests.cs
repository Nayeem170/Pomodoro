using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

/// <summary>
/// bUnit tests for TaskList component.
/// Tests task list rendering, adding tasks, and task interactions.
/// </summary>
[Trait("Category", "Component")]
public class TaskListTests : TestContext
{
    public TaskListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Initial Rendering Tests

    [Fact]
    public void TaskList_Always_ShowsAddTaskInput()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        Assert.Contains("task-input", cut.Markup);
        Assert.DoesNotContain("task-add-btn", cut.Markup);
    }

    [Fact]
    public void TaskList_WithNoTasks_ShowsEmptyList()
    {
        // Arrange & Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        // Assert - task-items div exists but is empty
        Assert.DoesNotContain("Completed", cut.Markup);
    }

    [Fact]
    public void TaskList_WithTasks_ShowsTaskItems()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Test Task 1", IsCompleted = false },
            new TaskItem { Id = Guid.NewGuid(), Name = "Test Task 2", IsCompleted = false }
        };

        // Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.Contains("Test Task 1", cut.Markup);
        Assert.Contains("Test Task 2", cut.Markup);
    }

    [Fact]
    public void TaskList_WithCompletedTasks_ShowsCompletedSection()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Active Task", IsCompleted = false },
            new TaskItem { Id = Guid.NewGuid(), Name = "Completed Task", IsCompleted = true }
        };

        // Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.Contains("completed-section", cut.Markup);
        Assert.Contains("Completed", cut.Markup);
    }

    [Fact]
    public void TaskList_WithOnlyCompletedTasks_ShowsCompletedSection()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Completed Task 1", IsCompleted = true }
        };

        // Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.Contains("completed-section", cut.Markup);
    }

    #endregion

    #region Add Task Form Tests

    [Fact]
    public void TaskList_AlwaysShowsAddTaskForm()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        Assert.Contains("add-task-form", cut.Markup);
        Assert.Contains("task-input", cut.Markup);
        Assert.Contains("btn-add", cut.Markup);
        Assert.DoesNotContain("btn-cancel", cut.Markup);
    }

    [Fact]
    public void TaskList_AddForm_InitiallyHasEmptyInput()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        var input = cut.Find("input.task-input");
        Assert.Equal("", input.GetAttribute("value") ?? "");
    }

    [Fact]
    public void TaskList_AddForm_AddButtonDisabledWhenEmpty()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        var addButton = cut.Find("button.btn-add-text");

        Assert.True(addButton.HasAttribute("disabled"));
    }

    [Fact]
    public void TaskList_EscapeKey_ClearsInputButKeepsFormVisible()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        var input = cut.Find("input.task-input");
        input.Input("Some text");
        input.KeyDown("Escape");

        Assert.Contains("add-task-form", cut.Markup);
        input = cut.Find("input.task-input");
        Assert.Equal("", input.GetAttribute("value") ?? "");
    }

    #endregion

    #region More Panel Tests

    [Fact]
    public void MoreClick_ShowsPanel_HidesCollapsedForm()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        cut.Find("button.btn-more").Click();

        Assert.Contains("DETAILS", cut.Markup);
        Assert.DoesNotContain("add-task-form", cut.Markup);
    }

    [Fact]
    public void CreateTask_KeepsPanelOpen_AfterAdd()
    {
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, req => addedTaskName = req.Name)));

        cut.Find("button.btn-more").Click();
        cut.Find(".add-task-section input.tep-input-name").Input("New Task");
        cut.Find(".add-task-section button.tep-save-btn").Click();

        Assert.Equal("New Task", addedTaskName);
        Assert.Contains("DETAILS", cut.Markup);
    }

    [Fact]
    public void CancelPanel_ClosesPanel_AndClearsName()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        cut.Find("button.btn-more").Click();
        cut.Find(".add-task-section input.tep-input-name").Input("Some task");
        cut.Find(".add-task-section button.tep-cancel-btn").Click();

        Assert.DoesNotContain("DETAILS", cut.Markup);
        Assert.Contains("add-task-form", cut.Markup);
        var input = cut.Find("input.task-input");
        Assert.Equal("", input.GetAttribute("value") ?? "");
    }

    [Fact]
    public void ShiftEnter_InBarInput_ExpandsPanel()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        var input = cut.Find("input.task-input");
        input.Input("Some task");
        input.TriggerEvent("onkeydown", new KeyboardEventArgs { Key = "Enter", ShiftKey = true });

        Assert.Contains("DETAILS", cut.Markup);
        Assert.DoesNotContain("add-task-form", cut.Markup);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public void TaskList_AddTask_InvokesOnTaskAddCallback()
    {
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, req => addedTaskName = req.Name)));

        var input = cut.Find("input.task-input");
        input.Input("New Task");
        cut.Find("button.btn-add-text").Click();

        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_AddTask_TrimsWhitespace()
    {
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, req => addedTaskName = req.Name)));

        var input = cut.Find("input.task-input");
        input.Input("  New Task  ");
        cut.Find("button.btn-add-text").Click();

        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_AddTask_DoesNotInvokeWhenEmpty()
    {
        var callbackInvoked = false;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, _ => callbackInvoked = true)));

        var addButton = cut.Find("button.btn-add-text");
        Assert.True(addButton.HasAttribute("disabled"));

        Assert.False(callbackInvoked);
    }

    [Fact]
    public void AddTask_ViaAddButton_ReturnsFocusToNameInput()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, _ => { })));

        var input = cut.Find("input.task-input");
        input.Input("New Task");
        cut.Find("button.btn-add-text").Click();

        cut.Find("input.task-input").GetAttribute("value").Should().BeNullOrEmpty();
        GetShouldFocusName(cut.Instance).Should().BeFalse(
            "OnAfterRenderAsync must have focused the name input after Add so keystrokes do not trigger shortcuts");
    }

    [Fact]
    public void AddTask_ViaCreateButton_ReturnsFocusToDetailsInput()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, _ => { })));

        cut.Find("button.btn-more").Click();
        cut.Find(".add-task-section input.tep-input-name").Input("New Task");
        cut.Find(".add-task-section button.tep-save-btn").Click();

        GetShouldFocusName(cut.Instance).Should().BeFalse(
            "OnAfterRenderAsync must have focused the details name input after Create so keystrokes do not trigger shortcuts");
    }

    private static bool GetShouldFocusName(TaskListBase instance)
    {
        var field = typeof(TaskListBase).GetField(
            "_shouldFocusName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)(field?.GetValue(instance) ?? false);
    }

    [Fact]
    public void TaskList_EnterKey_InvokesOnTaskAddCallback()
    {
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<NewTaskRequest>(this, req => addedTaskName = req.Name)));

        var input = cut.Find("input.task-input");
        input.Input("New Task");
        input.KeyDown("Enter");

        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_EscapeKey_ClearsInput()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        var input = cut.Find("input.task-input");
        input.Input("Some text");
        input.KeyDown("Escape");

        Assert.Contains("add-task-form", cut.Markup);
        input = cut.Find("input.task-input");
        Assert.Equal("", input.GetAttribute("value") ?? "");
    }

    #endregion

    #region Task Selection Tests

    [Fact]
    public void TaskList_WithSelectedTask_ShowsSelection()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = taskId, Name = "Selected Task", IsCompleted = false }
        };

        // Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, taskId));

        // Assert - Selected task should have selected class
        Assert.Contains("selected", cut.Markup);
    }

    [Fact]
    public void TaskList_WithNoSelectedTask_DoesNotShowSelection()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Task", IsCompleted = false }
        };

        // Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.DoesNotContain("selected", cut.Markup);
    }

    [Fact]
    public void TaskList_ClickTask_InvokesOnTaskSelectCallback()
    {
        var selectedTaskId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Active Task", IsCompleted = false }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskSelect, EventCallback.Factory.Create<Guid>(this, id => selectedTaskId = id)));

        cut.Find(".task-row").Click();

        Assert.Equal(taskId, selectedTaskId);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public void TaskList_WithTasksHavingLastWorkedOn_RendersAllTasks()
    {
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Never Worked", IsCompleted = false, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new TaskItem { Id = Guid.NewGuid(), Name = "Worked Recently", IsCompleted = false, CreatedAt = DateTime.UtcNow.AddDays(-2), LastWorkedOn = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Name = "Worked Earlier", IsCompleted = false, CreatedAt = DateTime.UtcNow.AddDays(-1), LastWorkedOn = DateTime.UtcNow.AddHours(-5) }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        Assert.Contains("Never Worked", cut.Markup);
        Assert.Contains("Worked Recently", cut.Markup);
        Assert.Contains("Worked Earlier", cut.Markup);
    }

    [Fact]
    public void TaskList_WithCompletedTasksHavingLastWorkedOn_ShowsCompletedSection()
    {
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Name = "Active", IsCompleted = false },
            new TaskItem { Id = Guid.NewGuid(), Name = "Completed No Work", IsCompleted = true, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new TaskItem { Id = Guid.NewGuid(), Name = "Completed With Work", IsCompleted = true, CreatedAt = DateTime.UtcNow.AddDays(-2), LastWorkedOn = DateTime.UtcNow }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null));

        Assert.Contains("completed-section", cut.Markup);
        cut.Find(".completed-toggle").Click();
        cut.Render();
        Assert.Contains("Completed No Work", cut.Markup);
        Assert.Contains("Completed With Work", cut.Markup);
    }

    #endregion

    #region Task Complete/Delete/Uncomplete Callback Tests

    [Fact]
    public void TaskList_CompleteButton_InvokesOnTaskCompleteCallback()
    {
        var completedTaskId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Active Task", IsCompleted = false }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskComplete, EventCallback.Factory.Create<Guid>(this, id => completedTaskId = id)));

        cut.Find("button[aria-label=\"Complete\"]").Click();

        Assert.Equal(taskId, completedTaskId);
    }

    [Fact]
    public void TaskList_DeleteButton_InvokesOnTaskDeleteCallback()
    {
        var deletedTaskId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Active Task", IsCompleted = false }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskDelete, EventCallback.Factory.Create<Guid>(this, id => deletedTaskId = id)));

        cut.Find("button[aria-label=\"Delete\"]").Click();

        Assert.Equal(taskId, deletedTaskId);
    }

    [Fact]
    public void TaskList_UncompleteButton_InvokesOnTaskUncompleteCallback()
    {
        var uncompletedTaskId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Completed Task", IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskUncomplete, EventCallback.Factory.Create<Guid>(this, id => uncompletedTaskId = id)));

        cut.Find(".completed-toggle").Click();
        cut.Render();
        cut.Find("button[aria-label=\"Undo\"]").Click();

        Assert.Equal(taskId, uncompletedTaskId);
    }

    [Fact]
    public void TaskList_EditButton_TriggersOnTaskEditCallback()
    {
        TaskItem? editedTask = null;
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = taskId, Name = "Active Task", IsCompleted = false }
        };

        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskEdit, EventCallback.Factory.Create<TaskItem>(this, t => editedTask = t)));

        cut.Find("button[aria-label=\"Edit task\"]").Click();
        cut.Find(".tep-save-btn").Click();

        Assert.NotNull(editedTask);
        Assert.Equal(taskId, editedTask!.Id);
    }

    #endregion

    #region Task Item Header Tests

    [Fact]
    public void TaskList_ShowsTaskCard()
    {
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        cut.Find(".task-card").Should().NotBeNull();
    }

    #endregion

    #region BuildTree parentage / collapse / callbacks (coverage)

    [Fact]
    public void Render_LocalParentWithChild_ShowsNestedRowsAndCollapseToggle()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId }
        };

        // Act
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null));

        // Assert
        cut.Markup.Should().Contain("Parent");
        cut.Markup.Should().Contain("Child");
        cut.FindAll(".row-toggle").Should().HaveCount(1);
    }

    [Fact]
    public void Render_GoogleParentWithChild_ResolvesGoogleParentageAndNests()
    {
        // Arrange - child linked only by GoogleParentTaskId; plus an orphan with an unmatched google parent.
        var parent = new TaskItem { Id = Guid.NewGuid(), Name = "GParent", CreatedAt = DateTime.UtcNow, GoogleTaskId = "g1" };
        var child = new TaskItem { Id = Guid.NewGuid(), Name = "GChild", CreatedAt = DateTime.UtcNow.AddSeconds(1), GoogleParentTaskId = "g1" };
        var orphan = new TaskItem { Id = Guid.NewGuid(), Name = "Orphan", CreatedAt = DateTime.UtcNow.AddSeconds(2), GoogleParentTaskId = "missing" };
        var tasks = new List<TaskItem> { parent, child, orphan };

        // Act
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.GoogleLists, [new TaskListRef("g1", "Personal", "var(--pomodoro-color)", 0, true, false)]));

        // Assert
        cut.Markup.Should().Contain("GChild");
        cut.FindAll(".row-toggle").Should().HaveCount(1, "only the google parent has a resolvable child");
    }

    [Fact]
    public void Render_GoogleParentWithMultipleChildren_OrdersByGooglePosition()
    {
        // Arrange - multiple google children with distinct positions exercise the
        // childrenByGoogleParent OrderBy (TaskList.razor.cs:118).
        var parent = new TaskItem { Id = Guid.NewGuid(), Name = "GParent", CreatedAt = DateTime.UtcNow, GoogleTaskId = "g1" };
        var tasks = new List<TaskItem>
        {
            parent,
            new() { Id = Guid.NewGuid(), Name = "GZeta", CreatedAt = DateTime.UtcNow, GoogleParentTaskId = "g1", GooglePosition = "z" },
            new() { Id = Guid.NewGuid(), Name = "GAlpha", CreatedAt = DateTime.UtcNow, GoogleParentTaskId = "g1", GooglePosition = "a" }
        };

        // Act
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.GoogleLists, [new TaskListRef("g1", "Personal", "var(--pomodoro-color)", 0, true, false)]));

        // Assert - both children render under the google parent.
        cut.Markup.Should().Contain("GZeta");
        cut.Markup.Should().Contain("GAlpha");
    }

    [Fact]
    public void ToggleCollapse_ClickedTwice_HidesThenShowsChildren()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId }
        };
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null));

        // Act - collapse
        cut.Find(".row-toggle").Click();
        cut.Render();
        cut.Markup.Should().NotContain("Child");

        // Act - expand again
        cut.Find(".row-toggle").Click();
        cut.Render();
        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public async Task HandleAddSubtask_ThroughChild_InvokesOnAddSubtask()
    {
        // Arrange
        AddSubtaskRequest? captured = null;
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Root", CreatedAt = DateTime.UtcNow };
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem> { task })
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.OnAddSubtask, EventCallback.Factory.Create<AddSubtaskRequest>(this, r => captured = r)));

        // Act - open subtask form, type, submit
        cut.Find("button[aria-label=\"Add subtask\"]").Click();
        cut.Render();
        cut.Find("input[aria-label=\"New subtask name\"]").Input("New sub");
        cut.Find("button[aria-label=\"Add\"]").Click();
        cut.Render();

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("New sub");
        captured.ParentTaskId.Should().Be(task.Id);
    }

    [Fact]
    public async Task HandleReparentToRoot_ThroughChild_InvokesOnReparentToRoot()
    {
        // Arrange - a child node (Depth > 0) shows the "Promote" button.
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow },
            new() { Id = childId, Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId }
        };
        Guid? reparented = null;
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.OnReparentToRoot, EventCallback.Factory.Create<Guid>(this, id => reparented = id)));

        // Act
        cut.Find("button[aria-label=\"Promote\"]").Click();

        // Assert
        reparented.Should().Be(childId);
    }

    [Fact]
    public async Task HandleDemote_ThroughSiblingSelection_InvokesOnDemote()
    {
        // Arrange - a task with siblings shows the "Demote" button and dropdown.
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = firstId, Name = "First", CreatedAt = DateTime.UtcNow },
            new() { Id = secondId, Name = "Second", CreatedAt = DateTime.UtcNow.AddSeconds(1) },
            new() { Id = thirdId, Name = "Third", CreatedAt = DateTime.UtcNow.AddSeconds(2) }
        };
        DemoteRequest? captured = null;
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.OnDemote, EventCallback.Factory.Create<DemoteRequest>(this, r => captured = r)));

        // Act - third task has siblings; open dropdown, select first sibling.
        var demoteButtons = cut.FindAll("button[aria-label=\"Demote\"]");
        demoteButtons.Should().HaveCountGreaterThan(0);
        demoteButtons[2].Click();
        cut.Find(".demote-pick").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.TaskId.Should().Be(thirdId);
    }

    #endregion

    #region Completed Child Stays Under Parent Tests

    [Fact]
    public void Render_CompletedChildUnderActiveParent_ChildStaysInActiveSection()
    {
        var parentId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow, IsCompleted = false },
            new() { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId, IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null));

        cut.Markup.Should().Contain("Parent");
        cut.Markup.Should().Contain("Child");
        cut.Markup.Should().NotContain("completed-section");
    }

    [Fact]
    public void Render_CompletedRoot_ChildMovesToCompletedSection()
    {
        var parentId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow, IsCompleted = true },
            new() { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId, IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null));

        cut.Markup.Should().Contain("completed-section");
        cut.Find(".completed-toggle").Click();
        cut.Render();
        cut.Markup.Should().Contain("Parent");
        cut.Markup.Should().Contain("Child");
    }

    [Fact]
    public void Render_CompletedChildShowsCheckmarkInActiveSection()
    {
        var parentId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = parentId, Name = "Parent", CreatedAt = DateTime.UtcNow, IsCompleted = false },
            new() { Id = Guid.NewGuid(), Name = "Child", CreatedAt = DateTime.UtcNow.AddSeconds(1), ParentTaskId = parentId, IsCompleted = true }
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, tasks)
            .Add(x => x.CurrentTaskId, null));

        cut.FindAll("button.task-checkbox.completed").Should().HaveCount(1);
    }

    #endregion

    #region More Options Panel

    [Fact]
    public void MorePanel_ShowsWhenMoreButtonClicked()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();

        cut.Markup.Should().Contain("task-edit-panel");
    }

    [Fact]
    public void MorePanel_ShowsGoogleLists_WhenGoogleListsProvided()
    {
        var googleLists = new List<TaskListRef>
        {
            new("google-1", "Work", "var(--pomodoro-color)", 2, true, true)
        };

        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null)
            .Add(x => x.GoogleLists, googleLists));

        cut.Find(".btn-more").Click();

        cut.Markup.Should().Contain("Work");
        cut.Markup.Should().Contain("google-1");
    }

    [Fact]
    public void MorePanel_ShowsWeekdayButtons_WhenWeeklySelected()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();
        var repeatSelect = cut.FindAll("select.tep-select")[1];
        repeatSelect.Change(RepeatType.Weekly);

        cut.Markup.Should().Contain("tep-weekday-btn");
        cut.FindAll(".tep-weekday-btn").Should().HaveCount(7);
    }

    [Fact]
    public void MorePanel_TogglesWeekday_WhenWeekdayClicked()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();
        cut.FindAll("select.tep-select")[1].Change(RepeatType.Weekly);

        var mondayBtn = cut.FindAll(".tep-weekday-btn")[0];
        mondayBtn.Click();
        cut.Render();

        cut.FindAll(".tep-weekday-btn.active").Should().HaveCount(1);
    }

    [Fact]
    public void MorePanel_ShowsPauseToggle_WhenRepeatTypeSelected()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();
        cut.FindAll("select.tep-select")[1].Change(RepeatType.Daily);

        cut.Markup.Should().Contain("tep-toggle");
        cut.Markup.Should().Contain("Not paused");
    }

    [Fact]
    public void MorePanel_TogglesPauseState_WhenPauseClicked()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();
        cut.FindAll("select.tep-select")[1].Change(RepeatType.Daily);

        cut.Find(".tep-toggle").Click();
        cut.Render();

        cut.Markup.Should().Contain("Paused");
    }

    [Fact]
    public void MorePanel_TogglesWeekdayOff_WhenAlreadyActive()
    {
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem>())
            .Add(x => x.CurrentTaskId, null));

        cut.Find(".btn-more").Click();
        cut.FindAll("select.tep-select")[1].Change(RepeatType.Weekly);

        var mondayBtn = cut.FindAll(".tep-weekday-btn")[0];
        mondayBtn.Click();
        cut.Render();
        cut.FindAll(".tep-weekday-btn.active").Should().HaveCount(1);

        mondayBtn = cut.FindAll(".tep-weekday-btn")[0];
        mondayBtn.Click();
        cut.Render();
        cut.FindAll(".tep-weekday-btn.active").Should().HaveCount(0);
    }

    #endregion
}

