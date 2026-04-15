using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class ServiceInitializationServiceTests
{
    private readonly Mock<ILogger<ServiceInitializationService>> _loggerMock;
    private readonly ServiceInitializationService _service;

    public ServiceInitializationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ServiceInitializationService>>();
        _service = new ServiceInitializationService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_InitializesWithLogger()
    {
        // Arrange & Act - done in constructor
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesIndexedDbFirst()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        indexedDbMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesTaskService()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        taskServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesActivityService()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        activityServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesTimerService()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        timerServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesKeyboardShortcutService()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        keyboardShortcutServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_InitializesConsentService()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        consentServiceMock.Verify(x => x.Initialize(), Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_LogsIndexedDbInitialization()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("IndexedDB initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_LogsServicesInitialization()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TaskService, ActivityService, TimerService, and KeyboardShortcutService initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_LogsConsentServiceInitialization()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ConsentService initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_LogsSuccessMessage()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("All services initialized successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_HandlesIndexedDbInitializationFailure()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        indexedDbMock.Setup(x => x.InitializeAsync())
            .ThrowsAsync(new InvalidOperationException("IndexedDB not available"));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act & Assert - should not throw
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("IndexedDB initialization failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeServicesAsync_HandlesConsentServiceInitializationFailure()
    {
        // Arrange
        var indexedDbMock = new Mock<IIndexedDbService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();
        var timerServiceMock = new Mock<ITimerService>();
        var keyboardShortcutServiceMock = new Mock<IKeyboardShortcutService>();
        var consentServiceMock = new Mock<IConsentService>();

        consentServiceMock.Setup(x => x.Initialize())
            .Throws(new InvalidOperationException("ConsentService initialization failed"));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IIndexedDbService)))
            .Returns(indexedDbMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IKeyboardShortcutService)))
            .Returns(keyboardShortcutServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IConsentService)))
            .Returns(consentServiceMock.Object);

        // Act & Assert - should not throw
        await _service.InitializeServicesAsync(serviceProviderMock.Object);

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ConsentService initialization failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsTestEnvironment_ReturnsFalse_WhenNotInTestEnvironment()
    {
        // Arrange - ensure environment variables are not set
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalPlaywright = Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST");
        
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", null);

            // Act
            var result = _service.IsTestEnvironment();

            // Assert
            Assert.False(result);
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", originalPlaywright);
        }
    }

    [Fact]
    public void IsTestEnvironment_ReturnsTrue_WhenInPlaywrightTestEnvironment()
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalPlaywright = Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST");
        
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", "true");

            // Act
            var result = _service.IsTestEnvironment();

            // Assert
            Assert.True(result);
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", originalPlaywright);
        }
    }

    [Fact]
    public void IsTestEnvironment_ReturnsFalse_WhenOnlyDevelopmentIsSet()
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalPlaywright = Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST");
        
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", null);

            // Act
            var result = _service.IsTestEnvironment();

            // Assert
            Assert.False(result);
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", originalPlaywright);
        }
    }

    [Fact]
    public void IsTestEnvironment_ReturnsFalse_WhenOnlyPlaywrightIsSet()
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalPlaywright = Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST");
        
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", "true");

            // Act
            var result = _service.IsTestEnvironment();

            // Assert
            Assert.False(result);
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("PLAYWRIGHT_TEST", originalPlaywright);
        }
    }
}

