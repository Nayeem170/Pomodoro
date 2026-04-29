using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Pomodoro.Web.Pages;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class AboutPageCoverageTests : TestContext
{
    [Fact]
    public void ToggleWhatIs_ExpandsSection()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[0].Click();

        cut.Markup.Should().Contain("collapse-body");
        cut.Markup.Should().Contain("What is Pomodoro Technique?");
    }

    [Fact]
    public void ToggleWhatIs_CollapsesSection()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[0].Click();
        cut.Markup.Should().Contain("collapse-body");

        cut.FindAll(".collapse-toggle")[0].Click();
        cut.Markup.Should().NotContain("collapse-body");
    }

    [Fact]
    public void ToggleDefaultTimes_ExpandsSection()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[4].Click();

        cut.Markup.Should().Contain("collapse-body");
        cut.Markup.Should().Contain("times-grid");
        cut.Markup.Should().Contain("time-card");
    }

    [Fact]
    public void ToggleDefaultTimes_ShowsDefaultValues()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[4].Click();

        cut.Markup.Should().Contain("25");
        cut.Markup.Should().Contain("5");
        cut.Markup.Should().Contain("15");
    }

    [Fact]
    public void ToggleDefaultTimes_CollapsesSection()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[4].Click();
        cut.Markup.Should().Contain("collapse-body");

        cut.FindAll(".collapse-toggle")[4].Click();
        cut.Markup.Should().NotContain("collapse-body");
    }

    [Fact]
    public void ToggleBenefits_ExpandsAndCollapses()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[2].Click();
        cut.Markup.Should().Contain("benefits-grid");

        cut.FindAll(".collapse-toggle")[2].Click();
        cut.Markup.Should().NotContain("benefits-grid");
    }

    [Fact]
    public void ToggleTips_ExpandsAndCollapses()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[3].Click();
        cut.Markup.Should().Contain("tips-list");

        cut.FindAll(".collapse-toggle")[3].Click();
        cut.Markup.Should().NotContain("tips-list");
    }

    [Fact]
    public void CollapseArrow_OpenClass()
    {
        var cut = RenderComponent<About>();

        cut.FindAll(".collapse-toggle")[0].Click();

        cut.Markup.Should().Contain("collapse-arrow open");
    }

    [Fact]
    public void CollapseArrow_ClosedClass()
    {
        var cut = RenderComponent<About>();

        var arrow = cut.Find(".collapse-arrow");
        arrow.ClassList.Should().NotContain("open");
    }

    [Fact]
    public void HeroSection_DisplaysVersion()
    {
        var cut = RenderComponent<About>();

        var match = Regex.Match(cut.Markup, @"v\d+\.\d+");
        match.Success.Should().BeTrue();
    }
}
