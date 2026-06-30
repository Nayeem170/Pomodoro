using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class TaskListTabsTests : TestContext
{
    public TaskListTabsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TaskListTabs_RendersLocalAndScheduleTabs()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 3, true, true),
            new(Constants.TaskLists.ScheduleListId, "Schedule", "#eab308", 1, true, true)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId));

        Assert.Contains("Tasks", cut.Markup);
        Assert.Contains("Schedule", cut.Markup);
        Assert.Contains("3", cut.Markup);
        Assert.Contains("1", cut.Markup);
    }

    [Fact]
    public void TaskListTabs_RendersGoogleListTabs()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "My Google List", "#4285F4", 2, true, false)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId));

        Assert.Contains("My Google List", cut.Markup);
        Assert.Contains("#4285F4", cut.Markup);
        Assert.Contains("2", cut.Markup);
    }

    [Fact]
    public void TaskListTabs_HidesInvisibleLists()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Hidden List", "#4285F4", 0, false, false)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId));

        Assert.DoesNotContain("Hidden List", cut.Markup);
        Assert.Contains("Tasks", cut.Markup);
    }

    [Fact]
    public void TaskListTabs_ActiveTab_Highlighted()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, "glist-1"));

        Assert.Contains("act", cut.Markup);
    }

    [Fact]
    public void TaskListTabs_ClickTab_InvokesCallback()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        var tabChanged = false;
        string? changedId = null;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, id =>
            {
                tabChanged = true;
                changedId = id;
            })));

        cut.FindAll("button.lt").Last().Click();

        Assert.True(tabChanged);
        Assert.Equal("glist-1", changedId);
    }

    [Fact]
    public void TaskListTabs_ClickSameTab_DoesNotInvokeCallback()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true)
        };
        var tabChanged = false;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, _ => tabChanged = true)));

        cut.Find("button.lt").Click();

        Assert.False(tabChanged);
    }

    [Fact]
    public void TaskListTabs_HasTablistRole()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId));

        Assert.Equal("tablist", cut.Find(".ltabs").GetAttribute("role"));
    }

    [Fact]
    public void TaskListTabs_ZeroCount_HidesBadge()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId));

        Assert.DoesNotContain("lt-cnt", cut.Markup);
    }

    [Fact]
    public async Task TaskListTabs_ArrowRight_NavigatesToNextTab()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        string? changedId = null;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, id => changedId = id)));

        await cut.Find("button.lt").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal("glist-1", changedId);
    }

    [Fact]
    public async Task TaskListTabs_ArrowLeft_NavigatesToPreviousTab()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        string? changedId = null;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, "glist-1")
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, id => changedId = id)));

        await cut.FindAll("button.lt").Last().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal(Constants.TaskLists.LocalPomodoroListId, changedId);
    }

    [Fact]
    public async Task TaskListTabs_ArrowRight_WrapsAround()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        string? changedId = null;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, "glist-1")
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, id => changedId = id)));

        await cut.FindAll("button.lt").Last().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal(Constants.TaskLists.LocalPomodoroListId, changedId);
    }

    [Fact]
    public async Task TaskListTabs_NonArrowKey_DoesNotNavigate()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        var tabChanged = false;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, _ => tabChanged = true)));

        await cut.Find("button.lt").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        Assert.False(tabChanged);
    }

    [Fact]
    public async Task TaskListTabs_SingleVisibleList_ArrowKeyDoesNotNavigate()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true)
        };
        var tabChanged = false;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, _ => tabChanged = true)));

        await cut.Find("button.lt").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.False(tabChanged);
    }

    [Fact]
    public void TaskListTabs_ServiceCurrentListId_PrefersServiceValueOverStalePageId()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.ServiceCurrentListId, "glist-1"));

        var active = cut.FindAll("button.lt.act");
        Assert.Single(active);
        Assert.Contains("Google", active[0].TextContent);
    }

    [Fact]
    public void TaskListTabs_DivergedIds_ClickingNonServiceList_NotSuppressed()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Google", "#4285F4", 0, true, false)
        };
        string? changedId = null;

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, "glist-1")
            .Add(p => p.ServiceCurrentListId, Constants.TaskLists.LocalPomodoroListId)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<string>(this, id => changedId = id)));

        var active = cut.FindAll("button.lt.act");
        Assert.Single(active);
        Assert.Contains("Tasks", active[0].TextContent);

        cut.FindAll("button.lt").Last().Click();

        Assert.Equal("glist-1", changedId);
    }

    [Fact]
    public void TaskListTabs_HiddenCurrentList_FallsBackToFirstVisible()
    {
        var lists = new List<TaskListRef>
        {
            new(Constants.TaskLists.LocalPomodoroListId, "Tasks", "var(--pomodoro-color)", 0, true, true),
            new("glist-1", "Hidden", "#4285F4", 0, false, false)
        };

        var cut = RenderComponent<TaskListTabs>(parameters => parameters
            .Add(p => p.Lists, lists)
            .Add(p => p.CurrentListId, "glist-1"));

        Assert.Contains("act", cut.Markup);
        Assert.DoesNotContain("Hidden", cut.Markup);
    }
}

[Trait("Category", "Component")]
public class TaskListSyncStripTests : TestContext
{
    public TaskListSyncStripTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TaskListSyncStrip_GoogleList_ShowsContent()
    {
        var cut = RenderComponent<TaskListSyncStrip>(parameters => parameters
            .Add(p => p.IsGoogleList, true)
            .Add(p => p.ListName, "My Google List"));

        Assert.Contains("Google Tasks", cut.Markup);
        Assert.Contains("My Google List", cut.Markup);
        Assert.Contains("synced", cut.Markup);
    }

    [Fact]
    public void TaskListSyncStrip_LocalList_ShowsNothing()
    {
        var cut = RenderComponent<TaskListSyncStrip>(parameters => parameters
            .Add(p => p.IsGoogleList, false)
            .Add(p => p.ListName, "Tasks"));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void TaskListSyncStrip_NullListName_ShowsStripWithoutName()
    {
        var cut = RenderComponent<TaskListSyncStrip>(parameters => parameters
            .Add(p => p.IsGoogleList, true)
            .Add(p => p.ListName, null));

        Assert.Contains("Google Tasks", cut.Markup);
        Assert.Contains("synced", cut.Markup);
    }
}
