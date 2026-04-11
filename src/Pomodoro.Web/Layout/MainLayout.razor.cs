using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Pomodoro.Web.Layout;

public partial class MainLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    private DotNetObjectReference<MainLayoutBase>? _dotNetRef;
    private bool _isDisposed;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            var routes = new[]
            {
                Constants.Routing.HomeRoute,
                Constants.Routing.HistoryRoute,
                Constants.Routing.SettingsRoute,
                Constants.Routing.AboutRoute
            };
            await JSRuntime.InvokeVoidAsync("swipeNavigation.init", _dotNetRef, routes);
        }
    }

    [JSInvokable]
    public void NavigateTo(string path)
    {
        NavigationManager.NavigateTo(path);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _dotNetRef?.Dispose();
        _ = JSRuntime.InvokeVoidAsync("swipeNavigation.dispose");
    }
}
