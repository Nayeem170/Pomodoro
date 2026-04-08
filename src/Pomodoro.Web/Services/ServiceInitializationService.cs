using Microsoft.Extensions.Logging;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for initializing application services.
/// Extracted from Program.cs for testability.
/// </summary>
public interface IServiceInitializationService
{
    Task InitializeServicesAsync(IServiceProvider serviceProvider);
}

public class ServiceInitializationService : IServiceInitializationService
{
    private readonly ILogger<ServiceInitializationService> _logger;

    public ServiceInitializationService(ILogger<ServiceInitializationService> logger)
    {
        _logger = logger;
    }

    // Made public virtual for testing purposes
    public virtual bool IsTestEnvironment()
    {
        // Check if running in Playwright/test environment
        // Playwright runs in headless mode which may not support IndexedDB properly
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment == "Development" && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST"));
    }

    public async Task InitializeServicesAsync(IServiceProvider serviceProvider)
    {
        // Initialize IndexedDB first (required by other services)
        // Make it optional - don't fail if it times out (may happen in headless browsers)
        try
        {
            var indexedDb = serviceProvider.GetRequiredService<IIndexedDbService>();
            await indexedDb.InitializeAsync();
            _logger.LogInformation("IndexedDB initialized");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IndexedDB initialization failed - continuing without IndexedDB");
        }

        // Get service references for parallel initialization
        var taskService = serviceProvider.GetRequiredService<ITaskService>();
        var activityService = serviceProvider.GetRequiredService<IActivityService>();
        var timerService = serviceProvider.GetRequiredService<ITimerService>();
        var keyboardShortcutService = serviceProvider.GetRequiredService<IKeyboardShortcutService>();

        // Initialize independent services in parallel for faster startup
        // TaskService and ActivityService don't depend on each other
        // TimerService loads settings independently
        var initTasks = new List<Task>
        {
            taskService.InitializeAsync(),
            activityService.InitializeAsync(),
            timerService.InitializeAsync(),
            keyboardShortcutService.InitializeAsync()
        };

        // Wait for all services to complete
        await Task.WhenAll(initTasks);
        _logger.LogInformation("TaskService, ActivityService, TimerService, and KeyboardShortcutService initialized");

        // Initialize ConsentService to wire up event handlers
        try
        {
            var consentService = serviceProvider.GetRequiredService<IConsentService>();
            consentService.Initialize();
            _logger.LogInformation("ConsentService initialized");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ConsentService initialization failed - continuing");
        }

        _logger.LogInformation("All services initialized successfully");
    }
}
