using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Pomodoro.Web;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for WebAssemblyHostBuilder to enable testing
/// </summary>
public interface IHostBuilderWrapper
{
    IServiceCollection Services { get; }
    IHostEnvironmentWrapper HostEnvironment { get; }
    void AddRootComponent<TComponent>(string selector) where TComponent : IComponent;
}

/// <summary>
/// Interface for WebAssemblyHostEnvironment to enable testing
/// </summary>
public interface IHostEnvironmentWrapper
{
    string BaseAddress { get; }
}

/// <summary>
/// Wrapper implementation for WebAssemblyHostBuilder
/// </summary>
public class WebAssemblyHostBuilderWrapper : IHostBuilderWrapper
{
    private readonly WebAssemblyHostBuilder? _builder;
    private readonly IServiceCollection? _testServices;
    private readonly IWebAssemblyHostEnvironment? _testEnvironment;
    private readonly List<(Type ComponentType, string Selector)> _addedRootComponents = new();

    public WebAssemblyHostBuilderWrapper(WebAssemblyHostBuilder builder)
    {
        _builder = builder;
    }

    internal WebAssemblyHostBuilderWrapper(IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        _testServices = services;
        _testEnvironment = environment;
    }

    public IServiceCollection Services => _builder?.Services ?? _testServices!;
    public IHostEnvironmentWrapper HostEnvironment => new WebAssemblyHostEnvironmentWrapper(_builder?.HostEnvironment ?? _testEnvironment!);

    public void AddRootComponent<TComponent>(string selector) where TComponent : IComponent
    {
        if (_builder != null)
        {
            _builder.RootComponents.Add<TComponent>(selector);
        }
        _addedRootComponents.Add((typeof(TComponent), selector));
    }

    internal IReadOnlyList<(Type ComponentType, string Selector)> AddedRootComponents => _addedRootComponents;
}

/// <summary>
/// Wrapper implementation for WebAssemblyHostEnvironment
/// </summary>
public class WebAssemblyHostEnvironmentWrapper : IHostEnvironmentWrapper
{
    private readonly IWebAssemblyHostEnvironment _environment;

    public WebAssemblyHostEnvironmentWrapper(IWebAssemblyHostEnvironment environment)
    {
        _environment = environment;
    }

    public string BaseAddress => _environment.BaseAddress;
}

/// <summary>
/// Service responsible for handling application startup and host configuration
/// </summary>
public class ApplicationStartupService : IApplicationStartupService
{
    private readonly ILogger<ApplicationStartupService>? _logger;
    private readonly ILogger<ServiceRegistrationService>? _serviceRegistrationLogger;

    public ApplicationStartupService(ILogger<ApplicationStartupService>? logger = null, ILogger<ServiceRegistrationService>? serviceRegistrationLogger = null)
    {
        _logger = logger;
        _serviceRegistrationLogger = serviceRegistrationLogger;
    }

    /// <summary>
    /// Configures the WebAssemblyHostBuilder with root components and services
    /// </summary>
    /// <param name="builder">The WebAssemblyHostBuilder to configure</param>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual void ConfigureHostBuilder(WebAssemblyHostBuilder builder)
    {
        ConfigureHostBuilder(new WebAssemblyHostBuilderWrapper(builder));
    }

    /// <summary>
    /// Configures the WebAssemblyHostBuilder with root components and services (testable overload)
    /// </summary>
    /// <param name="builder">The wrapped builder to configure</param>
    public virtual void ConfigureHostBuilder(IHostBuilderWrapper builder)
    {
        ConfigureHostBuilderInternal(builder);
    }

    /// <summary>
    /// Internal implementation that uses the wrapper interface for testability
    /// </summary>
    /// <param name="builder">The wrapped builder to configure</param>
    protected virtual void ConfigureHostBuilderInternal(IHostBuilderWrapper builder)
    {
        // Add root components
        builder.AddRootComponent<App>(Constants.Blazor.AppRootSelector);
        builder.AddRootComponent<HeadOutlet>(Constants.Blazor.HeadOutletSelector);

        // Add HttpClient
        ConfigureHttpClient(builder.Services, builder.HostEnvironment.BaseAddress);

        // Configure logging
        ConfigureLogging(builder.Services);

        // Register infrastructure services for testability
        RegisterInfrastructureServices(builder.Services);

        // Register all application services
        RegisterApplicationServices(builder.Services);
    }

    /// <summary>
    /// Configures services for application (extracted for testability)
    /// This method contains the core service configuration logic from ConfigureHostBuilder
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="baseAddress">The base address for HttpClient</param>
    public virtual void ConfigureServices(IServiceCollection services, string baseAddress)
    {
        // Add HttpClient
        ConfigureHttpClient(services, baseAddress);

        // Configure logging
        ConfigureLogging(services);

        // Register infrastructure services for testability
        RegisterInfrastructureServices(services);

        // Register all application services
        RegisterApplicationServices(services);
    }

    /// <summary>
    /// Configures the HTTP client with the base address
    /// </summary>
    protected virtual void ConfigureHttpClient(IServiceCollection services, string baseAddress)
    {
        services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
    }

    /// <summary>
    /// Initializes and runs the application host
    /// </summary>
    /// <param name="builder">The configured WebAssemblyHostBuilder</param>
    /// <returns>A task representing the asynchronous operation</returns>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual async Task InitializeAndRunHostAsync(WebAssemblyHostBuilder builder)
    {
        var host = builder.Build();

        // Initialize services with error handling
        await InitializeServicesWithErrorHandlingAsync(host.Services);

        await host.RunAsync();
    }

    /// <summary>
    /// Configures logging services
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    protected virtual void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter(Constants.Logging.MicrosoftCategory, LogLevel.Warning);
            builder.AddFilter(Constants.Logging.SystemCategory, LogLevel.Warning);
        });
    }

    /// <summary>
    /// Registers infrastructure services for testability
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    protected virtual void RegisterInfrastructureServices(IServiceCollection services)
    {
        // Register service registration service for testability
        services.AddScoped<IServiceRegistrationService, ServiceRegistrationService>();

        // Register service initialization service for testability
        services.AddScoped<IServiceInitializationService, ServiceInitializationService>();

        // Register event wiring service for testability
        services.AddScoped<IEventWiringService, EventWiringService>();

        // Register layout presenter service for testability
        services.AddScoped<LayoutPresenterService, LayoutPresenterService>();

        // Register this service for testability
        services.AddScoped<IApplicationStartupService, ApplicationStartupService>();
    }

    /// <summary>
    /// Registers all application services
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    protected virtual void RegisterApplicationServices(IServiceCollection services)
    {
        // Register all application services through ServiceRegistrationService
        var serviceRegistration = new ServiceRegistrationService(_serviceRegistrationLogger);
        serviceRegistration.RegisterServices(services);
    }

    /// <summary>
    /// Initializes services with error handling
    /// </summary>
    /// <param name="services">The service provider to retrieve services from</param>
    /// <returns>A task representing the asynchronous operation</returns>
    protected virtual async Task InitializeServicesWithErrorHandlingAsync(IServiceProvider services)
    {
        try
        {
            // Initialize services through ServiceInitializationService
            var serviceInitialization = services.GetRequiredService<IServiceInitializationService>();
            await serviceInitialization.InitializeServicesAsync(services);

            // Wire up event subscribers through EventWiringService
            var eventWiring = services.GetRequiredService<IEventWiringService>();
            eventWiring.WireEventSubscribers(services);
        }
        catch (Exception ex)
        {
            // Log initialization error - app will still run but may have limited functionality
            _logger?.LogWarning(ex, Constants.Messages.ServiceInitializationFailed);
        }
    }
}