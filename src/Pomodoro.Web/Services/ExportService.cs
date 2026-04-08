using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for exporting and importing application data
/// </summary>
public class ExportService : IExportService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IActivityRepository activityRepository,
        ITaskRepository taskRepository,
        ISettingsRepository settingsRepository,
        ILogger<ExportService> logger)
    {
        _activityRepository = activityRepository;
        _taskRepository = taskRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Exports all data to JSON format
    /// </summary>
    public async Task<string> ExportToJsonAsync()
    {
        try
        {
            // Get all data from IndexedDB
            var activities = await _activityRepository.GetAllAsync();
            var tasks = await _taskRepository.GetAllAsync();
            var settings = await _settingsRepository.GetAsync();
            
            // Create export data structure
            var exportData = new
            {
                Version = 1,
                ExportDate = DateTime.UtcNow,
                Settings = settings,
                Activities = activities,
                Tasks = tasks
            };
            
            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            _logger.LogInformation(Constants.Messages.LogExportJsonFormat, activities.Count, tasks.Count);
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogExportJsonFailed);
            throw;
        }
    }

    /// <summary>
    /// Imports data from JSON format with duplicate detection and validation
    /// </summary>
    public async Task<ImportResult> ImportFromJsonAsync(string jsonData)
    {
        try
        {
            // Validate input
            var validationResult = ValidateJsonInput(jsonData);
            if (!validationResult.IsValid)
            {
                return validationResult.Result;
            }

            // Parse JSON
            var parseResult = await ParseJsonDataAsync(jsonData);
            if (!parseResult.IsValid)
            {
                return parseResult.Result;
            }

            var importData = parseResult.ImportData!;
            
            // Load existing data for duplicate detection
            var existingData = await LoadExistingDataAsync();
            
            // Import settings
            var settingsImported = await ImportSettingsAsync(importData.Settings);
            
            // Import tasks with duplicate detection
            var taskImportResult = await ImportTasksAsync(importData.Tasks, existingData);
            
            // Import activities with duplicate detection
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

    /// <summary>
    /// Validates JSON input data
    /// </summary>
    private (bool IsValid, ImportResult Result) ValidateJsonInput(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning(Constants.Messages.LogImportJsonInvalid);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorEmptyFile));
        }

        return (true, ImportResult.Succeeded(0, 0, 0, 0, false));
    }

    /// <summary>
    /// Parses JSON data and validates structure
    /// </summary>
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
        
        // Validate parsed data structure
        if (importData == null)
        {
            _logger.LogWarning(Constants.Messages.LogImportJsonInvalid);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorInvalidFormat), null);
        }

        // Validate required fields
        if (importData.Version <= 0)
        {
            _logger.LogWarning("Import file has invalid version: {Version}", importData.Version);
            return (false, ImportResult.Failed(Constants.Messages.ImportErrorInvalidVersion), null);
        }

        return (true, ImportResult.Succeeded(0, 0, 0, 0, false), importData);
    }

    /// <summary>
    /// Loads existing data for duplicate detection
    /// </summary>
    private async Task<ExistingData> LoadExistingDataAsync()
    {
        var existingActivities = await _activityRepository.GetAllAsync();
        var existingTasks = await _taskRepository.GetAllAsync();
        
        // Create lookup sets for efficient duplicate detection
        var activityLookup = existingActivities
            .Select(a => new ActivityKey(a.Type, a.CompletedAt, a.DurationMinutes, a.TaskName))
            .ToHashSet();
        
        var taskLookup = existingTasks
            .Select(t => new TaskKey(t.Name, t.CreatedAt))
            .ToHashSet();
        
        // Build dictionary for O(1) task ID lookups during activity import
        var existingTaskById = existingTasks.ToDictionary(t => t.Id, t => t);
        
        // Build dictionary for O(1) task lookups by key (Name + CreatedAt) for duplicate mapping
        // Use GroupBy to handle potential duplicates gracefully (take first occurrence)
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

    /// <summary>
    /// Imports settings
    /// </summary>
    private async Task<bool> ImportSettingsAsync(TimerSettings? settings)
    {
        if (settings != null)
        {
            await _settingsRepository.SaveAsync(settings);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Imports tasks with duplicate detection
    /// </summary>
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
                    // Duplicate found - skip but still map the ID for activity references
                    // Use O(1) dictionary lookup instead of O(n) FirstOrDefault
                    if (existingData.TaskByKey.TryGetValue(key, out var existingTask) && task.Id != Guid.Empty)
                    {
                        taskIdMapping[task.Id] = existingTask.Id;
                    }
                    else if (task.Id != Guid.Empty)
                    {
                        // Edge case: taskLookup contained the key but dictionary lookup failed (data inconsistency)
                        _logger.LogWarning("Duplicate task found but couldn't map ID {TaskId} for task {Name}", task.Id, task.Name);
                    }
                    tasksSkipped++;
                    _logger.LogDebug("Skipping duplicate task: {Name} created at {CreatedAt}", task.Name, task.CreatedAt);
                }
                else
                {
                    // Not a duplicate - create new record with new ID
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
                    
                    // Map old task ID to new task ID for activity references
                    if (task.Id != Guid.Empty)
                    {
                        taskIdMapping[task.Id] = newId;
                    }
                    
                    tasksImported++;
                    existingData.TaskLookup.Add(key); // Add to lookup to catch duplicates within the same import file
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

    /// <summary>
    /// Imports activities with duplicate detection
    /// </summary>
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
                    // Duplicate found - skip
                    activitiesSkipped++;
                    _logger.LogDebug("Skipping duplicate activity: {Type} at {CompletedAt}", activity.Type, activity.CompletedAt);
                }
                else
                {
                    // Map the old TaskId to the new TaskId if it exists
                    Guid? newTaskId = null;
                    if (activity.TaskId.HasValue && activity.TaskId.Value != Guid.Empty)
                    {
                        if (taskIdMapping.TryGetValue(activity.TaskId.Value, out var mappedId))
                        {
                            newTaskId = mappedId;
                        }
                        else if (existingData.TaskById.TryGetValue(activity.TaskId.Value, out var existingTask))
                        {
                            // Task wasn't in the import but exists in the database, keep the reference
                            newTaskId = activity.TaskId.Value;
                        }
                        // else: Task doesn't exist, leave TaskId as null
                    }
                    
                    // Not a duplicate - create new record with new ID
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
                    existingData.ActivityLookup.Add(key); // Add to lookup to catch duplicates within the same import file
                }
            }
        }
        
        return new ActivityImportResult
        {
            ActivitiesImported = activitiesImported,
            ActivitiesSkipped = activitiesSkipped
        };
    }

    /// <summary>
    /// Clears all application data
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        try
        {
            var activityCount = await _activityRepository.GetCountAsync();
            var taskCount = await _taskRepository.GetCountAsync();
            
            await _activityRepository.ClearAllAsync();
            await _taskRepository.ClearAllAsync();
            
            // Clear settings by resetting to defaults
            var defaultSettings = new TimerSettings();
            await _settingsRepository.SaveAsync(defaultSettings);
            
            _logger.LogInformation(Constants.Messages.LogClearDataSuccess, activityCount, taskCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogClearDataFailed);
            throw;
        }
    }

    #region Export Data Model

    /// <summary>
    /// Data structure for JSON export/import
    /// </summary>
    private class ExportData
    {
        public int Version { get; set; }
        public DateTime ExportDate { get; set; }
        public TimerSettings? Settings { get; set; }
        public List<ActivityRecord>? Activities { get; set; }
        public List<TaskItem>? Tasks { get; set; }
    }

    /// <summary>
    /// Container for existing data used during import
    /// </summary>
    private class ExistingData
    {
        public List<ActivityRecord> Activities { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
        public HashSet<ActivityKey> ActivityLookup { get; set; } = new();
        public HashSet<TaskKey> TaskLookup { get; set; } = new();
        public Dictionary<Guid, TaskItem> TaskById { get; set; } = new();
        public Dictionary<TaskKey, TaskItem> TaskByKey { get; set; } = new();
    }

    /// <summary>
    /// Result of task import operation
    /// </summary>
    private class TaskImportResult
    {
        public int TasksImported { get; set; }
        public int TasksSkipped { get; set; }
        public Dictionary<Guid, Guid> TaskIdMapping { get; set; } = new();
    }

    /// <summary>
    /// Result of activity import operation
    /// </summary>
    private class ActivityImportResult
    {
        public int ActivitiesImported { get; set; }
        public int ActivitiesSkipped { get; set; }
    }

    #endregion
}
