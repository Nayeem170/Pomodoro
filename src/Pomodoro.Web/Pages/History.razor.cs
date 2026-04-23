using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using System.Threading;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Code-behind for History page
/// Displays activity history with summary cards and timeline
/// </summary>
public class HistoryBase : ComponentBase, IAsyncDisposable
{
    #region Services (Dependency Injection)

    [Inject]
    protected IActivityService ActivityService { get; set; } = default!;
    [Inject]
    protected IStatisticsService StatisticsService { get; set; } = default!;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    protected IInfiniteScrollInterop InfiniteScrollInterop { get; set; } = default!;

    [Inject]
    protected ILogger<HistoryBase> Logger { get; set; } = default!;

    [Inject]
    protected IHistoryStatsService HistoryStatsService { get; set; } = default!;

    [Inject]
    protected HistoryPagePresenterService HistoryPagePresenterService { get; set; } = default!;

    [Inject]
    protected ILocalDateTimeService LocalDateTimeService { get; set; } = default!;

    #endregion

    #region State

    protected bool IsLoading { get; set; } = true;
    protected DateTime SelectedDate { get; set; } = DateTime.Now.Date;
    protected DateTime SelectedWeekStart { get; set; }
    protected HistoryTab ActiveTab { get; set; } = HistoryTab.Daily;
    protected List<ActivityRecord> CurrentActivities { get; set; } = new();
    protected DailyStatsSummary CurrentStats { get; set; } = new();
    protected WeeklyStats? WeeklyStats { get; set; }
    protected Dictionary<DateTime, int> WeeklyFocusMinutes { get; set; } = new();
    protected Dictionary<DateTime, int> WeeklyBreakMinutes { get; set; } = new();
    protected int CurrentSkip { get; set; }
    protected bool HasMoreActivities { get; set; }
    protected bool IsLoadingMore { get; set; }
    protected int PageSize { get; } = 20;

    protected string FormattedSelectedDate { get; set; } = string.Empty;
    protected string FormattedWeekRange { get; set; } = string.Empty;
    protected bool IsSelectedDateToday { get; set; }
    protected bool IsSelectedWeekCurrent { get; set; }

    /// <summary>
    /// Component parameter for testing: Sets initial active tab
    /// </summary>
    [Parameter]
    public HistoryTab InitialActiveTab { get; set; } = HistoryTab.Daily;

    /// <summary>
    /// Component parameter for testing: Sets the initial weekly stats
    /// </summary>
    [Parameter]
    public WeeklyStats? InitialWeeklyStats { get; set; }

    /// <summary>
    /// Component parameter for testing: Sets whether there are more activities to load
    /// </summary>
    [Parameter]
    public bool InitialHasMoreActivities { get; set; } = false;

    /// <summary>
    /// Component parameter for testing: Sets whether more activities are loading
    /// </summary>
    [Parameter]
    public bool InitialIsLoadingMore { get; set; } = false;

    /// <summary>
    /// Component parameter for testing: Sets the initial activities list
    /// </summary>
    [Parameter]
    public List<ActivityRecord> InitialActivities { get; set; } = new();

    /// <summary>
    /// Component parameter for testing: Sets the initial current stats
    /// </summary>
    [Parameter]
    public DailyStatsSummary? InitialCurrentStats { get; set; }

    /// <summary>
    /// Component parameter for testing: Sets the initial selected date
    /// </summary>
    [Parameter]
    public DateTime? InitialSelectedDate { get; set; }

    /// <summary>
    /// Component parameter for testing: Sets the initial selected week start
    /// </summary>
    [Parameter]
    public DateTime? InitialSelectedWeekStart { get; set; }

    // Infinite scroll state
    private DotNetObjectReference<HistoryBase>? _dotNetRef;
    private bool _observerInitialized;
    private bool _isDisposed;
    private bool _isCallbackInProgress;
    private SemaphoreSlim _observerSetupLock = new SemaphoreSlim(1, 1);

    #endregion

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Set protected properties from component parameters for testing purposes
        // This allows tests to control conditional rendering paths
        if (InitialActiveTab != HistoryTab.Daily)
        {
            ActiveTab = InitialActiveTab;
        }

        if (InitialWeeklyStats != null)
        {
            WeeklyStats = InitialWeeklyStats;
        }

        HasMoreActivities = InitialHasMoreActivities;

        IsLoadingMore = InitialIsLoadingMore;

        if (InitialActivities != null && InitialActivities.Count > 0)
        {
            CurrentActivities = InitialActivities;
        }

        if (InitialCurrentStats != null)
        {
            CurrentStats = InitialCurrentStats;
        }

        if (InitialSelectedDate.HasValue)
        {
            SelectedDate = InitialSelectedDate.Value;
        }

        if (InitialSelectedWeekStart.HasValue)
        {
            SelectedWeekStart = InitialSelectedWeekStart.Value;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ActivityService.OnActivityChanged += OnActivityChanged;
            await ActivityService.InitializeAsync();
            var localDate = await LocalDateTimeService.GetLocalDateAsync();
            SelectedDate = localDate;
            SelectedWeekStart = WeekNavigatorBase.GetWeekStart(localDate);
            await LoadDataAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Always recreate DotNetObjectReference if it's null, not just on first render
        // This prevents memory leaks if component is re-rendered after a failed initialization
        if (_dotNetRef == null)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        // Set up intersection observer when conditions are met
        if (ShouldSetupInfiniteScrollObserver())
        {
            // Small delay ensures DOM update cycle completes before observer creation
            await Task.Delay(50);
            await SetupInfiniteScrollObserverAsync(retryCount: 0);
        }
    }

    /// <summary>
    /// Determines if the infinite scroll observer should be set up
    /// </summary>
    /// <returns>True if observer setup should be attempted</returns>
    private bool ShouldSetupInfiniteScrollObserver()
    {
        // Only attempt if:
        // 1. Component is not disposed
        // 2. There are more activities to load
        // 3. Observer is not already initialized
        // 4. DotNet reference is available
        // 5. Currently in Daily view (which has timeline)
        return !_isDisposed &&
               HasMoreActivities &&
               !_observerInitialized &&
               _dotNetRef != null &&
               ActiveTab == HistoryTab.Daily;
    }

    /// <summary>
    /// Sets up the Intersection Observer for infinite scroll
    /// </summary>
    /// <param name="retryCount">Current retry attempt (0-2 for max 3 attempts)</param>
    private async Task SetupInfiniteScrollObserverAsync(int retryCount = 0)
    {
        if (!await CanProceedWithObserverSetupAsync())
        {
            return;
        }

        await ExecuteObserverSetupWithLockAsync(retryCount);
    }

    /// <summary>
    /// Determines if observer setup can proceed
    /// </summary>
    /// <returns>True if setup can proceed, false otherwise</returns>
    private async Task<bool> CanProceedWithObserverSetupAsync()
    {
        return await AcquireObserverLockAsync();
    }

    /// <summary>
    /// Executes the observer setup with proper lock management
    /// </summary>
    /// <param name="retryCount">Current retry attempt</param>
    private async Task ExecuteObserverSetupWithLockAsync(int retryCount)
    {
        try
        {
            var setupResult = await TryCreateObserverAsync(retryCount);

            if (ShouldRetryObserverSetup(setupResult))
            {
                await HandleRetryAsync(setupResult);
                return;
            }
        }
        finally
        {
            ReleaseObserverLockIfHeld();
        }
    }

    /// <summary>
    /// Determines if observer setup should be retried
    /// </summary>
    /// <param name="setupResult">The result of the observer setup attempt</param>
    /// <returns>True if retry is needed, false otherwise</returns>
    private bool ShouldRetryObserverSetup(ObserverSetupResult setupResult)
    {
        return setupResult.ShouldRetry && !_isDisposed && !_observerInitialized;
    }

    /// <summary>
    /// Acquires the observer setup lock
    /// </summary>
    /// <returns>True if lock was acquired, false if another setup is in progress</returns>
    private async Task<bool> AcquireObserverLockAsync()
    {
        // Prevent concurrent initialization attempts using async lock
        if (!await _observerSetupLock.WaitAsync(0))
        {
            Logger.LogDebug("Infinite scroll observer setup already in progress, skipping");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Handles the retry logic for observer setup
    /// </summary>
    /// <param name="setupResult">The result of the observer setup attempt</param>
    private async Task HandleRetryAsync(ObserverSetupResult setupResult)
    {
        // Release lock before retry to allow retry to acquire it
        _observerSetupLock.Release();
        await ExecuteRetryAsync(setupResult.NextRetryCount, setupResult.BackoffDelay);
    }

    /// <summary>
    /// Releases the observer lock if it's still held
    /// </summary>
    private void ReleaseObserverLockIfHeld()
    {
        // Only release if we didn't already release for retry
        if (_observerSetupLock.CurrentCount == 0)
        {
            _observerSetupLock.Release();
        }
    }

    /// <summary>
    /// Attempts to create the infinite scroll observer
    /// </summary>
    /// <param name="retryCount">Current retry attempt</param>
    /// <returns>Setup result indicating success and whether retry is needed</returns>
    private async Task<ObserverSetupResult> TryCreateObserverAsync(int retryCount)
    {
        var result = new ObserverSetupResult();

        try
        {
            if (!await IsIntersectionObserverSupportedAsync())
            {
                return result;
            }

            var success = await CreateObserverWithInteropAsync();

            HandleObserverCreationResult(success, retryCount, result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize infinite scroll observer");
        }

        return result;
    }

    /// <summary>
    /// Checks if Intersection Observer API is supported
    /// </summary>
    /// <returns>True if supported, false otherwise</returns>
    private async Task<bool> IsIntersectionObserverSupportedAsync()
    {
        var supported = await InfiniteScrollInterop.IsSupportedAsync();
        if (!supported)
        {
            Logger.LogWarning("Intersection Observer API not supported");
        }
        return supported;
    }

    /// <summary>
    /// Creates the observer using interop
    /// </summary>
    /// <returns>True if creation was successful, false otherwise</returns>
    private async Task<bool> CreateObserverWithInteropAsync()
    {
        return await InfiniteScrollInterop.CreateObserverAsync(
            Constants.UI.InfiniteScrollSentinelId,
            DotNetObjectReference.Create((object)_dotNetRef!.Value),
            Constants.UI.TimelineScrollContainerId,
            Constants.UI.InfiniteScrollRootMargin,
            Constants.UI.InfiniteScrollTimeoutMs);
    }

    /// <summary>
    /// Handles the result of observer creation
    /// </summary>
    /// <param name="success">Whether observer creation was successful</param>
    /// <param name="retryCount">Current retry attempt</param>
    /// <param name="result">Result object to update</param>
    private void HandleObserverCreationResult(bool success, int retryCount, ObserverSetupResult result)
    {
        if (success)
        {
            _observerInitialized = true;
            Logger.LogDebug("Infinite scroll observer initialized");
        }
        else
        {
            HandleObserverCreationFailure(retryCount, result);
        }
    }

    /// <summary>
    /// Handles failure of observer creation
    /// </summary>
    /// <param name="retryCount">Current retry attempt</param>
    /// <param name="result">Result object to update</param>
    private void HandleObserverCreationFailure(int retryCount, ObserverSetupResult result)
    {
        if (retryCount < 2)
        {
            SetupRetryParameters(retryCount, result);
            Logger.LogDebug("Observer setup failed, retrying in {Delay}ms (attempt {Attempt}/3)",
                result.BackoffDelay, result.NextRetryCount + 1);
        }
        else
        {
            Logger.LogWarning("Infinite scroll observer setup failed after 3 attempts");
        }
    }

    /// <summary>
    /// Sets up retry parameters
    /// </summary>
    /// <param name="retryCount">Current retry attempt</param>
    /// <param name="result">Result object to update</param>
    private void SetupRetryParameters(int retryCount, ObserverSetupResult result)
    {
        result.ShouldRetry = true;
        result.NextRetryCount = retryCount + 1;
        result.BackoffDelay = 100 * (retryCount + 1);
    }

    /// <summary>
    /// Executes retry attempt with proper locking
    /// </summary>
    /// <param name="nextRetryCount">Next retry attempt number</param>
    /// <param name="backoffDelay">Delay before retry in milliseconds</param>
    private async Task ExecuteRetryAsync(int nextRetryCount, int backoffDelay)
    {
        await Task.Delay(backoffDelay);

        // Re-acquire lock for retry attempt
        if (!await _observerSetupLock.WaitAsync(0))
        {
            Logger.LogDebug("Observer retry skipped - another setup is already in progress");
            return;
        }

        try
        {
            // Check again if observer is still not initialized before retrying
            // This prevents race condition where another thread initialized it during delay
            if (!_observerInitialized)
            {
                await SetupInfiniteScrollObserverAsync(nextRetryCount);
            }
        }
        finally
        {
            _observerSetupLock.Release();
        }
    }

    /// <summary>
    /// Result of observer setup attempt
    /// </summary>
    private class ObserverSetupResult
    {
        public bool ShouldRetry { get; set; }
        public int NextRetryCount { get; set; }
        public int BackoffDelay { get; set; }
    }

    /// <summary>
    /// Callback from JavaScript when sentinel element is visible
    /// </summary>
    [JSInvokable]
    public async Task OnSentinelIntersecting()
    {
        if (_isDisposed || _isCallbackInProgress || _dotNetRef == null || IsLoadingMore || !HasMoreActivities)
        {
            return;
        }

        _isCallbackInProgress = true;
        try
        {
            await LoadMoreActivitiesAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load more activities on sentinel intersect");
            // Don't re-throw - JS side already handles cleanup in the catch block
            // Add delay before allowing next callback to prevent rapid retries on errors
            await Task.Delay(1000);
        }
        finally
        {
            _isCallbackInProgress = false;
        }
    }

    private void OnActivityChanged()
    {
        SafeTaskRunner.RunAndForget(async () =>
        {
            await InvokeAsync(async () =>
            {
                await LoadDataAsync();
                StateHasChanged();
            });
        }, Logger, "OnActivityChanged");
    }

    private async Task LoadDataAsync()
    {
        var today = AppState.GetCurrentDayKey();
        var selectedDate = SelectedDate.Date;

        // Reset pagination state when loading new date
        CurrentSkip = 0;

        // Reset observer state when loading new date
        _observerInitialized = false;

        // Load activities for selected date (Daily view) - initial page only using async pagination
        CurrentActivities = await ActivityService.GetActivitiesPagedAsync(
            selectedDate, selectedDate.AddDays(1), 0, PageSize);

        // Update skip to reflect loaded count
        CurrentSkip = CurrentActivities.Count;

        // Calculate stats for selected date (use all activities for accurate stats)
        var allActivitiesForDate = ActivityService.GetActivitiesForDate(selectedDate);
        CurrentStats = CalculateStats(allActivitiesForDate);

        // Check if there are more activities to load
        var totalCount = await ActivityService.GetActivityCountAsync(selectedDate, selectedDate.AddDays(1));
        HasMoreActivities = CurrentSkip < totalCount;

        // Log for debugging
        Logger.LogDebug(Constants.Messages.LogHistoryLoadDataFormat,
            selectedDate, today, selectedDate == today);
        Logger.LogDebug(Constants.Messages.LogHistoryStatsFormat,
            CurrentActivities.Count, CurrentStats.PomodoroCount, CurrentStats.FocusMinutes);

        // Use SelectedWeekStart for weekly data (independent from daily view)
        var weekStart = SelectedWeekStart;
        var weekEnd = weekStart.AddDays(6); // Friday

        // Load weekly data for chart (Saturday to Friday week)
        WeeklyFocusMinutes = ActivityService.GetDailyFocusMinutes(weekStart, weekEnd);
        WeeklyBreakMinutes = ActivityService.GetDailyBreakMinutes(weekStart, weekEnd);

        // Load weekly statistics
        WeeklyStats = await StatisticsService.GetWeeklyStatsAsync(weekStart);

        UpdateFormattedDate();
        UpdateFormattedWeekRange();

        // Observer will be set up in OnAfterRenderAsync after DOM is fully updated
        // This ensures sentinel element exists before observer is created
    }

    private DailyStatsSummary CalculateStats(List<ActivityRecord> activities)
    {
        return HistoryStatsService.CalculateStats(activities);
    }

    /// <summary>
    /// Format focus time for display
    /// </summary>
    protected string FormatFocusTime(int minutes)
    {
        return HistoryPagePresenterService.FormatFocusTime(minutes);
    }

    #endregion

    #region Event Handlers

    protected async Task GoToPreviousDay()
    {
        await HandleDateChanged(SelectedDate.AddDays(-1));
    }

    protected async Task GoToNextDay()
    {
        await HandleDateChanged(SelectedDate.AddDays(1));
    }

    protected async Task GoToToday()
    {
        await HandleDateChanged(DateTime.Now.Date);
    }

    protected async Task GoToPreviousWeek()
    {
        await HandleWeekChanged(SelectedWeekStart.AddDays(-7));
    }

    protected async Task GoToNextWeek()
    {
        await HandleWeekChanged(SelectedWeekStart.AddDays(7));
    }

    protected async Task GoToThisWeek()
    {
        var thisWeekStart = WeekNavigatorBase.GetWeekStart(DateTime.Now.Date);
        await HandleWeekChanged(thisWeekStart);
    }

    private void UpdateFormattedDate()
    {
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);

        if (SelectedDate.Date == today)
        {
            FormattedSelectedDate = "Today, " + SelectedDate.ToString("MMM d");
            IsSelectedDateToday = true;
        }
        else if (SelectedDate.Date == yesterday)
        {
            FormattedSelectedDate = "Yesterday, " + SelectedDate.ToString("MMM d");
            IsSelectedDateToday = false;
        }
        else
        {
            FormattedSelectedDate = SelectedDate.ToString("MMM d");
            IsSelectedDateToday = false;
        }
    }

    private void UpdateFormattedWeekRange()
    {
        var weekEnd = SelectedWeekStart.AddDays(6);
        FormattedWeekRange = $"{SelectedWeekStart:MMM d} – {weekEnd:MMM d}";

        var thisWeekStart = WeekNavigatorBase.GetWeekStart(DateTime.Now.Date);
        IsSelectedWeekCurrent = SelectedWeekStart.Date == thisWeekStart.Date;
    }

    protected async Task HandleDateChanged(DateTime newDate)
    {
        SelectedDate = newDate;
        CurrentSkip = 0;

        // Explicitly destroy observer before resetting state
        if (_observerInitialized)
        {
            try
            {
                await InfiniteScrollInterop.DestroyObserverAsync(Constants.UI.InfiniteScrollSentinelId);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to destroy observer on date change");
            }
        }

        _observerInitialized = false;
        await LoadDataAsync();
        StateHasChanged();
    }

    protected async Task HandleTabChanged(HistoryTab newTab)
    {
        ActiveTab = newTab;

        // Clean up observer when leaving Daily view
        if (newTab != HistoryTab.Daily && _observerInitialized)
        {
            try
            {
                await InfiniteScrollInterop.DestroyObserverAsync(Constants.UI.InfiniteScrollSentinelId);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to destroy observer on tab change");
            }
            _observerInitialized = false;
        }

        StateHasChanged();
    }

    protected async Task HandleWeekChanged(DateTime newWeekStart)
    {
        SelectedWeekStart = newWeekStart;
        await LoadDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Loads more activities with lazy loading
    /// </summary>
    protected async Task LoadMoreActivitiesAsync()
    {
        if (IsLoadingMore || !HasMoreActivities) return;

        try
        {
            IsLoadingMore = true;
            StateHasChanged();

            var newActivities = await ActivityService.GetActivitiesPagedAsync(
                SelectedDate,
                SelectedDate.AddDays(1),
                CurrentSkip,
                PageSize);

            CurrentActivities.AddRange(newActivities);
            CurrentSkip += newActivities.Count;

            // Check if there are more activities
            var totalCount = await ActivityService.GetActivityCountAsync(SelectedDate, SelectedDate.AddDays(1));
            HasMoreActivities = CurrentSkip < totalCount;

            // Note: Observer doesn't need re-initialization here because the sentinel element
            // remains in the DOM. As new activities are added above it, it moves further down
            // the page and will trigger again when scrolled into view.

            // After loading more items, the sentinel moves down the page.
            // The observer will automatically detect when it comes back into view.
        }
        finally
        {
            IsLoadingMore = false;
            StateHasChanged();
        }
    }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _observerSetupLock?.Dispose();

        ActivityService.OnActivityChanged -= OnActivityChanged;

        // Clean up JavaScript observer
        if (_dotNetRef != null)
        {
            try
            {
                await InfiniteScrollInterop.DestroyObserverAsync(Constants.UI.InfiniteScrollSentinelId);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to destroy observer by ID, attempting to destroy all");
                try
                {
                    await InfiniteScrollInterop.DestroyAllObserversAsync();
                }
                catch (Exception fallbackEx)
                {
                    Logger.LogWarning(fallbackEx, "Failed to destroy all observers during disposal");
                }
            }

            _dotNetRef.Dispose();
        }
    }

    #endregion
}
