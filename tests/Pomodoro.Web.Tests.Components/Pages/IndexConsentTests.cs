using Bunit;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.Pages;

/// <summary>
/// Tests for Index.razor.Consent.cs consent action handler
/// </summary>
public class IndexConsentTests : TestHelper
{
    public IndexConsentTests()
    {
        // Setup default mock behavior for ConsentService
        ConsentServiceMock
            .Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>()))
            .Returns(Task.CompletedTask);
    }

    #region HandleConsentOptionSelect Tests

    [Fact]
    public async Task HandleConsentOptionSelect_WithPomodoro_CallsSelectOptionAsync()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        
        // Act - Call method directly and await completion
        var task = cut.Instance.HandleConsentOptionSelect(SessionType.Pomodoro);
        await task;
        
        // Assert
        ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.Pomodoro), Times.Once);
    }

    [Fact]
    public async Task HandleConsentOptionSelect_WithShortBreak_CallsSelectOptionAsync()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        
        // Act - Call method directly and await completion
        var task = cut.Instance.HandleConsentOptionSelect(SessionType.ShortBreak);
        await task;
        
        // Assert
        ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.ShortBreak), Times.Once);
    }

    [Fact]
    public async Task HandleConsentOptionSelect_WithLongBreak_CallsSelectOptionAsync()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        
        // Act - Call method directly and await completion
        var task = cut.Instance.HandleConsentOptionSelect(SessionType.LongBreak);
        await task;
        
        // Assert
        ConsentServiceMock.Verify(x => x.SelectOptionAsync(SessionType.LongBreak), Times.Once);
    }

    [Fact]
    public async Task HandleConsentOptionSelect_OnException_SetsErrorMessage()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var expectedException = new InvalidOperationException("Test exception");
        ConsentServiceMock
            .Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>()))
            .ThrowsAsync(expectedException);
        
        // Act - Call method directly and await completion
        var task = cut.Instance.HandleConsentOptionSelect(SessionType.Pomodoro);
        await task;
        
        // Assert
        Assert.Contains("Error selecting consent option", cut.Instance.ErrorMessage);
        Assert.Contains("Test exception", cut.Instance.ErrorMessage);
    }

    #endregion
}
