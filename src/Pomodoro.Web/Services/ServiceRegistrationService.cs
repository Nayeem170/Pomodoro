using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for re6gistering all application services with DI container.
/// Extracted from Program.cs for testability.
/// </summary>
public interface IServiceRegistrationService
{
    void RegisterServices(IServiceCollection services);
}

public class ServiceRegistrationService : IServiceRegistrationService
{
    private readonly ILogger<ServiceRegistrationService>? _logger;

    public ServiceRegistrationService(ILogger<ServiceRegistrationService>? logger = null)
    {
        _logger = logger;
    }

    public void RegisterServices(IServiceCollection services)
    {
        // Register AppState as singleton-like scoped service
        services.AddScoped<AppState>();

        // Register IndexedDB service (must be registered before services that depend on it)
        services.AddScoped<IIndexedDbService, IndexedDbService>();

        // Register repositories (must be registered before services that depend on them)
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        // Register services that use repositories and implement ITimerEventSubscriber
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ITimerService, TimerService>();
        services.AddScoped<ISessionOptionsService, SessionOptionsService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPipTimerService, PipTimerService>();
        services.AddScoped<IChartService, ChartService>();
        services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IDailyStatsService, DailyStatsService>();
        services.AddScoped<IJsTimerInterop, JsTimerInterop>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Register formatter services for components (enables testable code with coverage tracking)
        services.AddScoped<StatCardFormatter>();
        services.AddScoped<ActivityItemFormatter>();
        services.AddScoped<ActivityTimelineFormatter>();
        services.AddScoped<TimeFormatter>();
        services.AddScoped<ChartDataFormatter>();
        services.AddScoped<TimerThemeFormatter>();
        services.AddScoped<SummaryCardsFormatter>();
        services.AddScoped<ITodayStatsService, TodayStatsService>();
        services.AddScoped<IHistoryStatsService, HistoryStatsService>();
        services.AddScoped<SettingsPresenterService>();
        services.AddScoped<HistoryPagePresenterService>();
        services.AddScoped<IndexPagePresenterService>();
        services.AddScoped<IJSInteropService, JSInteropService>();
        services.AddScoped<IInfiniteScrollInterop, InfiniteScrollInterop>();
        services.AddScoped<ILocalDateTimeService, LocalDateTimeService>();

        _logger?.LogInformation("All services registered successfully");
    }
}
