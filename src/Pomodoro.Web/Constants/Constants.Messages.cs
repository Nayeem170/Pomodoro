namespace Pomodoro.Web;

/// <summary>
/// UI message strings and log messages
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// UI message strings for localization support
    /// </summary>
    public static class Messages
    {
        // Consent Modal Titles
        public const string PomodoroCompleteTitle = "Pomodoro Complete!";
        public const string BreakCompleteTitle = "Break Complete!";
        public const string LongBreakCompleteTitle = "Long Break Complete!";
        public const string SessionCompleteTitle = "Session Complete!";

        // Consent Modal Messages
        public const string PomodoroCompleteMessage = "Great work! What would you like to do next?";
        public const string BreakCompleteMessage = "Ready to get back to work?";
        public const string SessionCompleteMessage = "What would you like to do next?";

        // Log Messages
        public const string ServiceInitializationFailed = "Service initialization failed. The app will continue with limited functionality.";
        public const string TimerStartFailed = "Timer start failed, retrying after delay";
        public const string TimerStartFailedAfterRetry = "Timer start failed after retry";
        public const string TimerStopFailed = "Failed to stop JS timer";
        public const string TimerCompletionError = "Timer completion error";
        public const string TimerHandleCompleteError = "Error handling timer complete";
        public const string TimerCompletionHandlerError = "Error in timer completion handler";
        public const string AudioUnlockFailed = "Audio context unlock failed - this is expected on some browsers";

        // Repository log messages
        public const string LogFailedToSaveActivity = "Failed to save activity {ActivityId}";
        public const string LogFailedToSaveTask = "Failed to save task {TaskId}";

        // PipTimer Log Messages
        public const string LogPipInitialized = "Initialized. Supported: {IsSupported}";
        public const string LogPipInitializationFailed = "Initialization failed";
        public const string LogPipOpenFailed = "Failed to open PiP";
        public const string LogPipCloseFailed = "Failed to close PiP";
        public const string LogPipUpdateFailed = "Failed to update PiP";
        public const string LogPipToggleTimerError = "Error in OnPipToggleTimer";
        public const string LogPipResetTimerError = "Error in OnPipResetTimer";
        public const string LogPipSwitchSessionError = "Error in OnPipSwitchSession";

        public const string LogHandlerErrorFormat = "Error in {HandlerName}";
        public const string LogTimerCompleteError = "Error in OnTimerComplete";
        public const string ErrorInOnTimerComplete = "Error in OnTimerComplete";

        // ConsentService Log Messages
        public const string LogHandleTimerCompleteError = "HandleTimerComplete error";
        public const string LogConsentHandleCompleteError = "Error in HandleTimerComplete";
        public const string LogCannotStartPomodoroNoTask = "Cannot start pomodoro: no task selected";
        public const string LogCountdownError = "Error in countdown";
        public const string LogFailedToResolveTimerService = "Failed to resolve ITimerService from service provider";
        public const string LogFailedToResolveTaskService = "Failed to resolve ITaskService from service provider";
        public const string LogFailedToResolveNotificationService = "Failed to resolve INotificationService from service provider";
        public const string LogErrorCaughtByBoundary = "Unhandled error caught by ErrorBoundary";

        // History page log messages
        public const string LogHistoryLoadDataFormat = "LoadDataAsync: SelectedDate={SelectedDate}, Today={Today}, IsToday={IsToday}";
        public const string LogHistoryStatsFormat = "Activities count: {Count}, Stats: {PomodoroCount} pomodoros, {FocusMinutes} min";
        public const string ErrorUpdatingTimeDistributionChart = "Error updating time distribution chart";
        public const string ErrorLoadingMoreActivities = "Error loading more activities";

        // ActivityService log messages
        public const string LogActivitiesLoadedFormat = "Loaded {Count} activities from IndexedDB (cache limit: {MaxCacheSize})";
        public const string LogActivityDebugFormat = "Activity: {Type}, CompletedAt={CompletedAt}, TaskName={TaskName}";
        public const string LogGetTodayActivitiesFormat = "GetTodayActivities: Today={Today}, CacheCount={CacheCount}, ResultCount={ResultCount}";
        public const string LogAddedActivityFormat = "Added activity: {Type}, CompletedAt={CompletedAt}, CacheCount={CacheCount}";

        // IndexedDbService Log Messages
        public const string LogInitializedSuccessfully = "Initialized successfully";
        public const string LogFailedToInitialize = "Failed to initialize";
        public const string LogFailedToInitializeJsConstants = "Failed to initialize JS constants";
        public const string LogJsConstantsInitializedFormat = "JS constants initialized with pomodoro={Pomodoro}min, shortBreak={ShortBreak}min, longBreak={LongBreak}min";
        public const string LogJsonDeserializationFailed = "JSON deserialization failed for {StoreName}/{Key}";
        public const string LogDataCorruptionDetected = "Data corruption detected in {StoreName}";
        public const string LogErrorGettingItem = "Error getting item from {StoreName} with key {Key}";
        public const string LogFailedToGetItem = "Failed to get item from {0}: {1}";
        public const string LogErrorGettingAllItems = "Error getting all items from {StoreName}";
        public const string LogFailedToGetAllItems = "Failed to get all items from {0}: {1}";
        public const string LogErrorQueryingIndex = "Error querying index {IndexName}";
        public const string LogErrorQueryingDateRange = "Error querying date range";
        public const string LogErrorPuttingItem = "Error putting item to {StoreName}";
        public const string LogErrorPuttingAllItems = "Error putting all items to {StoreName}";
        public const string LogErrorDeletingItem = "Error deleting item from {StoreName}";
        public const string LogErrorClearingStore = "Error clearing store {StoreName}";
        public const string LogErrorGettingCount = "Error getting count for {StoreName}";
        public const string IndexedDbNotInitialized = "IndexedDbService has not been initialized. Call InitializeAsync first.";
        public const string ErrorAddingTask = "Error adding task";
        public const string ErrorSelectingTask = "Error selecting task";
        public const string ErrorCompletingTask = "Error completing task";
        public const string ErrorDeletingTask = "Error deleting task";
        public const string ErrorUncompletingTask = "Error uncompleting task";
        public const string ErrorUpdatingTask = "Error updating task";
        public const string ErrorStartingTimer = "Error starting timer";
        public const string ErrorPausingTimer = "Error pausing timer";
        public const string ErrorResumingTimer = "Error resuming timer";
        public const string ErrorResettingTimer = "Error resetting timer";
        public const string ErrorSwitchingSession = "Error switching session";
        public const string ErrorTogglingFloatingTimer = "Error toggling floating timer";
        public const string PipPopupBlocked = "Pop-up blocked. Please allow pop-ups and redirects for this site to use the floating timer.";
        public const string ErrorInitializing = "Error initializing";
        public const string ErrorSelectingConsentOption = "Error selecting consent option";
        public const string ErrorCheckingPendingNotificationAction = "Error checking pending notification action";
        public const string ErrorInUpdateState = "Error in UpdateState";
        public const string ErrorInDispose = "Error in Dispose";
        public const string SelectTaskBeforePomodoro = "Please select a task before starting a pomodoro.";

        // StatisticsService Log Messages
        public const string LogWeeklyStatsError = "Error calculating weekly stats for week starting {WeekStartDate}";

        // NotificationService Log Messages
        public const string ErrorInitializingNotificationService = "Failed to initialize notification service";
        public const string ErrorRequestingNotificationPermission = "Failed to request notification permission";
        public const string ErrorShowingNotification = "Failed to show notification";
        public const string ErrorPlayingTimerCompleteSound = "Failed to play timer complete sound";
        public const string ErrorPlayingBreakCompleteSound = "Failed to play break complete sound";

        // Error Message Formatting
        public const string ErrorSeparator = ": ";
        public const string ErrorFormat = "{0}" + ErrorSeparator + "{1}";

        // Notification Messages
        public const string PomodoroNotificationTitle = "Pomodoro Complete!";
        public const string PomodoroNotificationMessage = "Time for a break. Choose your next session.";
        public const string BreakNotificationTitle = "Break Complete!";
        public const string BreakNotificationMessage = "Ready to focus again? Start your next Pomodoro.";

        // Export/Import Log Messages
        public const string LogExportJsonFormat = "Exported {ActivityCount} activities and {TaskCount} tasks to JSON";
        public const string LogExportJsonFailed = "Failed to export data to JSON";
        public const string LogImportJsonInvalid = "Invalid JSON data format";
        public const string LogImportJsonParseFailed = "Failed to parse JSON data";
        public const string LogImportJsonFailed = "Failed to import data from JSON";
        public const string LogImportJsonSuccess = "Imported {Count} records from backup dated {ExportDate}";
        public const string LogClearDataSuccess = "Cleared {ActivityCount} activities and {TaskCount} tasks";
        public const string LogClearDataFailed = "Failed to clear all data";

        // Import Error Messages (User-facing)
        public const string ImportErrorEmptyFile = "The import file is empty. Please select a valid backup file.";
        public const string ImportErrorInvalidJson = "Invalid JSON format. Please select a valid backup file.";
        public const string ImportErrorInvalidFormat = "The file format is not recognized. Please select a backup file exported from this app.";
        public const string ImportErrorInvalidVersion = "The backup file version is not supported. Please export a new backup from the latest version of the app.";
        public const string ImportErrorFailed = "An error occurred while importing. Please try again.";
    }
}
