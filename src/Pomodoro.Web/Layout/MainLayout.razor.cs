using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Layout;

public partial class MainLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected ICloudSyncService CloudSyncService { get; set; } = default!;

    [Inject]
    protected IKeyboardShortcutService KeyboardShortcutService { get; set; } = default!;

    private DotNetObjectReference<MainLayoutBase>? _dotNetRef;
    private bool _isDisposed;

    internal bool IsKeyboardHelpVisible { get; set; }

    protected void ShowKeyboardHelp()
    {
        IsKeyboardHelpVisible = true;
    }

    protected void HideKeyboardHelp()
    {
        IsKeyboardHelpVisible = false;
    }

    protected override void OnInitialized()
    {
        KeyboardShortcutService.RegisterShortcut(Constants.KeyboardShortcuts.HelpKey, () =>
        {
            IsKeyboardHelpVisible = true;
            StateHasChanged();
        }, Constants.KeyboardShortcuts.HelpDescription);

        KeyboardShortcutService.RegisterShortcut(Constants.KeyboardShortcuts.EscapeKey, () =>
        {
            if (IsKeyboardHelpVisible)
            {
                IsKeyboardHelpVisible = false;
                StateHasChanged();
            }
        }, Constants.KeyboardShortcuts.CloseHelpDescription);
    }

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

            await CloudSyncService.InitializeAsync();
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
        KeyboardShortcutService.UnregisterShortcut(Constants.KeyboardShortcuts.HelpKey);
        KeyboardShortcutService.UnregisterShortcut(Constants.KeyboardShortcuts.EscapeKey);
        _dotNetRef?.Dispose();
        _ = JSRuntime.InvokeVoidAsync("swipeNavigation.dispose");
    }
}
