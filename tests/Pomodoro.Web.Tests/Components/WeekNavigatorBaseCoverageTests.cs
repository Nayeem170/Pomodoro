using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Xunit;
using System.Reflection;

namespace Pomodoro.Web.Tests.Components;

[Trait("Category", "Component")]
public class WeekNavigatorBaseCoverageTests : TestContext
{
    [Fact]
    public void GetWeekStart_CalculatesCorrectly()
    {
        var result = WeekNavigatorBase.GetWeekStart(new DateTime(2026, 4, 22, 15, 30, 0));
        result.DayOfWeek.Should().Be(DayOfWeek.Saturday);
    }

    [Fact]
    public void Render_ShowsFormattedWeekRange()
    {
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, new DateTime(2026, 4, 18)));

        cut.Markup.Should().Contain("period-lbl");
        cut.Markup.Should().Contain("nav-arr");
    }

    [Fact]
    public void NextWeek_Disabled_WhenIsThisWeek()
    {
        var thisWeekStart = WeekNavigatorBase.GetWeekStart(DateTime.Now.Date);
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, thisWeekStart));

        var nextButton = cut.FindAll("button.nav-arr")[1];
        nextButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void NextWeek_Enabled_WhenNotThisWeek()
    {
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, new DateTime(2026, 1, 1)));

        var nextButton = cut.FindAll("button.nav-arr")[1];
        nextButton.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void PreviousWeek_FiresCallback()
    {
        var weekChanged = false;
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, new DateTime(2026, 4, 18))
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, d => weekChanged = true)));

        cut.Find("button.nav-arr").Click();

        weekChanged.Should().BeTrue();
    }

    [Fact]
    public void NextWeek_FiresCallback()
    {
        var weekChanged = false;
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, new DateTime(2026, 1, 1))
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, d => weekChanged = true)));

        cut.FindAll("button.nav-arr")[1].Click();

        weekChanged.Should().BeTrue();
    }

    [Fact]
    public async Task GoToThisWeek_InvokesCallbackWithCurrentWeekStart()
    {
        var expectedWeekStart = WeekNavigatorBase.GetWeekStart(DateTime.Now.Date);
        var receivedWeekStart = new DateTime?();
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, new DateTime(2026, 1, 1))
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, d => receivedWeekStart = d)));

        var method = typeof(WeekNavigatorBase).GetMethod("GoToThisWeek", BindingFlags.Instance | BindingFlags.NonPublic);
        await (Task)method!.Invoke(cut.Instance, null)!;

        receivedWeekStart.Should().Be(expectedWeekStart);
    }
}
