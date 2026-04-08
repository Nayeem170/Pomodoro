using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for consent modal operations
/// </summary>
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

/// <summary>
/// Represents an option in the consent modal
/// </summary>
public class ConsentOption
{
    public SessionType SessionType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
