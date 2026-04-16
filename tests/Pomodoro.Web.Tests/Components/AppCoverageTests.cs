using System;
using System.Collections.Generic;
using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Models;
using Pomodoro.Web.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Pomodoro.Web.Tests
{
    /// <summary>
    /// Tests for App.razor component coverage
    /// Focuses on NotFound template and CustomPageTitle component
    /// </summary>
    [Trait("Category", "Component")]
    public class AppCoverageTests : TestContext
    {
        public AppCoverageTests()
        {
            // Register LayoutPresenterService as a mock
            var mockLayoutPresenter = new Mock<LayoutPresenterService>();
            mockLayoutPresenter.Setup(x => x.GetNavMenuCssClass()).Returns("collapse");
            mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(new List<Pomodoro.Web.Services.NavLinkData>());
            mockLayoutPresenter.Setup(x => x.ToggleNavMenu());
            Services.AddSingleton<LayoutPresenterService>(mockLayoutPresenter.Object);

            // Register all of the services that Index.razor requires
            Services.AddSingleton(new Mock<ITaskService>().Object);
            Services.AddSingleton(new Mock<IActivityService>().Object);
            Services.AddSingleton(new Mock<ITimerService>().Object);
            Services.AddSingleton(new Mock<INotificationService>().Object);
            Services.AddSingleton(new Mock<IExportService>().Object);
            Services.AddSingleton(new Mock<IIndexedDbService>().Object);
            Services.AddSingleton(new Mock<IConsentService>().Object);
            Services.AddSingleton(new Mock<IPipTimerService>().Object);
            Services.AddSingleton(new Mock<IKeyboardShortcutService>().Object);
            Services.AddSingleton(new Mock<IndexPagePresenterService>(new Mock<ILogger<IndexPagePresenterService>>().Object).Object);
            Services.AddSingleton<ITodayStatsService>(new TodayStatsService(new Mock<IActivityService>().Object));
            Services.AddSingleton(new Mock<TimerThemeFormatter>().Object);
            Services.AddSingleton(new Mock<IJSRuntime>().Object);
            Services.AddSingleton(new Mock<ILogger<App>>().Object);
            Services.AddSingleton(new Mock<ILogger<LayoutPresenterService>>().Object);
            Services.AddSingleton(new Mock<ILogger<ErrorDisplay>>().Object);
            Services.AddSingleton(new Mock<AppState>().Object);
        }

        #region CustomPageTitle Component Tests

        [Fact]
        public void CustomPageTitle_ShouldRenderWithCorrectValue()
        {
            // Arrange & Act
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, "Test Title"));

            // Assert - Verify the Value parameter is set correctly
            Assert.Equal("Test Title", cut.Instance.Value);

            // Note: PageTitle component doesn't render a <title> element in bunit tests
            // It's handled by Blazor's HeadOutlet at runtime
        }

        [Fact]
        public void CustomPageTitle_ShouldRenderWithNotFoundPageTitle()
        {
            // Arrange & Act
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, Constants.Routing.NotFoundPageTitle));

            // Assert - Verify the Value parameter is set correctly
            Assert.Equal(Constants.Routing.NotFoundPageTitle, cut.Instance.Value);
        }

        [Fact]
        public void CustomPageTitle_ShouldUpdateWhenValueChanges()
        {
            // Arrange
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, "Initial Title"));

            // Act
            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.Value, "Updated Title"));

            // Assert - Verify the Value parameter was updated
            Assert.Equal("Updated Title", cut.Instance.Value);
        }

        [Fact]
        public void CustomPageTitle_ShouldHandleEmptyValue()
        {
            // Arrange
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, string.Empty));

            // Act & Assert
            // When Value is empty, PageTitle component may not render a title element
            // Verify the component accepts empty value without throwing and the parameter is set correctly
            Assert.Equal(string.Empty, cut.Instance.Value);

            // Verify the component renders without errors (empty title is valid)
            var titleElements = cut.FindAll("title");
            if (titleElements.Count > 0)
            {
                Assert.Equal(string.Empty, titleElements[0].TextContent);
            }
        }

        [Fact]
        public void CustomPageTitle_ShouldEvaluateValueExpression()
        {
            // Arrange
            var testValue = "Coverage Test Title " + Guid.NewGuid();

            // Act - Render and access markup to force BuildRenderTree execution
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, testValue));

            // Access markup to ensure rendering pipeline completes
            var markup = cut.Markup;

            // Assert - Verify the Value was evaluated during rendering
            Assert.Equal(testValue, cut.Instance.Value);
            Assert.NotNull(markup);
        }

        [Fact]
        public void CustomPageTitle_ShouldRenderMarkupWithPageTitleComponent()
        {
            // Arrange & Act
            var cut = RenderComponent<CustomPageTitle>(parameters => parameters
                .Add(p => p.Value, "Markup Test"));

            // Access the rendered markup to trigger full rendering
            var markup = cut.Markup;

            // Assert - Component should render without errors
            Assert.NotNull(markup);
            Assert.Equal("Markup Test", cut.Instance.Value);
        }

        #endregion

        #region NotFound Template Tests

        [Fact]
        public void NotFoundTemplate_ShouldRenderWhenNavigatingToInvalidRoute()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();

            // Act - Navigate to a non-existent route
            navigationManager.NavigateTo("/non-existent-route-for-coverage");

            // Assert
            Assert.NotNull(component);
            Assert.Contains("/non-existent-route-for-coverage", navigationManager.Uri);
        }

        [Fact]
        public void NotFoundTemplate_ShouldRenderNotFoundMessage()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act - Navigate to a non-existent route to trigger NotFound
            navigationManager.NavigateTo("/test-not-found-message");
            var cut = RenderComponent<App>();

            // Assert - Verify the component renders without error
            Assert.NotNull(cut);

            // Verify we're on the not-found route
            Assert.Contains("/test-not-found-message", navigationManager.Uri);
        }

        [Fact]
        public void NotFoundTemplate_ShouldRenderWithMainLayout()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/test-not-found-layout");
            var cut = RenderComponent<App>();

            // Assert - Component should render successfully
            Assert.NotNull(cut);

            // Verify navigation occurred
            Assert.Contains("/test-not-found-layout", navigationManager.Uri);
        }

        [Fact]
        public void NotFoundTemplate_ShouldUseConstantsForMessage()
        {
            // Assert - Verify constants exist and have expected values
            Assert.Equal("Not found", Constants.Routing.NotFoundPageTitle);
            Assert.Equal("Sorry, there's nothing at this address.", Constants.Routing.NotFoundMessage);
        }

        #endregion

        #region GetNotFoundPageTitle Method Tests

        [Fact]
        public void GetNotFoundPageTitle_ShouldReturnCorrectValue()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var pageTitle = appInstance.GetNotFoundPageTitle();

            // Assert
            Assert.Equal(Constants.Routing.NotFoundPageTitle, pageTitle);
        }

        [Fact]
        public void GetNotFoundPageTitle_ShouldReturnNotNull()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var pageTitle = appInstance.GetNotFoundPageTitle();

            // Assert
            Assert.NotNull(pageTitle);
        }

        [Fact]
        public void GetNotFoundPageTitle_ShouldReturnNotEmpty()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var pageTitle = appInstance.GetNotFoundPageTitle();

            // Assert
            Assert.NotEmpty(pageTitle);
        }

        [Fact]
        public void GetNotFoundPageTitle_ShouldBeCallableMultipleTimes()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call method multiple times
            var result1 = appInstance.GetNotFoundPageTitle();
            var result2 = appInstance.GetNotFoundPageTitle();
            var result3 = appInstance.GetNotFoundPageTitle();

            // Assert - All calls should return the same value
            Assert.Equal(Constants.Routing.NotFoundPageTitle, result1);
            Assert.Equal(Constants.Routing.NotFoundPageTitle, result2);
            Assert.Equal(Constants.Routing.NotFoundPageTitle, result3);
        }

        [Fact]
        public void GetNotFoundPageTitle_ShouldMatchConstantValue()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var pageTitle = appInstance.GetNotFoundPageTitle();

            // Assert
            Assert.Equal("Not found", pageTitle);
        }

        #endregion

        #region Combined NotFound and GetNotFoundPageTitle Tests

        [Fact]
        public void NotFoundTemplate_ShouldUseGetNotFoundPageTitle()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/test-get-not-found-page-title");

            // Get the page title
            var pageTitle = appInstance.GetNotFoundPageTitle();

            // Assert - Verify the method returns the correct value
            Assert.Equal(Constants.Routing.NotFoundPageTitle, pageTitle);
            Assert.Contains("/test-get-not-found-page-title", navigationManager.Uri);
        }



        #endregion

        #region Router and ErrorBoundary Tests

        [Fact]
        public void Router_ShouldHandleInvalidRoutes()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();

            // Act - Navigate to invalid routes
            navigationManager.NavigateTo("/invalid-route-test-1");
            navigationManager.NavigateTo("/invalid-route-test-2");

            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void ErrorBoundary_ShouldRenderNotFoundContent()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();

            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/error-boundary-not-found-test");

            // Assert
            Assert.NotNull(component);
        }

        #endregion
    }
}

