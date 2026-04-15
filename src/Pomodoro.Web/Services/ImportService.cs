using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class ImportService : IImportService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        IActivityRepository activityRepository,
        ITaskRepository taskRepository,
        ISettingsRepository settingsRepository,
        ILogger<ImportService> logger)
    {
        _activityRepository = activityRepository;
        _taskRepository = taskRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<ImportResult> ImportFromJsonAsync(string jsonData)
    {
        try
        {
            var validationResult = ValidateJsonInput(jsonData);
            if (!validationResult.IsValid)
            {
                return validationResult.Result;
            }

            var parseResult = await ParseJsonDataAsync(jsonData);
            if (!parseResult.IsValid)
            {
                return parseResult.Result;
            }

            var importData = parseResult.ImportData!;
            var existingData = await LoadExistingDataAsync();
            var settingsImported = await ImportSettingsAsync(importData.Settings);
            var taskImportResult = await ImportTasksAsync(importData.Tasks, existingData);
            var activityImportResult = await ImportActivitiesAsync(importData.Activities, existingData, taskImportResult.TaskIdMapping);

            _logger.LogInformation(
                "Import completed: {ActivitiesImported} activities imported, {ActivitiesSkipped} activities skipped, " +
                "{TasksImported} tasks imported, {TasksSkipped} tasks skipped",
                activityImportResult.ActivitiesImported, activityImportResult.ActivitiesSkipped,
                taskImportResult.TasksImported, taskImportResult.TasksSkipped);

            return ImportResult.Succeeded(
                activityImportResult.ActivitiesImported,
                activityImportResult.ActivitiesSkipped,
                taskImportResult.TasksImported,
                taskImportResult.TasksSkipped,
                settingsImported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogImportJsonFailed);
            return ImportResult.Failed(Constants.Messages.ImportErrorFailed);
        }
    }

    private (bool IsValid, ImportResult Result) ValidateJsonInput(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning(Constants.Messages.LogImportJsonInvalid);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorEmptyFile));
        }

        return (true, ImportResult.Succeeded(0, 0, 0, 0, false));
    }

    private async Task<(bool IsValid, ImportResult Result, ExportData? ImportData)> ParseJsonDataAsync(string jsonData)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 64
        };

        ExportData? importData;
        try
        {
            importData = JsonSerializer.Deserialize<ExportData>(jsonData, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, Constants.Messages.LogImportJsonParseFailed);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorInvalidJson), null);
        }

        if (importData == null)
        {
            _logger.LogWarning(Constants.Messages.LogImportJsonInvalid);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorInvalidFormat), null);
        }

        if (importData.Version <= 0)
        {
            _logger.LogWarning("Import file has invalid version: {Version}", importData.Version);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorInvalidVersion), null);
        }

        return (true, ImportResult.Succeeded(0, 0, 0, 0, false), importData);
    }

    private async Task<ExistingData> LoadExistingDataAsync()
    {
        var existingActivities = await _activityRepository.GetAllAsync();
        var existingTasks = await _taskRepository.GetAllAsync();

        var activityLookup = existingActivities
            .Select(a => new ActivityKey(a.Type, a.CompletedAt, a.DurationMinutes, a.TaskName))
            .ToHashSet();

        var taskLookup = existingTasks
            .Select(t => new TaskKey(t.Name, t.CreatedAt))
            .ToHashSet();

        var existingTaskById = existingTasks.ToDictionary(t => t.Id, t => t);

        var existingTaskByKey = existingTasks
            .GroupBy(t => new TaskKey(t.Name, t.CreatedAt))
            .ToDictionary(g => g.Key, g => g.First());

        return new ExistingData
        {
            Activities = existingActivities,
            Tasks = existingTasks,
            ActivityLookup = activityLookup,
            TaskLookup = taskLookup,
            TaskById = existingTaskById,
            TaskByKey = existingTaskByKey
        };
    }

    private async Task<bool> ImportSettingsAsync(TimerSettings? settings)
    {
        if (settings != null)
        {
            await _settingsRepository.SaveAsync(settings);
            return true;
        }

        return false;
    }

    private async Task<TaskImportResult> ImportTasksAsync(List<TaskItem>? tasks, ExistingData existingData)
    {
        var tasksImported = 0;
        var tasksSkipped = 0;
        var taskIdMapping = new Dictionary<Guid, Guid>();

        if (tasks != null)
        {
            foreach (var task in tasks)
            {
                var key = new TaskKey(task.Name, task.CreatedAt);

                if (existingData.TaskLookup.Contains(key))
                {
                    if (existingData.TaskByKey.TryGetValue(key, out var existingTask) && task.Id != Guid.Empty)
                    {
                        taskIdMapping[task.Id] = existingTask.Id;
                    }
                    else if (task.Id != Guid.Empty)
                    {
                        _logger.LogWarning("Duplicate task found but couldn't map ID {TaskId} for task {Name}", task.Id, task.Name);
                    }
                    tasksSkipped++;
                    _logger.LogDebug("Skipping duplicate task: {Name} created at {CreatedAt}", task.Name, task.CreatedAt);
                }
                else
                {
                    var newId = Guid.NewGuid();
                    var importedTask = new TaskItem
                    {
                        Id = newId,
                        Name = task.Name,
                        PomodoroCount = task.PomodoroCount,
                        TotalFocusMinutes = task.TotalFocusMinutes,
                        IsCompleted = task.IsCompleted,
                        CreatedAt = task.CreatedAt,
                        LastWorkedOn = task.LastWorkedOn,
                        IsDeleted = task.IsDeleted,
                        DeletedAt = task.DeletedAt
                    };
                    await _taskRepository.SaveAsync(importedTask);

                    if (task.Id != Guid.Empty)
                    {
                        taskIdMapping[task.Id] = newId;
                    }

                    tasksImported++;
                    existingData.TaskLookup.Add(key);
                }
            }
        }

        return new TaskImportResult
        {
            TasksImported = tasksImported,
            TasksSkipped = tasksSkipped,
            TaskIdMapping = taskIdMapping
        };
    }

    private async Task<ActivityImportResult> ImportActivitiesAsync(List<ActivityRecord>? activities, ExistingData existingData, Dictionary<Guid, Guid> taskIdMapping)
    {
        var activitiesImported = 0;
        var activitiesSkipped = 0;

        if (activities != null)
        {
            foreach (var activity in activities)
            {
                var key = new ActivityKey(activity.Type, activity.CompletedAt, activity.DurationMinutes, activity.TaskName);

                if (existingData.ActivityLookup.Contains(key))
                {
                    activitiesSkipped++;
                    _logger.LogDebug("Skipping duplicate activity: {Type} at {CompletedAt}", activity.Type, activity.CompletedAt);
                }
                else
                {
                    Guid? newTaskId = null;
                    if (activity.TaskId.HasValue && activity.TaskId.Value != Guid.Empty)
                    {
                        if (taskIdMapping.TryGetValue(activity.TaskId.Value, out var mappedId))
                        {
                            newTaskId = mappedId;
                        }
                        else if (existingData.TaskById.TryGetValue(activity.TaskId.Value, out var existingTask))
                        {
                            newTaskId = activity.TaskId.Value;
                        }
                    }

                    var importedActivity = new ActivityRecord
                    {
                        Id = Guid.NewGuid(),
                        Type = activity.Type,
                        TaskId = newTaskId,
                        TaskName = activity.TaskName,
                        CompletedAt = activity.CompletedAt,
                        DurationMinutes = activity.DurationMinutes,
                        WasCompleted = activity.WasCompleted
                    };
                    await _activityRepository.SaveAsync(importedActivity);
                    activitiesImported++;
                    existingData.ActivityLookup.Add(key);
                }
            }
        }

        return new ActivityImportResult
        {
            ActivitiesImported = activitiesImported,
            ActivitiesSkipped = activitiesSkipped
        };
    }

    #region Import Data Models

    private class ExportData
    {
        public int Version { get; set; }
        public DateTime ExportDate { get; set; }
        public TimerSettings? Settings { get; set; }
        public List<ActivityRecord>? Activities { get; set; }
        public List<TaskItem>? Tasks { get; set; }
    }

    private class ExistingData
    {
        public List<ActivityRecord> Activities { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
        public HashSet<ActivityKey> ActivityLookup { get; set; } = new();
        public HashSet<TaskKey> TaskLookup { get; set; } = new();
        public Dictionary<Guid, TaskItem> TaskById { get; set; } = new();
        public Dictionary<TaskKey, TaskItem> TaskByKey { get; set; } = new();
    }

    private class TaskImportResult
    {
        public int TasksImported { get; set; }
        public int TasksSkipped { get; set; }
        public Dictionary<Guid, Guid> TaskIdMapping { get; set; } = new();
    }

    private class ActivityImportResult
    {
        public int ActivitiesImported { get; set; }
        public int ActivitiesSkipped { get; set; }
    }

    #endregion
}
