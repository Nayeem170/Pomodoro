using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IConsentService
{
    event Action? OnConsentRequired;
    event Action? OnCountdownTick;
    event Action? OnConsentHandled;

    bool IsModalVisible { get; }
    SessionType CompletedSessionType { get; }
    int CountdownSeconds { get; }
    List<ConsentOption> AvailableOptions { get; }

    void Initialize();
    void ShowConsentModal(SessionType completedSessionType);
    void HideConsentModal();
    void RefreshOptions();
    Task SelectOptionAsync(SessionType nextSessionType);
    Task HandleTimeoutAsync();
}

public class ConsentOption
{
    public SessionType SessionType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsResume { get; set; }
}
