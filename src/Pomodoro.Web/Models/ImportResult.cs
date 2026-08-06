namespace Pomodoro.Web.Models;

public class ImportResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public int ActivitiesImported { get; set; }

    public int ActivitiesSkipped { get; set; }

    public int TasksImported { get; set; }

    public int TasksSkipped { get; set; }

    public bool SettingsImported { get; set; }

    public int TotalImported => ActivitiesImported + TasksImported;

    public int TotalSkipped => ActivitiesSkipped + TasksSkipped;

    public static ImportResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };

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
