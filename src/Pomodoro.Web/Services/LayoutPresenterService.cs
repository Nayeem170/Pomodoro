using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Pomodoro.Web.Services
{
    public class LayoutPresenterService
    {
        private bool _collapseNavMenu = true;

        /// <returns>CSS class name or null if not collapsed</returns>
        public virtual string? GetNavMenuCssClass()
        {
            return _collapseNavMenu ? "collapse" : null;
        }

        /// <returns>New collapsed state</returns>
        public virtual bool ToggleNavMenu()
        {
            _collapseNavMenu = !_collapseNavMenu;
            return _collapseNavMenu;
        }

        /// <returns>True if collapsed, false if expanded</returns>
        public bool GetNavMenuCollapsedState()
        {
            return _collapseNavMenu;
        }

        /// <param name="collapsed">True to collapse, false to expand</param>
        public void SetNavMenuCollapsedState(bool collapsed)
        {
            _collapseNavMenu = collapsed;
        }

        /// <returns>Current UTC year</returns>
        public virtual int GetCurrentYear()
        {
            return DateTime.UtcNow.Year;
        }

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

        /// <returns>Collection of navigation link information</returns>
        public virtual IEnumerable<NavLinkData> GetNavigationLinks()
        {
            yield return new NavLinkData
            {
                Href = Constants.Routing.HomeRoute,
                Icon = Constants.Layout.FocusNavLogoPath,
                Title = Constants.Layout.TimerNavLinkTitle,
                Match = NavLinkMatch.All,
                IsLogoIcon = true
            };

            yield return new NavLinkData
            {
                Href = Constants.Routing.HistoryRoute,
                Icon = Constants.Layout.HistoryNavLogoPath,
                Title = Constants.Layout.HistoryNavLinkTitle,
                Match = NavLinkMatch.Prefix,
                IsLogoIcon = true
            };

            yield return new NavLinkData
            {
                Href = Constants.Routing.SettingsRoute,
                Icon = Constants.Layout.SettingsNavLogoPath,
                Title = Constants.Layout.SettingsNavLinkTitle,
                Match = NavLinkMatch.Prefix,
                IsLogoIcon = true
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

    public class NavLinkData
    {
        public string Href { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;
        public bool IsLogoIcon { get; set; }
    }
}
