using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;
using System.Reflection;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TaskItemBaseCoverageTests : TestContext
{
    [Fact]
    public void GetStatusIcon_CompletedTask()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = true }));

        cut.Find(".task-checkbox").ClassList.Should().Contain("completed");
        cut.Find(".task-text").ClassList.Should().Contain("completed");
    }

    [Fact]
    public void GetStatusIcon_TaskWithPomodoros()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", PomodoroCount = 3, TotalFocusMinutes = 75 }));

        cut.Find(".task-pomo-count").TextContent.Should().Contain("1h 15m");
    }

    [Fact]
    public void GetStatusIcon_DefaultTask()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test" }));

        cut.Find(".task-checkbox").ClassList.Should().NotContain("completed");
        cut.Find(".task-pomo-count").TextContent.Should().Be("0m");
    }

    [Fact]
    public void HandleSelect_Fires_WhenNotCompleted()
    {
        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = false })
            .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selected = true)));

        cut.Find(".task-row").Click();

        selected.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyDown_EnterKey_SelectsTask()
    {
        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = false })
            .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selected = true)));

        cut.Find(".task-row").TriggerEvent("onkeydown", new KeyboardEventArgs { Key = "Enter" });

        selected.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyDown_SpaceKey_SelectsTask()
    {
        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = false })
            .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selected = true)));

        cut.Find(".task-row").TriggerEvent("onkeydown", new KeyboardEventArgs { Key = " " });

        selected.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyDown_OtherKey_DoesNotSelect()
    {
        var selected = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = false })
            .Add(p => p.OnSelect, EventCallback.Factory.Create<Guid>(this, id => selected = true)));

        cut.Find(".task-row").TriggerEvent("onkeydown", new KeyboardEventArgs { Key = "Tab" });

        selected.Should().BeFalse();
    }

    [Fact]
    public void GetTaskClass_SelectedAndCompleted()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = true })
            .Add(p => p.IsSelected, true));

        cut.Find(".task-row").ClassList.Should().Contain("selected");
        cut.Find(".task-row").ClassList.Should().Contain("completed");
    }

    [Fact]
    public void GetTaskClass_NotSelectedNotCompleted()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = false })
            .Add(p => p.IsSelected, false));

        cut.Find(".task-row").ClassList.Should().NotContain("selected");
        cut.Find(".task-row").ClassList.Should().NotContain("completed");
    }

    [Fact]
    public void HandleComplete_FiresCallback()
    {
        var completed = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test" })
            .Add(p => p.OnComplete, EventCallback.Factory.Create<Guid>(this, id => completed = true)));

        cut.Find(".task-checkbox").Click();

        completed.Should().BeTrue();
    }

    [Fact]
    public void HandleDelete_FiresCallback()
    {
        var deleted = false;
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test" })
            .Add(p => p.OnDelete, EventCallback.Factory.Create<Guid>(this, id => deleted = true)));

        cut.Find(".task-action-btn[aria-label=\"Delete\"]").Click();

        deleted.Should().BeTrue();
    }

    [Fact]
    public void GetStatusIcon_ViaReflection_CompletedTask()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", IsCompleted = true }));

        var method = typeof(TaskItemBase).GetMethod("GetStatusIcon", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = (string)method!.Invoke(cut.Instance, null)!;

        result.Should().Be(Constants.Tasks.CompletedEmoji);
    }

    [Fact]
    public void GetStatusIcon_ViaReflection_TaskWithPomodoros()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test", PomodoroCount = 3 }));

        var method = typeof(TaskItemBase).GetMethod("GetStatusIcon", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = (string)method!.Invoke(cut.Instance, null)!;

        result.Should().Be(Constants.Tasks.HasPomodorosEmoji);
    }

    [Fact]
    public void GetStatusIcon_ViaReflection_DefaultTask()
    {
        var cut = RenderComponent<TaskItemComponent>(parameters => parameters
            .Add(p => p.Item, new TaskItem { Name = "Test" }));

        var method = typeof(TaskItemBase).GetMethod("GetStatusIcon", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = (string)method!.Invoke(cut.Instance, null)!;

        result.Should().Be(Constants.Tasks.DefaultEmoji);
    }
}
