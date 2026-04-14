using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests;

public class TimelineSectionTests : TestContext
{
    public TimelineSectionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ActivityTimelineFormatter>();
        Services.AddSingleton<ActivityItemFormatter>();
    }

    [Fact]
    public void TimelineSection_WithNullActivities_ShowsZeroCount()
    {
        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, (List<ActivityRecord>?)null)
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("0 activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithEmptyActivities_ShowsZeroCount()
    {
        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, new List<ActivityRecord>())
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("0 activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithActivities_ShowsCorrectCount()
    {
        var activities = new List<ActivityRecord>
        {
            new() { TaskName = "Test 1" },
            new() { TaskName = "Test 2" },
            new() { TaskName = "Test 3" }
        };

        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, activities)
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("3 activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithNoMoreActivitiesAndEmptyList_DoesNotShowNoMoreActivitiesMessage()
    {
        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, new List<ActivityRecord>())
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.DoesNotContain("No more activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithNoMoreActivitiesAndHasActivities_ShowsNoMoreActivitiesMessage()
    {
        var activities = new List<ActivityRecord>
        {
            new() { TaskName = "Test 1" }
        };

        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, activities)
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("1 activities", cut.Markup);
        Assert.Contains("No more activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithMoreActivities_ShowsLoadingIndicator()
    {
        var activities = new List<ActivityRecord>
        {
            new() { TaskName = "Test 1" }
        };

        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, activities)
            .Add(p => p.HasMoreActivities, true)
            .Add(p => p.IsLoadingMore, true));

        Assert.Contains("1 activities", cut.Markup);
        Assert.Contains("Loading more activities...", cut.Markup);
    }

    [Fact]
    public void TimelineSection_WithMoreActivitiesNotLoading_DoesNotShowLoadingIndicator()
    {
        var activities = new List<ActivityRecord>
        {
            new() { TaskName = "Test 1" }
        };

        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, activities)
            .Add(p => p.HasMoreActivities, true)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("1 activities", cut.Markup);
        Assert.DoesNotContain("Loading more activities...", cut.Markup);
        Assert.DoesNotContain("No more activities", cut.Markup);
    }

    [Fact]
    public void TimelineSection_RendersTimelineTitle()
    {
        var cut = RenderComponent<TimelineSection>(parameters => parameters
            .Add(p => p.Activities, new List<ActivityRecord>())
            .Add(p => p.HasMoreActivities, false)
            .Add(p => p.IsLoadingMore, false));

        Assert.Contains("Timeline", cut.Markup);
    }
}
