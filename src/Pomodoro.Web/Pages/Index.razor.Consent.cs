using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Pages;

public partial class IndexBase
{
    #region Consent Actions

    public async Task HandleConsentOptionSelect(ConsentOption option)
    {
        try
        {
            if (option.IsResume)
            {
                await TimerService.ResumeInterruptedPomodoroAsync();
                ConsentService.HideConsentModal();
            }
            else
            {
                await ConsentService.SelectOptionAsync(option.SessionType);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorSelectingConsentOption}: {ex.Message}";
        }
    }

    #endregion
}
