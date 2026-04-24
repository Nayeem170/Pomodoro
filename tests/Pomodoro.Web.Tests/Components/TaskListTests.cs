using Bunit;
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
    public void TaskList_Initially_ShowsAddTaskButton()
    {
        // Arrange & Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.Contains("task-add-btn", cut.Markup);
        Assert.Contains("+ Add", cut.Markup);
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
    public void TaskList_ClickAddTask_ShowsAddTaskForm()
    {
        // Arrange
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        // Act
        cut.Find("button.task-add-btn").Click();

        // Assert
        Assert.Contains("add-task-form", cut.Markup);
        Assert.Contains("task-input", cut.Markup);
        Assert.Contains("btn-add", cut.Markup);
        Assert.Contains("btn-cancel", cut.Markup);
    }

    [Fact]
    public void TaskList_AddForm_InitiallyHasEmptyInput()
    {
        // Arrange
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        // Act
        cut.Find("button.task-add-btn").Click();

        // Assert
        var input = cut.Find("input.task-input");
        Assert.Equal("", input.GetAttribute("value") ?? "");
    }

    [Fact]
    public void TaskList_AddForm_AddButtonDisabledWhenEmpty()
    {
        // Arrange
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));
        cut.Find("button.task-add-btn").Click();

        // Act
        var addButton = cut.Find("button.btn-add");

        // Assert
        Assert.True(addButton.HasAttribute("disabled"));
    }

    [Fact]
    public void TaskList_ClickCancel_HidesAddTaskForm()
    {
        // Arrange
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));
        cut.Find("button.task-add-btn").Click();

        // Act
        cut.Find("button.btn-cancel").Click();

        // Assert
        Assert.DoesNotContain("add-task-form", cut.Markup);
        Assert.Contains("task-add-btn", cut.Markup);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public void TaskList_AddTask_InvokesOnTaskAddCallback()
    {
        // Arrange
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<string>(this, name => addedTaskName = name)));

        // Act - Show form and enter task name
        cut.Find("button.task-add-btn").Click();
        var input = cut.Find("input.task-input");
        input.Input("New Task");
        cut.Find("button.btn-add").Click();

        // Assert
        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_AddTask_TrimsWhitespace()
    {
        // Arrange
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<string>(this, name => addedTaskName = name)));

        // Act
        cut.Find("button.task-add-btn").Click();
        var input = cut.Find("input.task-input");
        input.Input("  New Task  ");
        cut.Find("button.btn-add").Click();

        // Assert
        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_AddTask_DoesNotInvokeWhenEmpty()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<string>(this, _ => callbackInvoked = true)));

        // Act
        cut.Find("button.task-add-btn").Click();
        // Input is empty, add button is disabled
        var addButton = cut.Find("button.btn-add");
        // Can't click disabled button, but let's verify it's disabled
        Assert.True(addButton.HasAttribute("disabled"));

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TaskList_EnterKey_InvokesOnTaskAddCallback()
    {
        // Arrange
        var addedTaskName = string.Empty;
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null)
            .Add(p => p.OnTaskAdd, EventCallback.Factory.Create<string>(this, name => addedTaskName = name)));

        // Act
        cut.Find("button.task-add-btn").Click();
        var input = cut.Find("input.task-input");
        input.Input("New Task");
        input.KeyDown("Enter");

        // Assert
        Assert.Equal("New Task", addedTaskName);
    }

    [Fact]
    public void TaskList_EscapeKey_CancelsAddTask()
    {
        // Arrange
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));
        cut.Find("button.task-add-btn").Click();

        // Act
        var input = cut.Find("input.task-input");
        input.KeyDown("Escape");

        // Assert
        Assert.DoesNotContain("add-task-form", cut.Markup);
        Assert.Contains("task-add-btn", cut.Markup);
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

        cut.Find("button[aria-label=\"Undo\"]").Click();

        Assert.Equal(taskId, uncompletedTaskId);
    }

    #endregion

    #region Task Item Header Tests

    [Fact]
    public void TaskList_ShowsTasksHeader()
    {
        // Arrange & Act
        var cut = RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CurrentTaskId, null));

        // Assert
        Assert.Contains("Tasks", cut.Markup);
    }

    #endregion
}

