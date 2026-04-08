using Bunit;
using FluentAssertions;
using Pomodoro.Web.Pages;
using Xunit;
using IndexPage = Pomodoro.Web.Pages.Index;

namespace Pomodoro.Web.Tests.Components.Pages;

/// <summary>
/// Tests for the Index page component.
/// These tests verify rendering and basic interactions of the Index page.
/// </summary>
public class IndexPageTests : TestHelper
{
    [Fact]
    public void IndexPage_RendersWithoutErrors()
    {
        // Arrange & Act
        var cut = RenderComponent<IndexPage>();
        
        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_ShowsTimerDisplay()
    {
        // Arrange & Act
        var cut = RenderComponent<IndexPage>();
        
        // Assert
        cut.Markup.Should().Contain("timer");
    }

    [Fact]
    public void IndexPage_HasTaskList()
    {
        // Arrange & Act
        var cut = RenderComponent<IndexPage>();
        
        // Assert
        cut.Markup.Should().Contain("task");
    }
}
