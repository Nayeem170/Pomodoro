using Microsoft.AspNetCore.Components.Routing;
using Xunit;
using FluentAssertions;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services
{
    /// <summary>
    /// Tests for LayoutPresenterService
    /// </summary>
    public class LayoutPresenterServiceTests
    {
        private readonly LayoutPresenterService _service;

        public LayoutPresenterServiceTests()
        {
            _service = new LayoutPresenterService();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeWithCollapsedStateTrue()
        {
            // Act
            var service = new LayoutPresenterService();

            // Assert
            service.GetNavMenuCollapsedState().Should().BeTrue();
        }

        #endregion

        #region GetNavMenuCssClass Tests

        [Fact]
        public void GetNavMenuCssClass_WhenCollapsed_ReturnsCollapse()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(true);

            // Act
            var result = _service.GetNavMenuCssClass();

            // Assert
            result.Should().Be("collapse");
        }

        [Fact]
        public void GetNavMenuCssClass_WhenNotCollapsed_ReturnsNull()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(false);

            // Act
            var result = _service.GetNavMenuCssClass();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetNavMenuCssClass_WhenDefaultState_ReturnsCollapse()
        {
            // Act - Default state is collapsed (true)
            var result = _service.GetNavMenuCssClass();

            // Assert
            result.Should().Be("collapse");
        }

        #endregion

        #region ToggleNavMenu Tests

        [Fact]
        public void ToggleNavMenu_WhenCollapsed_ReturnsFalse()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(true);

            // Act
            var result = _service.ToggleNavMenu();

            // Assert
            result.Should().BeFalse();
            _service.GetNavMenuCollapsedState().Should().BeFalse();
        }

        [Fact]
        public void ToggleNavMenu_WhenNotCollapsed_ReturnsTrue()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(false);

            // Act
            var result = _service.ToggleNavMenu();

            // Assert
            result.Should().BeTrue();
            _service.GetNavMenuCollapsedState().Should().BeTrue();
        }

        [Fact]
        public void ToggleNavMenu_CalledMultipleTimes_TogglesCorrectly()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(true);

            // Act
            var result1 = _service.ToggleNavMenu();
            var result2 = _service.ToggleNavMenu();
            var result3 = _service.ToggleNavMenu();

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeTrue();
            result3.Should().BeFalse();
        }

        #endregion

        #region GetNavMenuCollapsedState Tests

        [Fact]
        public void GetNavMenuCollapsedState_WhenCollapsed_ReturnsTrue()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(true);

            // Act
            var result = _service.GetNavMenuCollapsedState();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetNavMenuCollapsedState_WhenNotCollapsed_ReturnsFalse()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(false);

            // Act
            var result = _service.GetNavMenuCollapsedState();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetNavMenuCollapsedState_WhenDefaultState_ReturnsTrue()
        {
            // Act - Default state is collapsed (true)
            var result = _service.GetNavMenuCollapsedState();

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region SetNavMenuCollapsedState Tests

        [Fact]
        public void SetNavMenuCollapsedState_WhenTrue_SetsCollapsed()
        {
            // Act
            _service.SetNavMenuCollapsedState(true);

            // Assert
            _service.GetNavMenuCollapsedState().Should().BeTrue();
        }

        [Fact]
        public void SetNavMenuCollapsedState_WhenFalse_SetsNotCollapsed()
        {
            // Act
            _service.SetNavMenuCollapsedState(false);

            // Assert
            _service.GetNavMenuCollapsedState().Should().BeFalse();
        }

        [Fact]
        public void SetNavMenuCollapsedState_CalledMultipleTimes_UpdatesCorrectly()
        {
            // Act
            _service.SetNavMenuCollapsedState(true);
            var result1 = _service.GetNavMenuCollapsedState();
            _service.SetNavMenuCollapsedState(false);
            var result2 = _service.GetNavMenuCollapsedState();
            _service.SetNavMenuCollapsedState(true);
            var result3 = _service.GetNavMenuCollapsedState();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeFalse();
            result3.Should().BeTrue();
        }

        #endregion

        #region GetCurrentYear Tests

        [Fact]
        public void GetCurrentYear_ReturnsCurrentUtcYear()
        {
            // Act
            var result = _service.GetCurrentYear();

            // Assert
            result.Should().Be(DateTime.UtcNow.Year);
        }

        [Fact]
        public void GetCurrentYear_CalledMultipleTimes_ReturnsConsistentYear()
        {
            // Act
            var result1 = _service.GetCurrentYear();
            var result2 = _service.GetCurrentYear();
            var result3 = _service.GetCurrentYear();

            // Assert
            result1.Should().Be(result2);
            result2.Should().Be(result3);
        }

        #endregion

        #region ShouldHighlightNavLink Tests

        [Fact]
        public void ShouldHighlightNavLink_WhenHrefIsNull_ReturnsFalse()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home");

            // Act
            var result = _service.ShouldHighlightNavLink(null!, currentUri);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenHrefIsEmpty_ReturnsFalse()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home");

            // Act
            var result = _service.ShouldHighlightNavLink(string.Empty, currentUri);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchAllAndPathsMatch_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.All);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchAllAndPathsDoNotMatch_ReturnsFalse()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home");

            // Act
            var result = _service.ShouldHighlightNavLink("/settings", currentUri, NavLinkMatch.All);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchAllAndRootPathWithNonEmptyHref_ReturnsFalse()
        {
            // Arrange - root path with non-matching href forces || right-side evaluation
            var currentUri = new Uri("http://localhost/");

            // Act
            var result = _service.ShouldHighlightNavLink("/settings", currentUri, NavLinkMatch.All);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchAllAndRootPathMatchesEmptyHref_ReturnsFalse()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/");

            // Act
            var result = _service.ShouldHighlightNavLink("", currentUri, NavLinkMatch.All);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchPrefixAndPathStartsWithHref_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home/tasks");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.Prefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchPrefixAndPathDoesNotStartWithHref_ReturnsFalse()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/settings");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.Prefix);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchPrefixAndCaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/HOME");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.Prefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchPrefixAndQueryParameters_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home?param=value");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.Prefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchPrefixAndFragment_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home#section");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri, NavLinkMatch.Prefix);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldHighlightNavLink_WhenMatchDefaultPrefix_ReturnsTrue()
        {
            // Arrange
            var currentUri = new Uri("http://localhost/home");

            // Act
            var result = _service.ShouldHighlightNavLink("/home", currentUri);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region GetNavigationLinks Tests

        [Fact]
        public void GetNavigationLinks_ReturnsFourLinks()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            links.Should().HaveCount(4);
        }

        [Fact]
        public void GetNavigationLinks_FirstLinkIsTimer()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            links[0].Title.Should().Be("Timer");
            links[0].Href.Should().Be("/");
            links[0].Match.Should().Be(NavLinkMatch.All);
        }

        [Fact]
        public void GetNavigationLinks_SecondLinkIsHistory()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            links[1].Title.Should().Be("History");
            links[1].Href.Should().Be("/history");
            links[1].Match.Should().Be(NavLinkMatch.Prefix);
        }

        [Fact]
        public void GetNavigationLinks_ThirdLinkIsSettings()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            links[2].Title.Should().Be("Settings");
            links[2].Href.Should().Be("/settings");
            links[2].Match.Should().Be(NavLinkMatch.Prefix);
        }

        [Fact]
        public void GetNavigationLinks_FourthLinkIsAbout()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            links[3].Title.Should().Be("About Pomodoro");
            links[3].Href.Should().Be("/about");
            links[3].Match.Should().Be(NavLinkMatch.Prefix);
        }

        [Fact]
        public void GetNavigationLinks_AllLinksHaveIcons()
        {
            // Act
            var links = _service.GetNavigationLinks().ToList();

            // Assert
            foreach (var link in links)
            {
                link.Icon.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void GetNavigationLinks_CalledMultipleTimes_ReturnsNewInstances()
        {
            // Act
            var links1 = _service.GetNavigationLinks().ToList();
            var links2 = _service.GetNavigationLinks().ToList();

            // Assert
            links1.Should().NotBeSameAs(links2);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Integration_ToggleAndGetCssClass_WorksCorrectly()
        {
            // Arrange
            _service.SetNavMenuCollapsedState(true);

            // Act
            var css1 = _service.GetNavMenuCssClass();
            var state1 = _service.ToggleNavMenu();
            var css2 = _service.GetNavMenuCssClass();
            var state2 = _service.ToggleNavMenu();
            var css3 = _service.GetNavMenuCssClass();

            // Assert
            css1.Should().Be("collapse");
            state1.Should().BeFalse();
            css2.Should().BeNull();
            state2.Should().BeTrue();
            css3.Should().Be("collapse");
        }

        [Fact]
        public void Integration_NavigationLinksAndHighlighting_WorksCorrectly()
        {
            // Arrange
            var homeUri = new Uri("http://localhost/");
            var historyUri = new Uri("http://localhost/history");
            var settingsUri = new Uri("http://localhost/settings");

            // Act
            var links = _service.GetNavigationLinks().ToList();

            var homeHighlighted = _service.ShouldHighlightNavLink(links[0].Href, homeUri, links[0].Match);
            var historyHighlighted = _service.ShouldHighlightNavLink(links[1].Href, historyUri, links[1].Match);
            var settingsHighlighted = _service.ShouldHighlightNavLink(links[2].Href, settingsUri, links[2].Match);

            // Assert
            homeHighlighted.Should().BeTrue();
            historyHighlighted.Should().BeTrue();
            settingsHighlighted.Should().BeTrue();
        }

        #endregion
    }
}