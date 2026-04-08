using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public class ServiceRegistrationServiceTests
{
    private readonly Mock<ILogger<ServiceRegistrationService>> _loggerMock;
    private readonly ServiceRegistrationService _service;

    public ServiceRegistrationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ServiceRegistrationService>>();
        _service = new ServiceRegistrationService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_InitializesWithLogger()
    {
        // Arrange & Act - done in constructor
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public void RegisterServices_RegistersAppState_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AppState));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void RegisterServices_RegistersIndexedDbService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IIndexedDbService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(IndexedDbService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersTaskRepository_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITaskRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(TaskRepository), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersActivityRepository_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IActivityRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(ActivityRepository), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersSettingsRepository_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISettingsRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(SettingsRepository), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersTaskService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITaskService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(TaskService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersActivityService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IActivityService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(ActivityService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersTimerService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITimerService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(TimerService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersSessionOptionsService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISessionOptionsService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(SessionOptionsService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersConsentService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IConsentService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(ConsentService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersNotificationService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(INotificationService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(NotificationService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersPipTimerService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IPipTimerService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(PipTimerService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersKeyboardShortcutService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IKeyboardShortcutService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(KeyboardShortcutService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersExportService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IExportService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(ExportService), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterServices_RegistersChartService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ChartService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void RegisterServices_RegistersFormatterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert - Check that all formatters are registered
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(StatCardFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(ActivityItemFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(ActivityTimelineFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(TimeFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(ChartDataFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(TimerThemeFormatter)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(SummaryCardsFormatter)));
    }

    [Fact]
    public void RegisterServices_RegistersPresenterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert - Check that all presenter services are registered
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(ITodayStatsService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IHistoryStatsService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(SettingsPresenterService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(HistoryPagePresenterService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IndexPagePresenterService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IJSInteropService)));
    }

    [Fact]
    public void RegisterServices_LogsSuccessMessage()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("All services registered successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RegisterServices_ReturnsServiceCount()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _service.RegisterServices(services);

        // Assert - Check that we have a reasonable number of services registered
        Assert.True(services.Count >= 20, $"Expected at least 20 services, but got {services.Count}");
    }
}
