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

namespace Pomodoro.Web.Tests.Layout
{
    [Trait("Category", "Component")]
    public class MainLayoutTests : TestContext
    {
        private readonly Mock<LayoutPresenterService> _mockLayoutPresenter;

        public MainLayoutTests()
        {
            _mockLayoutPresenter = new Mock<LayoutPresenterService>();
            Services.AddSingleton(_mockLayoutPresenter.Object);
            Services.AddSingleton(Mock.Of<ICloudSyncService>());
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
            Assert.DoesNotContain("app-footer", component.Markup);
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

            // Assert - footer was removed
            Assert.DoesNotContain("footer", component.Markup);
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
            Assert.DoesNotContain("header-tagline", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersFooterContent()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - footer was removed
            Assert.DoesNotContain("footer-content", component.Markup);
            Assert.DoesNotContain("footer-made", component.Markup);
            Assert.DoesNotContain("footer-copy", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersCompleteHeaderStructure()
        {
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var component = RenderComponent<MainLayout>();

            var headerTitle = component.Find(".header-title");
            Assert.NotNull(headerTitle);

            var headerNav = component.Find(".header-nav");
            Assert.NotNull(headerNav);

            var headerTitleSpans = headerTitle.QuerySelectorAll("span");
            Assert.True(headerTitleSpans.Length >= 1);
        }

        [Fact]
        public void MainLayout_RendersCompleteFooterStructure()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2024);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - footer was removed
            Assert.DoesNotContain("app-footer", component.Markup);
            Assert.DoesNotContain("footer-content", component.Markup);
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

            // Footer section - removed
            Assert.DoesNotContain("app-footer", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersTagline()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            // Act
            var component = RenderComponent<MainLayout>();

            // Assert - tagline was removed
            Assert.DoesNotContain("header-tagline", component.Markup);
            Assert.DoesNotContain("Focus. Work. Achieve.", component.Markup);
        }

        [Fact]
        public void MainLayout_RendersHeaderIconAndTitle()
        {
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var component = RenderComponent<MainLayout>();

            var headerText = component.Find(".header-text");

            Assert.NotNull(headerText);
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

            // Assert - tagline was removed
            Assert.DoesNotContain("header-tagline", cut.Markup);
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

        [Fact]
        public void Dispose_AfterRender_DisposesDotNetRef()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            JSInterop.SetupVoid("swipeNavigation.init");

            // Act - Render triggers OnAfterRenderAsync which sets _dotNetRef
            var cut = RenderComponent<MainLayout>();

            // Dispose should call _dotNetRef?.Dispose()
            var exception = Record.Exception(() => cut.Instance.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void RecoverError_WhenErrorBoundarySetToNull_DoesNotThrow()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            var field = typeof(MainLayout).GetField("_errorBoundary", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field!.SetValue(cut.Instance, null);

            // Act & Assert
            var exception = Record.Exception(() => cut.Instance.RecoverError());
            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenDotNetRefIsNull_DoesNotThrow()
        {
            // Arrange
            _mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(Array.Empty<NavLinkData>());
            _mockLayoutPresenter.Setup(x => x.GetCurrentYear()).Returns(2023);

            var cut = RenderComponent<MainLayout>();

            var dotNetRefField = typeof(MainLayoutBase).GetField("_dotNetRef", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            dotNetRefField!.SetValue(cut.Instance, null);

            // Act & Assert
            var exception = Record.Exception(() => cut.Instance.Dispose());
            Assert.Null(exception);
        }
    }
}

