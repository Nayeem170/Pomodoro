namespace Pomodoro.Web;

/// <summary>
/// JavaScript interop function names and related constants
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// JavaScript interop function names
    /// </summary>
    public static class JsFunctions
    {
        public const string TimerStart = "timerFunctions.start";
        public const string TimerStop = "timerFunctions.stop";
        public const string GetUrlParameter = "getUrlParameter";
        public const string RemoveUrlParameter = "removeUrlParameter";
    }

    /// <summary>
    /// JSInvokable method names
    /// </summary>
    public static class JsInvokableMethods
    {
        public const string OnTimerTick = "OnTimerTickJs";
        public const string OnPipToggleTimer = "OnPipToggleTimer";
        public const string OnPipResetTimer = "OnPipResetTimer";
        public const string OnPipSwitchSession = "OnPipSwitchSession";
        public const string OnPipClosed = "OnPipClosed";
        public const string OnNotificationActionClick = "OnNotificationActionClick";
    }

    /// <summary>
    /// IndexedDB JavaScript function names
    /// </summary>
    public static class IndexedDbJsFunctions
    {
        public const string InitDatabase = "indexedDbInterop.initDatabase";
        public const string GetItem = "indexedDbInterop.getItem";
        public const string GetAllItems = "indexedDbInterop.getAllItems";
        public const string GetItemsByIndex = "indexedDbInterop.getItemsByIndex";
        public const string GetItemsByDateRange = "indexedDbInterop.getItemsByDateRange";
        public const string PutItem = "indexedDbInterop.putItem";
        public const string PutAllItems = "indexedDbInterop.putAllItems";
        public const string DeleteItem = "indexedDbInterop.deleteItem";
        public const string ClearStore = "indexedDbInterop.clearStore";
        public const string GetCount = "indexedDbInterop.getCount";
        public const string PomodoroConstantsInitialize = "pomodoroConstants.initialize";
    }

    /// <summary>
    /// PiP JavaScript function names
    /// </summary>
    public static class PipJsFunctions
    {
        public const string IsSupported = "pipTimer.isSupported";
        public const string RegisterDotNetRef = "pipTimer.registerDotNetRef";
        public const string UnregisterDotNetRef = "pipTimer.unregisterDotNetRef";
        public const string Open = "pipTimer.open";
        public const string Close = "pipTimer.close";
        public const string Update = "pipTimer.update";
    }

    /// <summary>
    /// Notification JavaScript function names
    /// </summary>
    public static class NotificationJsFunctions
    {
        public const string RegisterDotNetRef = "notificationFunctions.registerDotNetRef";
        public const string UnregisterDotNetRef = "notificationFunctions.unregisterDotNetRef";
        public const string RequestPermission = "notificationFunctions.requestNotificationPermission";
        public const string ShowNotification = "notificationFunctions.showNotification";
        public const string PlayTimerCompleteSound = "notificationFunctions.playTimerCompleteSound";
        public const string PlayBreakCompleteSound = "notificationFunctions.playBreakCompleteSound";
        public const string UnlockAudio = "notificationFunctions.unlockAudio";
    }

    /// <summary>
    /// Local date/time JavaScript function names
    /// </summary>
    public static class LocalDateTimeJsFunctions
    {
        public const string GetLocalDate = "localDateTime.getLocalDate";
        public const string GetLocalDateTime = "localDateTime.getLocalDateTime";
        public const string GetTimezoneOffset = "localDateTime.getTimezoneOffset";
    }

    /// <summary>
    /// Keyboard shortcut JavaScript function names
    /// </summary>
    public static class KeyboardShortcutJsFunctions
    {
        public const string Initialize = "keyboardShortcuts.initialize";
        public const string Dispose = "keyboardShortcuts.dispose";
    }

    /// <summary>
    /// Infinite scroll JavaScript function names
    /// </summary>
    public static class InfiniteScrollJsFunctions
    {
        public const string IsSupported = "infiniteScroll.isSupported";
        public const string CreateObserver = "infiniteScroll.createObserver";
        public const string DestroyObserver = "infiniteScroll.destroyObserver";
        public const string DestroyAllObservers = "infiniteScroll.destroyAllObservers";
    }

    /// <summary>
    /// Notification permission values
    /// </summary>
    public static class NotificationPermissions
    {
        public const string Granted = "granted";
    }

    /// <summary>
    /// URL parameter names
    /// </summary>
    public static class UrlParameters
    {
        public const string NotificationAction = "notification_action";
    }


}
