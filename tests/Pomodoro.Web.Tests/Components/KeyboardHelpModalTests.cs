using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pomodoro.Web.Components.Shared;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

public class KeyboardHelpModalTests : TestContext
{
    public KeyboardHelpModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void KeyboardHelpModal_HasVisibleClass_WhenVisible()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        Assert.NotNull(cut.Find(".keyboard-help-modal.visible"));
    }

    [Fact]
    public void KeyboardHelpModal_DoesNotHaveVisibleClass_WhenNotVisible()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".keyboard-help-modal.visible"));
        Assert.NotNull(cut.Find(".keyboard-help-modal"));
    }

    [Fact]
    public void KeyboardHelpModal_RendersTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var header = cut.Find(".modal-header h3");
        Assert.NotNull(header);
        Assert.False(string.IsNullOrEmpty(header.TextContent));
    }

    [Fact]
    public void KeyboardHelpModal_RendersCloseButton()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var closeButton = cut.Find(".modal-close");
        Assert.NotNull(closeButton);
    }

    [Fact]
    public void KeyboardHelpModal_RendersTimerSection()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var sections = cut.FindAll(".shortcut-section");
        Assert.True(sections.Count >= 1, "Should have at least one shortcut section");
    }

    [Fact]
    public void KeyboardHelpModal_RendersKeyboardShortcuts()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var keyElements = cut.FindAll("kbd");
        Assert.True(keyElements.Count >= 1, "Should have at least one keyboard shortcut key");
    }

    [Fact]
    public void KeyboardHelpModal_ClickCloseButton_InvokesOnClose()
    {
        // Arrange
        var onCloseInvoked = false;
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => onCloseInvoked = true)));

        // Act
        var closeButton = cut.Find(".modal-close");
        closeButton.Click();

        // Assert
        Assert.True(onCloseInvoked);
    }

    [Fact]
    public void KeyboardHelpModal_ClickBackdrop_InvokesOnClose()
    {
        // Arrange
        var onCloseInvoked = false;
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => onCloseInvoked = true)));

        // Act
        var backdrop = cut.Find(".modal-backdrop");
        backdrop.Click();

        // Assert
        Assert.True(onCloseInvoked);
    }

    [Fact]
    public void KeyboardHelpModal_RendersShortcutItems()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var shortcutItems = cut.FindAll(".shortcut-item");
        Assert.True(shortcutItems.Count >= 1, "Should have at least one shortcut item");
    }

    [Fact]
    public void KeyboardHelpModal_RendersAllSections()
    {
        // Arrange & Act
        var cut = RenderComponent<KeyboardHelpModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert - Should have Timer, Session, and Other sections
        var sections = cut.FindAll(".shortcut-section h4");
        Assert.True(sections.Count >= 3, "Should have at least 3 sections (Timer, Session, Other)");
    }
}
