using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Components;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

public class CustomPageTitleTests : TestContext
{
    public CustomPageTitleTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void CustomPageTitle_RendersWithProvidedValue()
    {
        RenderComponent<HeadOutlet>();
        var cut = RenderComponent<CustomPageTitle>(parameters => parameters
            .Add(p => p.Value, "Test Title"));

        Assert.NotNull(cut.Instance);
    }
}
