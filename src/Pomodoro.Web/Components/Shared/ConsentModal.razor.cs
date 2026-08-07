using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components.Shared;

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
    public EventCallback<ConsentOption> OnOptionSelected { get; set; }

    #endregion

    #region Constants

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

    protected double GetProgressPercentage()
    {
        if (_initialCountdownSeconds <= 0)
            return 0;

        return ((double)CountdownSeconds / _initialCountdownSeconds) * Constants.UI.PercentageMultiplier;
    }

    protected async Task HandleOptionSelect(ConsentOption option)
    {
        await OnOptionSelected.InvokeAsync(option);
    }

    protected RenderFragment RenderOptions => builder =>
    {
        int seq = 0;
        foreach (var option in Options)
        {
            var cssClass = $"btn-option {(option.IsDefault ? "default" : "")}";
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", cssClass);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, _ => HandleOptionSelect(option)));

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "option-label");
            builder.AddContent(seq++, option.Label);
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "option-duration");
            builder.AddContent(seq++, option.Duration);
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "option-chevron");
            builder.AddContent(seq++, "\u203A");
            builder.CloseElement();

            builder.CloseElement();
        }
    };

    #endregion
}
