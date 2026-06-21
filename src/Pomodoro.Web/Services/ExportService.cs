using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class ExportService : IExportService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IPomodoroMetaRepository _pomodoroMetaRepo;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IActivityRepository activityRepository,
        ITaskRepository taskRepository,
        ISettingsRepository settingsRepository,
        IPomodoroMetaRepository pomodoroMetaRepo,
        ILogger<ExportService> logger)
    {
        _activityRepository = activityRepository;
        _taskRepository = taskRepository;
        _settingsRepository = settingsRepository;
        _pomodoroMetaRepo = pomodoroMetaRepo;
        _logger = logger;
    }

    public async Task<string> ExportToJsonAsync()
    {
        try
        {
            var activities = await _activityRepository.GetAllAsync();
            var tasks = await _taskRepository.GetAllAsync();
            var settings = await _settingsRepository.GetAsync();
            var pomoMeta = (await _pomodoroMetaRepo.GetAllAsync()).ToList();

            var exportData = new ExportData
            {
                Version = 2,
                ExportDate = DateTime.UtcNow,
                Settings = settings,
                Activities = activities,
                Tasks = tasks.Where(t => !t.IsGoogleTask).Select(t => t.WithUpdates(c =>
                {
                    c.GoogleTaskId = null;
                    c.GoogleListId = null;
                    c.ETag = null;
                    c.UpdatedAt = null;
                })).ToList(),
                PomodoroMeta = pomoMeta
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

    public async Task<string> ExportToJsonStringAsync()
    {
        try
        {
            var activities = await _activityRepository.GetAllAsync();
            var tasks = await _taskRepository.GetAllAsync();
            var settings = await _settingsRepository.GetAsync();
            var pomoMeta = (await _pomodoroMetaRepo.GetAllAsync()).ToList();

            var exportData = new ExportData
            {
                Version = 2,
                ExportDate = DateTime.UtcNow,
                Settings = settings,
                Activities = activities,
                Tasks = tasks.Where(t => !t.IsGoogleTask).Select(t => t.WithUpdates(c =>
                {
                    c.GoogleTaskId = null;
                    c.GoogleListId = null;
                    c.ETag = null;
                    c.UpdatedAt = null;
                })).ToList(),
                PomodoroMeta = pomoMeta
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = false,
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

    public async Task ClearAllDataAsync()
    {
        try
        {
            var activityCount = await _activityRepository.GetCountAsync();
            var taskCount = await _taskRepository.GetCountAsync();

            await _activityRepository.ClearAllAsync();
            await _taskRepository.ClearAllAsync();

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
}
