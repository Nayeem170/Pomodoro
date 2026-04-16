using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Pomodoro.Web.Services
{
    /// <summary>
    /// Service for managing layout-related presentation logic
    /// </summary>
    public class LayoutPresenterService
    {
        private bool _collapseNavMenu = true;

        /// <summary>
        /// Gets the CSS class for the navigation menu based on its collapsed state
        /// </summary>
        /// <returns>CSS class name or null if not collapsed</returns>
        public virtual string? GetNavMenuCssClass()
        {
            return _collapseNavMenu ? "collapse" : null;
        }

        /// <summary>
        /// Toggles the navigation menu's collapsed state
        /// </summary>
        /// <returns>New collapsed state</returns>
        public virtual bool ToggleNavMenu()
        {
            _collapseNavMenu = !_collapseNavMenu;
            return _collapseNavMenu;
        }

        /// <summary>
        /// Gets the current collapsed state of the navigation menu
        /// </summary>
        /// <returns>True if collapsed, false if expanded</returns>
        public bool GetNavMenuCollapsedState()
        {
            return _collapseNavMenu;
        }

        /// <summary>
        /// Sets the navigation menu's collapsed state
        /// </summary>
        /// <param name="collapsed">True to collapse, false to expand</param>
        public void SetNavMenuCollapsedState(bool collapsed)
        {
            _collapseNavMenu = collapsed;
        }

        /// <summary>
        /// Gets the current year for footer copyright display
        /// </summary>
        /// <returns>Current UTC year</returns>
        public virtual int GetCurrentYear()
        {
            return DateTime.UtcNow.Year;
        }

        /// <summary>
        /// Determines if a navigation link should be highlighted based on the current URI
        /// </summary>
        /// <param name="href">The href of the navigation link</param>
        /// <param name="currentUri">The current page URI</param>
        /// <param name="match">The NavLinkMatch behavior</param>
        /// <returns>True if the link should be highlighted</returns>
        public bool ShouldHighlightNavLink(string href, Uri currentUri, NavLinkMatch match = NavLinkMatch.Prefix)
        {
            if (string.IsNullOrEmpty(href))
                return false;

            if (match == NavLinkMatch.All)
            {
                return currentUri.AbsolutePath == href ||
                       (currentUri.AbsolutePath == "/" && href == "");
            }

            return currentUri.AbsolutePath.StartsWith(href, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets navigation link data for the application
        /// </summary>
        /// <returns>Collection of navigation link information</returns>
        public virtual IEnumerable<NavLinkData> GetNavigationLinks()
        {
            yield return new NavLinkData
            {
                Href = Constants.Routing.HomeRoute,
                Icon = Constants.Layout.TimerNavIcon,
                Title = Constants.Layout.TimerNavLinkTitle,
                Match = NavLinkMatch.All
            };

            yield return new NavLinkData
            {
                Href = Constants.Routing.HistoryRoute,
                Icon = Constants.Layout.HistoryNavIcon,
                Title = Constants.Layout.HistoryNavLinkTitle,
                Match = NavLinkMatch.Prefix
            };

            yield return new NavLinkData
            {
                Href = Constants.Routing.SettingsRoute,
                Icon = Constants.Layout.SettingsNavIcon,
                Title = Constants.Layout.SettingsNavLinkTitle,
                Match = NavLinkMatch.Prefix
            };

            yield return new NavLinkData
            {
                Href = Constants.Routing.AboutRoute,
                Icon = Constants.Layout.AboutNavIcon,
                Title = Constants.Layout.AboutNavLinkTitle,
                Match = NavLinkMatch.Prefix
            };
        }

    }

    /// <summary>
    /// Data model for navigation links
    /// </summary>
    public class NavLinkData
    {
        public string Href { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;
    }
}