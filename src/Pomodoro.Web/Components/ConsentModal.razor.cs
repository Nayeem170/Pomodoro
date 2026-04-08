using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components;

/// <summary>
/// Code-behind for ConsentModal component
/// Separates business logic from view
/// </summary>
public class ConsentModalBase : ComponentBase
{
    #region Parameters (Model)
    
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public SessionType CompletedSessionType { get; set; }

    [Parameter]
    public int CountdownSeconds { get; set; }

    [Parameter]
    public List<ConsentOption> Options { get; set; } = new();

    [Parameter]
    public EventCallback<SessionType> OnOptionSelected { get; set; }
    
    #endregion

    #region Constants
    
    /// <summary>Maximum countdown seconds for the consent modal (used for progress bar calculation)</summary>
    private int _initialCountdownSeconds;
    
    #endregion

    #region Lifecycle Methods
    
    protected override void OnParametersSet()
    {
        // Track the initial countdown value for progress bar calculation
        // Only update if the countdown was reset (back to a higher value)
        if (CountdownSeconds > _initialCountdownSeconds)
        {
            _initialCountdownSeconds = CountdownSeconds;
        }
        
        // If countdown is 0, reset for next time
        if (CountdownSeconds <= 0)
        {
            _initialCountdownSeconds = 0;
        }
    }
    
    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Gets the icon for the completed session type
    /// </summary>
    protected string GetIcon()
    {
        return CompletedSessionType switch
        {
            SessionType.Pomodoro => Constants.SessionTypes.PomodoroEmoji,
            SessionType.ShortBreak => Constants.SessionTypes.ShortBreakEmoji,
            SessionType.LongBreak => Constants.SessionTypes.LongBreakEmoji,
            _ => Constants.SessionTypes.PomodoroEmoji
        };
    }

    /// <summary>
    /// Gets the title for the completed session type
    /// </summary>
    protected string GetTitle()
    {
        return CompletedSessionType switch
        {
            SessionType.Pomodoro => Constants.Messages.PomodoroCompleteTitle,
            SessionType.ShortBreak => Constants.Messages.BreakCompleteTitle,
            SessionType.LongBreak => Constants.Messages.LongBreakCompleteTitle,
            _ => Constants.Messages.SessionCompleteTitle
        };
    }

    /// <summary>
    /// Gets the message for the completed session type
    /// </summary>
    protected string GetMessage()
    {
        return CompletedSessionType switch
        {
            SessionType.Pomodoro => Constants.Messages.PomodoroCompleteMessage,
            SessionType.ShortBreak => Constants.Messages.BreakCompleteMessage,
            SessionType.LongBreak => Constants.Messages.BreakCompleteMessage,
            _ => Constants.Messages.SessionCompleteMessage
        };
    }

    /// <summary>
    /// Calculates the progress bar percentage
    /// </summary>
    protected double GetProgressPercentage()
    {
        if (_initialCountdownSeconds <= 0)
            return 0;
        
        return ((double)CountdownSeconds / _initialCountdownSeconds) * Constants.UI.PercentageMultiplier;
    }

    /// <summary>
    /// Handles option selection
    /// </summary>
    protected async Task HandleOptionSelect(SessionType sessionType)
    {
        await OnOptionSelected.InvokeAsync(sessionType);
    }

    protected RenderFragment RenderOptions => builder =>
    {
        int seq = 0;
        foreach (var option in Options)
        {
            var cssClass = $"btn btn-option {(option.IsDefault ? "default" : "")}";
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", cssClass);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, _ => HandleOptionSelect(option.SessionType)));

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "option-label");
            builder.AddContent(seq++, option.Label);
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "option-duration");
            builder.AddContent(seq++, option.Duration);
            builder.CloseElement();

            builder.CloseElement();
        }
    };
    
    #endregion
}
