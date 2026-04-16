using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public class JsTimerInterop : IJsTimerInterop
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<JsTimerInterop> _logger;

    public JsTimerInterop(IJSRuntime jsRuntime, ILogger<JsTimerInterop> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task StartAsync(DotNetObjectReference<object> dotNetRef)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.UnlockAudio);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, Constants.Messages.AudioUnlockFailed);
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.JsFunctions.TimerStart, dotNetRef);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.Messages.TimerStartFailed);

            try
            {
                await Task.Delay(100);
                await _jsRuntime.InvokeVoidAsync(Constants.JsFunctions.TimerStart, dotNetRef);
            }
            catch (Exception retryEx)
            {
                _logger.LogError(retryEx, Constants.Messages.TimerStartFailedAfterRetry);
            }
        }
    }

    public async Task StopAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.JsFunctions.TimerStop);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.Messages.TimerStopFailed);
        }
    }
}
