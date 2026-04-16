namespace Pomodoro.Web;

/// <summary>
/// Storage and caching constants
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// IndexedDB storage-related constants
    /// </summary>
    public static class Storage
    {
        /// <summary>Store name for timer settings</summary>
        public const string SettingsStore = "settings";

        /// <summary>Store name for daily statistics</summary>
        public const string DailyStatsStore = "dailyStats";

        /// <summary>Store name for tasks</summary>
        public const string TasksStore = "tasks";

        /// <summary>Store name for activity records</summary>
        public const string ActivitiesStore = "activities";

        /// <summary>Store name for app state</summary>
        public const string AppStateStore = "appState";

        /// <summary>Default settings record ID</summary>
        public const string DefaultSettingsId = "default";

        /// <summary>Database name</summary>
        public const string DatabaseName = "PomodoroDB";

        /// <summary>Database version</summary>
        public const int DatabaseVersion = 1;

        /// <summary>Index name for completedAt field</summary>
        public const string CompletedAtIndex = "completedAt";
    }

    /// <summary>
    /// Cache-related constants
    /// </summary>
    public static class Cache
    {
        /// <summary>Maximum number of activities to cache</summary>
        public const int MaxActivityCacheSize = 500;
    }
}
