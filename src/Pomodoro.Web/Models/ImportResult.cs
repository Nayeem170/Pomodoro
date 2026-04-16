namespace Pomodoro.Web.Models;

/// <summary>
/// Represents the result of an import operation with detailed statistics
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Indicates whether the import operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the import failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of activities that were successfully imported
    /// </summary>
    public int ActivitiesImported { get; set; }

    /// <summary>
    /// Number of activities that were skipped due to duplicates
    /// </summary>
    public int ActivitiesSkipped { get; set; }

    /// <summary>
    /// Number of tasks that were successfully imported
    /// </summary>
    public int TasksImported { get; set; }

    /// <summary>
    /// Number of tasks that were skipped due to duplicates
    /// </summary>
    public int TasksSkipped { get; set; }

    /// <summary>
    /// Indicates whether settings were imported
    /// </summary>
    public bool SettingsImported { get; set; }

    /// <summary>
    /// Total number of records imported (activities + tasks)
    /// </summary>
    public int TotalImported => ActivitiesImported + TasksImported;

    /// <summary>
    /// Total number of records skipped due to duplicates
    /// </summary>
    public int TotalSkipped => ActivitiesSkipped + TasksSkipped;

    /// <summary>
    /// Creates a failed import result with an error message
    /// </summary>
    public static ImportResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };

    /// <summary>
    /// Creates a successful import result
    /// </summary>
    public static ImportResult Succeeded(int activitiesImported, int activitiesSkipped,
        int tasksImported, int tasksSkipped, bool settingsImported) => new()
        {
            Success = true,
            ActivitiesImported = activitiesImported,
            ActivitiesSkipped = activitiesSkipped,
            TasksImported = tasksImported,
            TasksSkipped = tasksSkipped,
            SettingsImported = settingsImported
        };
}
