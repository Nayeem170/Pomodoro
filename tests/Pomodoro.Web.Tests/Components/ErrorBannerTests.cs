using Bunit;
using Pomodoro.Web.Components.Shared;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class ErrorBannerTests : TestContext
{
    public ErrorBannerTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ErrorBanner_WithMessage_RendersBanner()
    {
        var cut = RenderComponent<ErrorBanner>(parameters => parameters
            .Add(p => p.ErrorMessage, "Test error"));

        cut.Find(".error-banner").MarkupMatches("<div class=\"error-banner\"><p>Test error</p><button aria-label=\"Dismiss error\">×</button></div>");
    }

    [Fact]
    public void ErrorBanner_WithNullMessage_DoesNotRenderBanner()
    {
        var cut = RenderComponent<ErrorBanner>(parameters => parameters
            .Add(p => p.ErrorMessage, (string?)null));

        Assert.Empty(cut.FindAll(".error-banner"));
    }

    [Fact]
    public void ErrorBanner_WithEmptyMessage_DoesNotRenderBanner()
    {
        var cut = RenderComponent<ErrorBanner>(parameters => parameters
            .Add(p => p.ErrorMessage, ""));

        Assert.Empty(cut.FindAll(".error-banner"));
    }

    [Fact]
    public void ErrorBanner_ClickDismiss_InvokesCallback()
    {
        var dismissed = false;
        var cut = RenderComponent<ErrorBanner>(parameters => parameters
            .Add(p => p.ErrorMessage, "Test error")
            .Add(p => p.OnDismiss, () => dismissed = true));

        cut.Find("button").Click();

        Assert.True(dismissed);
    }
}

