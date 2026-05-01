using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components.Timer;

public class TimerDisplayBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject]
    protected ITimerService TimerService { get; set; } = default!;

    [Inject]
    protected ITimerEventPublisher TimerEventPublisher { get; set; } = default!;

    [Inject]
    protected ILogger<TimerDisplayBase> Logger { get; set; } = default!;

    #endregion

    #region Parameters (for initial/override values only)

    [Parameter]
    public TimeSpan? RemainingTime { get; set; }

    [Parameter]
    public SessionType? SessionType { get; set; }

    [Parameter]
    public bool? IsRunning { get; set; }

    #endregion

    #region Properties - Always read from service for real-time updates

    protected TimeSpan CurrentRemainingTime => TimerService.RemainingTime;
    protected SessionType CurrentSessionType => TimerService.CurrentSessionType;
    protected bool CurrentIsRunning => TimerService.IsRunning;

    private const double Circumference = 2 * Math.PI * 81; // ~508.94

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        TimerEventPublisher.OnTick += OnTimerTick;
        TimerEventPublisher.OnTimerStateChanged += OnTimerStateChanged;
    }

    internal virtual void UpdateDisplay()
    {
        StateHasChanged();
    }

    internal virtual void HandleStateChangeError() { }

    internal async void OnTimerTick()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnTimerTick");
        }
    }

    internal async void OnTimerStateChanged()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnTimerStateChanged");
        }
    }

    public void Dispose()
    {
        TimerEventPublisher.OnTick -= OnTimerTick;
        TimerEventPublisher.OnTimerStateChanged -= OnTimerStateChanged;
    }

    #endregion

    #region Business Logic Methods

    protected string FormatTime(TimeSpan time)
    {
        return string.Format(Constants.TimeFormats.TimerFormat, (int)time.TotalMinutes, time.Seconds);
    }

    internal string GetSessionTypeLabel()
    {
        var sessionType = CurrentSessionType;
        if (sessionType == Models.SessionType.Pomodoro) return "FOCUSING";
        if (sessionType == Models.SessionType.ShortBreak) return "SHORT BREAK";
        return "LONG BREAK";
    }

    internal string GetRingSessionClass()
    {
        if (CurrentSessionType == Models.SessionType.Pomodoro) return "";
        if (CurrentSessionType == Models.SessionType.ShortBreak) return "short-break";
        return "long-break";
    }

    internal string GetDashOffset()
    {
        var settings = TimerService.Settings;
        if (settings == null) return "0";

        var totalSeconds = settings.GetDurationSeconds(CurrentSessionType);
        if (totalSeconds <= 0) return "0";

        var remainingSeconds = CurrentRemainingTime.TotalSeconds;
        var progress = remainingSeconds / totalSeconds;
        var offset = Circumference * (1 - progress);
        return offset.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
    }

    #endregion
}
