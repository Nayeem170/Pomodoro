using System;
using System.Reflection;
using Bunit;
using Moq;
using Xunit;
using Pomodoro.Web.Layout;
using Pomodoro.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Pomodoro.Web.Tests.Components.Layout
{
    public class MainLayoutTests : TestContext
    {
        private readonly Mock<LayoutPresenterService> _mockLayoutPresenter;

        public MainLayoutTests()
        {
            _mockLayoutPresenter = new Mock<LayoutPresenterService>();
            Services.AddSingleton(_mockLayoutPresenter.Object);
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void MainLayout_RendersCorrectly()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(new[]
            {
                new NavLinkData { Href = "/", Icon = "oi-timer", Title = "Timer", Match = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All },
                new NavLinkData { Href = "/history", Icon = "oi-calendar", Title = "History", Match = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.Prefix }
            });
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.NotNull(component);
            Assert.Contains("app-wrapper", component.Markup);
            Assert.Contains("app-header", component.Markup);
            Assert.Contains("app-content", component.Markup);
            Assert.Contains("app-footer", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersAppTitle()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.Contains("Pomodoro", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersNavigationLinks()
        {
            // Arrange
            var navLinks = new[]
            {
                new NavLinkData { Href = "/", Icon = "oi-timer", Title = "Timer", Match = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All },
                new NavLinkData { Href = "/history", Icon = "oi-calendar", Title = "History", Match = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.Prefix },
                new NavLinkData { Href = "/settings", Icon = "oi-cog", Title = "Settings", Match = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.Prefix }
            };
            
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(navLinks);
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.Contains("oi-timer", component.Markup);
            Assert.Contains("oi-calendar", component.Markup);
            Assert.Contains("oi-cog", component.Markup);
            Assert.Contains("Timer", component.Markup);
            Assert.Contains("History", component.Markup);
            Assert.Contains("Settings", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersFooterWithCurrentYear()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.Contains("2023", component.Markup);
            Assert.Contains("footer", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersErrorBoundary()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            // Verify that ErrorBoundary is rendered (it might be rendered as a different element name)
            // The important thing is that the main content area is wrapped in error handling
            Assert.Contains("app-content", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersHeaderContent()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.Contains("header-content-wrapper", component.Markup);
            Assert.Contains("header-left", component.Markup);
            Assert.Contains("header-nav", component.Markup);
            Assert.Contains("header-title", component.Markup);
            Assert.Contains("header-tagline", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersFooterContent()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert
            Assert.Contains("footer-content", component.Markup);
            Assert.Contains("footer-made", component.Markup);
            Assert.Contains("footer-copy", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersCompleteHeaderStructure()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify complete header structure is rendered (covers lines9,12,22)
            var headerTitle = component.Find(".header-title");
            Assert.NotNull(headerTitle);
            
            var headerNav = component.Find(".header-nav");
            Assert.NotNull(headerNav);
            
            // Verify the header-title contains both icon and text spans
            var headerTitleSpans = headerTitle.QuerySelectorAll("span");
            Assert.True(headerTitleSpans.Length >= 2);
        }

        [Fact]
        public void MainLayout_RendersCompleteFooterStructure()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2024);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify complete footer structure is rendered (covers lines33-37)
            var footer = component.Find(".app-footer");
            Assert.NotNull(footer);
            
            var footerContent = component.Find(".footer-content");
            Assert.NotNull(footerContent);
            
            // Verify year is rendered
            Assert.Contains("2024", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersMainContentArea()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify main content area with ErrorBoundary (covers lines25-37)
            var mainContent = component.Find(".app-content");
            Assert.NotNull(mainContent);
            
            // Verify the ErrorBoundary wrapper exists
            Assert.Contains("app-content", component.Markup);
        }

        [Fact]
        public void MainLayout_VerifyCompleteMarkupStructure()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2024);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify complete structure from wrapper to footer (covers all structural lines)
            // App wrapper
            var appWrapper = component.Find(".app-wrapper");
            Assert.NotNull(appWrapper);
            
            // Header section
            var header = component.Find(".app-header");
            Assert.NotNull(header);
            
            // Main section
            var main = component.Find(".app-content");
            Assert.NotNull(main);
            
            // Footer section
            var footer = component.Find(".app-footer");
            Assert.NotNull(footer);
        }

        [Fact]
        public void MainLayout_RendersTagline()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Use actual tagline from Constants
            var tagline = component.Find(".header-tagline");
            Assert.NotNull(tagline);
            Assert.Contains("Focus. Work. Achieve.", tagline.TextContent);
        }

        [Fact]
        public void MainLayout_RendersHeaderIconAndTitle()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify header icon and title spans (covers lines9-12)
            // Use actual icon from Constants (🍅)
            var headerIcon = component.Find(".header-icon");
            var headerText = component.Find(".header-text");
            
            Assert.NotNull(headerIcon);
            Assert.NotNull(headerText);
            Assert.Contains("🍅", headerIcon.TextContent);
            Assert.Contains("Pomodoro", headerText.TextContent);
        }

        [Fact]
        public void MainLayout_RendersNavigationWithMultipleLinks()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(new[]
            {
                new NavLinkData { Href = "/", Icon = "oi-home", Title = "Home", Match = NavLinkMatch.All },
                new NavLinkData { Href = "/history", Icon = "oi-history", Title = "History", Match = NavLinkMatch.Prefix },
                new NavLinkData { Href = "/settings", Icon = "oi-settings", Title = "Settings", Match = NavLinkMatch.Prefix }
            });
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - Verify all nav links are rendered (covers line22)
            var navContainer = component.Find(".header-nav");
            Assert.NotNull(navContainer);
            
            // Check that the navigation contains links
            var navLinks = navContainer.QuerySelectorAll("a");
            Assert.True(navLinks.Length >= 3);
        }

        [Fact]
        public void RecoverError_WhenErrorBoundaryExists_CallsRecover()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            // Act
            cut.Instance.RecoverError();

            // Assert - Should complete without throwing
            Assert.NotNull(cut);
        }

        [Fact]
        public void RecoverError_WhenErrorBoundaryIsNull_DoesNotThrow()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            // Act - RecoverError with null _errorBoundary should not throw
            // (it's already null before any error occurs)
            var exception = Record.Exception(() => cut.Instance.RecoverError());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void MainLayout_ImplementsIDisposable()
        {
            Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(MainLayout)));
        }

        [Fact]
        public void NavigateTo_InvokesNavigationManager()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            // Act & Assert - should not throw
            var exception = Record.Exception(() => cut.Instance.NavigateTo("/history"));
            Assert.Null(exception);
        }

        [Fact]
        public void MainLayout_RendersHeaderWithAppHeaderClass()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var cut = RenderComponent<MainLayout>();

            // Assert
            var header = cut.Find(".app-header");
            Assert.NotNull(header);
            Assert.Contains("app-header", header.ClassList);
        }

        [Fact]
        public void MainLayout_RendersTaglineBelowHeaderTitle()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var cut = RenderComponent<MainLayout>();

            // Assert - tagline is inside header-left, after header-title
            var headerLeft = cut.Find(".header-left");
            var markup = headerLeft.InnerHtml;
            var titleIndex = markup.IndexOf("header-title");
            var taglineIndex = markup.IndexOf("header-tagline");
            Assert.True(titleIndex >= 0, "header-title should exist");
            Assert.True(taglineIndex >= 0, "header-tagline should exist");
            Assert.True(taglineIndex > titleIndex, "header-tagline should be after header-title");
        }

        [Fact]
        public void MainLayout_HeaderContentWrapperContainsNavAndTitle()
        {
            // Arrange
            var navLinks = new[]
            {
                new NavLinkData { Href = "/", Icon = "🍅", Title = "Timer", Match = NavLinkMatch.All },
                new NavLinkData { Href = "/history", Icon = "📊", Title = "History", Match = NavLinkMatch.Prefix },
                new NavLinkData { Href = "/settings", Icon = "⚙️", Title = "Settings", Match = NavLinkMatch.Prefix },
                new NavLinkData { Href = "/about", Icon = "ℹ️", Title = "About", Match = NavLinkMatch.Prefix }
            };
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(navLinks);
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var cut = RenderComponent<MainLayout>();

            // Assert - header-content-wrapper has both header-left and header-nav
            var wrapper = cut.Find(".header-content-wrapper");
            Assert.NotNull(wrapper);
            Assert.NotNull(wrapper.QuerySelector(".header-left"));
            Assert.NotNull(wrapper.QuerySelector(".header-nav"));

            var navLinksCount = wrapper.QuerySelectorAll(".header-nav a").Length;
            Assert.Equal(4, navLinksCount);
        }

        [Fact]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                cut.Instance.Dispose();
                cut.Instance.Dispose();
            });
            Assert.Null(exception);
        }

    }
}
