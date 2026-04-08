using Pomodoro.Web.Models;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Consent actions partial for Index page
/// Contains consent modal-related event handlers
/// </summary>
public partial class IndexBase
{
    #region Consent Actions

    /// <summary>
    /// Handles selecting a consent option after timer completion
    /// </summary>
    public async Task HandleConsentOptionSelect(SessionType sessionType)
    {
        try
        {
            await ConsentService.SelectOptionAsync(sessionType);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorSelectingConsentOption}: {ex.Message}";
        }
    }

    #endregion
}
