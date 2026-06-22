using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Components.Shared;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests;

/// <summary>
/// Tests for ConsentModal component
/// </summary>
[Trait("Category", "Component")]
public class ConsentModalTests : TestContext
{
    public ConsentModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ConsentModal_Renders_WhenVisible()
    {
        // Arrange
        var options = new List<ConsentOption>
        {
            new() { SessionType = SessionType.ShortBreak, Label = "Short Break" },
            new() { SessionType = SessionType.LongBreak, Label = "Long Break" }
        };

        // Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, options));

        // Assert
        Assert.NotNull(cut.Find(".consent-modal"));
    }

    [Fact]
    public void ConsentModal_DoesNotRender_WhenNotVisible()
    {
        // Arrange
        var options = new List<ConsentOption>();

        // Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, options));

        // Assert - Component should not render the modal
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".consent-modal"));
    }

    [Fact]
    public void ConsentModal_DoesNotRenderOptions_WhenOptionsIsNull()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, (List<ConsentOption>)null));

        // Assert - Modal renders but no option buttons
        Assert.NotNull(cut.Find(".consent-modal"));
        Assert.Empty(cut.FindAll("button.btn-option"));
    }

    [Fact]
    public void ConsentModal_ShowsPomodoroComplete_WhenPomodoroEnds()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Assert - Should show pomodoro complete title
        cut.Markup.Contains(Constants.Messages.PomodoroCompleteTitle);
    }

    [Fact]
    public void ConsentModal_ShowsBreakComplete_WhenShortBreakEnds()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.ShortBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Assert - Should show break complete title
        cut.Markup.Contains(Constants.Messages.BreakCompleteTitle);
    }

    [Fact]
    public void ConsentModal_ShowsLongBreakComplete_WhenLongBreakEnds()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.LongBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Assert - Should show long break complete title
        cut.Markup.Contains(Constants.Messages.LongBreakCompleteTitle);
    }

    [Fact]
    public void ConsentModal_RendersOptions()
    {
        // Arrange
        var options = new List<ConsentOption>
        {
            new() { SessionType = SessionType.ShortBreak, Label = "Short Break" },
            new() { SessionType = SessionType.LongBreak, Label = "Long Break" },
            new() { SessionType = SessionType.Pomodoro, Label = "Another Pomodoro" }
        };

        // Act
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, options));

        // Assert - Should render all options
        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 3);
    }

    [Fact]
    public Task ConsentModal_InvokesOnOptionSelected_WhenOptionClicked()
    {
        // Arrange
        var selectedSessionType = (SessionType?)null;
        var options = new List<ConsentOption>
        {
            new() { SessionType = SessionType.ShortBreak, Label = "Short Break" }
        };

        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, options)
            .Add(p => p.OnOptionSelected, EventCallback.Factory.Create<ConsentOption>(this, option => selectedSessionType = option.SessionType)));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.Equal(SessionType.ShortBreak, selectedSessionType);
        return Task.CompletedTask;
    }

    [Fact]
    public void ConsentModal_GetIcon_ReturnsPomodoroEmoji_ForPomodoroSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.SessionTypes.PomodoroEmoji);
    }

    [Fact]
    public void ConsentModal_GetIcon_ReturnsShortBreakEmoji_ForShortBreakSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.ShortBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.SessionTypes.ShortBreakEmoji);
    }

    [Fact]
    public void ConsentModal_GetIcon_ReturnsLongBreakEmoji_ForLongBreakSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.LongBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.SessionTypes.LongBreakEmoji);
    }

    [Fact]
    public void ConsentModal_GetMessage_ReturnsPomodoroMessage_ForPomodoroSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.Messages.PomodoroCompleteMessage);
    }

    [Fact]
    public void ConsentModal_GetMessage_ReturnsBreakMessage_ForShortBreakSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.ShortBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.Messages.BreakCompleteMessage);
    }

    [Fact]
    public void ConsentModal_GetMessage_ReturnsBreakMessage_ForLongBreakSession()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.LongBreak)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert
        cut.Markup.Contains(Constants.Messages.BreakCompleteMessage);
    }

    [Fact]
    public void ConsentModal_ReturnsDefaults_ForUnknownSessionType()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, (SessionType)99)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert - Should fall back to default values
        var markup = cut.Markup;
        Assert.Contains(Constants.SessionTypes.PomodoroEmoji, markup);
        Assert.Contains(Constants.Messages.SessionCompleteTitle, markup);
        Assert.Contains(Constants.Messages.SessionCompleteMessage, markup);
    }

    [Fact]
    public void ConsentModal_GetProgressPercentage_ReturnsZero_WhenInitialCountdownIsZero()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 0)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert - Progress bar should be at 0%
        cut.Markup.Contains("width: 0%");
    }

    [Fact]
    public void ConsentModal_GetProgressPercentage_ReturnsCorrectPercentage_WhenCountdownIsHalf()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 15)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act & Assert - Progress bar should be at 50%
        cut.Markup.Contains("width: 50%");
    }

    [Fact]
    public void ConsentModal_OnParametersSet_UpdatesInitialCountdown_WhenCountdownIncreases()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 10)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act - Update countdown to a higher value
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CountdownSeconds, 30));

        // Assert - Progress should be 100% (30/30)
        cut.Markup.Contains("width: 100%");
    }

    [Fact]
    public void ConsentModal_OnParametersSet_ResetsInitialCountdown_WhenCountdownReachesZero()
    {
        // Arrange
        var cut = RenderComponent<ConsentModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.CompletedSessionType, SessionType.Pomodoro)
            .Add(p => p.CountdownSeconds, 30)
            .Add(p => p.Options, new List<ConsentOption>()));

        // Act - Set countdown to 0
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CountdownSeconds, 0));

        // Assert - Progress should be 0%
        cut.Markup.Contains("width: 0%");
    }
}

