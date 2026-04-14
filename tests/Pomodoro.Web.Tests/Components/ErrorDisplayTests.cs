using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.Shared;
using Xunit;

namespace Pomodoro.Web.Tests;

/// <summary>
/// Tests for ErrorDisplay component
/// </summary>
public class ErrorDisplayTests : TestContext
{
    public ErrorDisplayTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ErrorDisplay_Renders_WithException()
    {
        // Arrange
        var exception = new Exception("Test error message");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        Assert.NotNull(cut.Find(".error-container"));
        Assert.NotNull(cut.Find(".error-content"));
        cut.Markup.Contains("Test error message");
    }

    [Fact]
    public void ErrorDisplay_RendersDefaultMessage_WithoutException()
    {
        // Arrange
        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, null));

        // Assert
        Assert.NotNull(cut.Find(".error-container"));
        cut.Markup.Contains(Constants.ErrorDisplay.DefaultErrorMessage);
    }

    [Fact]
    public void ErrorDisplay_ShowsErrorIcon()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        cut.Markup.Contains(Constants.ErrorDisplay.ErrorIcon);
    }

    [Fact]
    public void ErrorDisplay_ShowsErrorTitle()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        cut.Markup.Contains(Constants.ErrorDisplay.ErrorTitle);
    }

    [Fact]
    public void ErrorDisplay_InvokesOnRetry_WhenRetryButtonClicked()
    {
        // Arrange
        var exception = new Exception("Test error");
        var retryInvoked = false;

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception)
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => retryInvoked = true)));

        var retryButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains(Constants.ErrorDisplay.RetryButtonText));
        retryButton?.Click();

        // Assert
        Assert.True(retryInvoked);
    }

    [Fact]
    public void ErrorDisplay_ReloadsPage_WhenReloadButtonClicked()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        var reloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains(Constants.ErrorDisplay.ReloadButtonText));
        reloadButton?.Click();

        // Assert - JSRuntime should be invoked
        JSInterop.VerifyInvoke("location.reload", 1);
    }

    [Fact]
    public void ErrorDisplay_RendersBothButtons()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Contains(buttons, b => b.TextContent.Contains(Constants.ErrorDisplay.RetryButtonText));
        Assert.Contains(buttons, b => b.TextContent.Contains(Constants.ErrorDisplay.ReloadButtonText));
    }

    [Fact]
    public void ErrorDisplay_HasCorrectCssClasses()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        Assert.NotNull(cut.Find(".error-container"));
        Assert.NotNull(cut.Find(".error-content"));
        Assert.NotNull(cut.Find(".error-icon"));
        Assert.NotNull(cut.Find(".error-message"));
        Assert.NotNull(cut.Find(".error-actions"));
    }

    [Fact]
    public void ErrorDisplay_RendersErrorMessageProperty_WhenExceptionProvided()
    {
        // Arrange
        var exception = new Exception("Custom error message");

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Exception, exception));

        // Assert
        var messageElement = cut.Find(".error-message");
        Assert.NotNull(messageElement);
        messageElement.TextContent.Contains("Custom error message");
    }
}
