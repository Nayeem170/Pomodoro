using Microsoft.JSInterop;

namespace Pomodoro.Web.Services;

public interface IJsTimerInterop
{
    Task StartAsync(DotNetObjectReference<object> dotNetRef);
    Task StopAsync();
}
