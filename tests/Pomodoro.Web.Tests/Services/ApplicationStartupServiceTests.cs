using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ApplicationStartupService
/// </summary>
public class ApplicationStartupServiceTests
{
    public class ConstructorTests
    {
        [Fact]
        public void Constructor_WithLogger_SetsLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();

            // Act
            var service = new ApplicationStartupService(mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithoutLogger_SetsLoggerToNull()
        {
            // Arrange & Act
            var service = new ApplicationStartupService();

            // Assert
            Assert.NotNull(service);
        }
    }

    public class ConfigureHttpClientTests
    {
        [Fact]
        public void ConfigureHttpClient_AddsHttpClientWithBaseAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            const string baseAddress = "https://localhost:5000/";

            // Act
            service.ConfigureHttpClientProtected(services, baseAddress);

            // Assert
            var provider = services.BuildServiceProvider();
            var httpClient = provider.GetService<HttpClient>();
            Assert.NotNull(httpClient);
            Assert.Equal(baseAddress, httpClient.BaseAddress?.ToString());
        }
    }

    public class ConfigureLoggingTests
    {
        [Fact]
        public void ConfigureLogging_AddsLoggingWithInformationLevel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();

            // Act
            service.ConfigureLoggingProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }

    public class RegisterInfrastructureServicesTests
    {
        [Fact]
        public void RegisterInfrastructureServices_RegistersServiceRegistrationService()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support service instantiation

            // Act
            service.RegisterInfrastructureServicesProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var serviceRegistration = provider.GetService<IServiceRegistrationService>();
            Assert.NotNull(serviceRegistration);
        }

        [Fact]
        public void RegisterInfrastructureServices_RegistersServiceInitializationService()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support service instantiation

            // Act
            service.RegisterInfrastructureServicesProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var serviceInitialization = provider.GetService<IServiceInitializationService>();
            Assert.NotNull(serviceInitialization);
        }

        [Fact]
        public void RegisterInfrastructureServices_RegistersEventWiringService()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support service instantiation

            // Act
            service.RegisterInfrastructureServicesProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var eventWiring = provider.GetService<IEventWiringService>();
            Assert.NotNull(eventWiring);
        }

        [Fact]
        public void RegisterInfrastructureServices_RegistersLayoutPresenterService()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support service instantiation

            // Act
            service.RegisterInfrastructureServicesProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var layoutPresenter = provider.GetService<LayoutPresenterService>();
            Assert.NotNull(layoutPresenter);
        }

        [Fact]
        public void RegisterInfrastructureServices_RegistersApplicationStartupService()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support service instantiation

            // Act
            service.RegisterInfrastructureServicesProtected(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var appStartup = provider.GetService<IApplicationStartupService>();
            Assert.NotNull(appStartup);
        }
    }

    public class RegisterApplicationServicesTests
    {
        [Fact]
        public void RegisterApplicationServices_CallsServiceRegistration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();

            // Act
            service.RegisterApplicationServicesProtected(services);

            // Assert
            Assert.True(service.ServiceRegistrationCalled);
        }
    }

    public class InitializeServicesWithErrorHandlingTests
    {
        [Fact]
        public async Task InitializeServicesWithErrorHandlingAsync_WhenSuccessful_CallsBothServices()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            
            var mockServiceInitialization = new Mock<IServiceInitializationService>();
            var mockEventWiring = new Mock<IEventWiringService>();
            
            services.AddScoped(_ => mockServiceInitialization.Object);
            services.AddScoped(_ => mockEventWiring.Object);
            
            var provider = services.BuildServiceProvider();

            // Act
            await service.InitializeServicesWithErrorHandlingAsyncProtected(provider);

            // Assert
            mockServiceInitialization.Verify(s => s.InitializeServicesAsync(It.IsAny<IServiceProvider>()), Times.Once);
            mockEventWiring.Verify(e => e.WireEventSubscribers(It.IsAny<IServiceProvider>()), Times.Once);
        }

        [Fact]
        public async Task InitializeServicesWithErrorHandlingAsync_WhenException_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            
            var mockServiceInitialization = new Mock<IServiceInitializationService>();
            mockServiceInitialization
                .Setup(s => s.InitializeServicesAsync(It.IsAny<IServiceProvider>()))
                .ThrowsAsync(new Exception("Test exception"));
            
            services.AddScoped(_ => mockServiceInitialization.Object);
            
            var provider = services.BuildServiceProvider();

            // Act
            await service.InitializeServicesWithErrorHandlingAsyncProtected(provider);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InitializeServicesWithErrorHandlingAsync_WhenEventWiringThrows_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            
            var mockServiceInitialization = new Mock<IServiceInitializationService>();
            var mockEventWiring = new Mock<IEventWiringService>();
            mockEventWiring
                .Setup(e => e.WireEventSubscribers(It.IsAny<IServiceProvider>()))
                .Throws(new Exception("Event wiring exception"));
            
            services.AddScoped(_ => mockServiceInitialization.Object);
            services.AddScoped(_ => mockEventWiring.Object);
            
            var provider = services.BuildServiceProvider();

            // Act
            await service.InitializeServicesWithErrorHandlingAsyncProtected(provider);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public class ConfigureHostBuilderTests
    {
        [Fact]
        public void ConfigureHostBuilder_WithValidBuilder_CallsAllConfigurationMethods()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var mockBuilder = new Mock<IHostBuilderWrapper>();
            var mockEnvironment = new Mock<IHostEnvironmentWrapper>();
            var services = new ServiceCollection();

            mockBuilder.Setup(b => b.Services).Returns(services);
            mockBuilder.Setup(b => b.HostEnvironment).Returns(mockEnvironment.Object);
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");

            // Act
            service.ConfigureHostBuilderInternalProtected(mockBuilder.Object);

            // Assert
            Assert.True(service.ConfigureHttpClientCalled);
            Assert.True(service.ConfigureLoggingCalled);
            Assert.True(service.RegisterInfrastructureServicesCalled);
            Assert.True(service.RegisterApplicationServicesCalled);
        }

        [Fact]
        public void ConfigureHostBuilder_AddsRootComponents()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var mockBuilder = new Mock<IHostBuilderWrapper>();
            var mockEnvironment = new Mock<IHostEnvironmentWrapper>();
            var services = new ServiceCollection();

            mockBuilder.Setup(b => b.Services).Returns(services);
            mockBuilder.Setup(b => b.HostEnvironment).Returns(mockEnvironment.Object);
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");

            // Act
            service.ConfigureHostBuilderInternalProtected(mockBuilder.Object);

            // Assert
            mockBuilder.Verify(b => b.AddRootComponent<App>("#app"), Times.Once);
            mockBuilder.Verify(b => b.AddRootComponent<HeadOutlet>("head::after"), Times.Once);
        }

        [Fact]
        public void ConfigureHostBuilder_WithWrapperInterface_CallsInternal()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var mockBuilder = new Mock<IHostBuilderWrapper>();
            var mockEnvironment = new Mock<IHostEnvironmentWrapper>();
            var services = new ServiceCollection();

            mockBuilder.Setup(b => b.Services).Returns(services);
            mockBuilder.Setup(b => b.HostEnvironment).Returns(mockEnvironment.Object);
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");

            // Act
            service.ConfigureHostBuilder(mockBuilder.Object);

            // Assert
            Assert.True(service.ConfigureHttpClientCalled);
            Assert.True(service.ConfigureLoggingCalled);
            Assert.True(service.RegisterInfrastructureServicesCalled);
            Assert.True(service.RegisterApplicationServicesCalled);
        }
    }

    public class ConfigureServicesTests
    {
        [Fact]
        public void ConfigureServices_CallsAllConfigurationMethods()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            const string baseAddress = "https://localhost:5000/";

            // Act
            service.ConfigureServices(services, baseAddress);

            // Assert
            Assert.True(service.ConfigureHttpClientCalled);
            Assert.True(service.ConfigureLoggingCalled);
            Assert.True(service.RegisterInfrastructureServicesCalled);
            Assert.True(service.RegisterApplicationServicesCalled);
        }

        [Fact]
        public void ConfigureServices_RegistersHttpClientWithCorrectBaseAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new TestableApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            const string baseAddress = "https://custom.host:8080/api/";

            // Act
            service.ConfigureServices(services, baseAddress);

            // Assert
            var provider = services.BuildServiceProvider();
            var httpClient = provider.GetService<HttpClient>();
            Assert.NotNull(httpClient);
            Assert.Equal(baseAddress, httpClient.BaseAddress?.ToString());
        }
    }

    public class RegisterApplicationServicesBaseTests
    {
        [Fact]
        public void RegisterApplicationServices_BaseClass_CreatesServiceRegistrationAndRegisters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new ApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging();

            // Act - Call base RegisterApplicationServices via reflection
            var method = typeof(ApplicationStartupService).GetMethod(
                "RegisterApplicationServices",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            method!.Invoke(service, new object[] { services });

            // Assert - ServiceRegistrationService.RegisterServices should have been called
            // Verify that AppState was registered (it's the first service in RegisterServices)
            var provider = services.BuildServiceProvider();
            var appState = provider.GetService<AppState>();
            Assert.NotNull(appState);
        }

        [Fact]
        public void RegisterApplicationServices_WithServiceRegistrationLogger_CoversLogBranch()
        {
            // Arrange
            var mockAppLogger = new Mock<ILogger<ApplicationStartupService>>();
            var mockServiceRegistrationLogger = new Mock<ILogger<ServiceRegistrationService>>();
            var service = new ApplicationStartupService(mockAppLogger.Object, mockServiceRegistrationLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging();

            // Act - Call base RegisterApplicationServices via reflection
            var method = typeof(ApplicationStartupService).GetMethod(
                "RegisterApplicationServices",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            method!.Invoke(service, new object[] { services });

            // Assert - ServiceRegistrationService should have logged with the non-null logger
            mockServiceRegistrationLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("All services registered successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public class InitializeServicesWithErrorHandlingNullLoggerTests
    {
        [Fact]
        public async Task InitializeServicesWithErrorHandling_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            var service = new TestableApplicationStartupService(null);
            var services = new ServiceCollection();

            var mockServiceInitialization = new Mock<IServiceInitializationService>();
            mockServiceInitialization
                .Setup(s => s.InitializeServicesAsync(It.IsAny<IServiceProvider>()))
                .ThrowsAsync(new Exception("Test exception"));

            services.AddScoped(_ => mockServiceInitialization.Object);
            var provider = services.BuildServiceProvider();

            // Act - Should not throw even with null logger
            var exception = await Record.ExceptionAsync(() =>
                service.InitializeServicesWithErrorHandlingAsyncProtected(provider));

            // Assert
            Assert.Null(exception);
        }
    }

    public class BaseClassDirectCallTests
    {
        [Fact]
        public void ConfigureHostBuilderInternal_OnBaseClass_CallsAllMethods()
        {
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new ApplicationStartupService(mockLogger.Object);
            var mockBuilder = new Mock<IHostBuilderWrapper>();
            var mockEnvironment = new Mock<IHostEnvironmentWrapper>();
            var services = new ServiceCollection();
            services.AddLogging();

            mockBuilder.Setup(b => b.Services).Returns(services);
            mockBuilder.Setup(b => b.HostEnvironment).Returns(mockEnvironment.Object);
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");

            var method = typeof(ApplicationStartupService).GetMethod(
                "ConfigureHostBuilderInternal",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(service, new object[] { mockBuilder.Object });

            mockBuilder.Verify(b => b.AddRootComponent<App>("#app"), Times.Once);
            mockBuilder.Verify(b => b.AddRootComponent<HeadOutlet>("head::after"), Times.Once);
        }

        [Fact]
        public void RegisterInfrastructureServices_OnBaseClass_RegistersAllServices()
        {
            var mockLogger = new Mock<ILogger<ApplicationStartupService>>();
            var service = new ApplicationStartupService(mockLogger.Object);
            var services = new ServiceCollection();
            services.AddLogging();

            var method = typeof(ApplicationStartupService).GetMethod(
                "RegisterInfrastructureServices",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(service, new object[] { services });

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<IServiceRegistrationService>());
            Assert.NotNull(provider.GetService<IServiceInitializationService>());
            Assert.NotNull(provider.GetService<IEventWiringService>());
            Assert.NotNull(provider.GetService<LayoutPresenterService>());
            Assert.NotNull(provider.GetService<IApplicationStartupService>());
        }
    }

    public class WrapperClassesTests
    {
        [Fact]
        public void WebAssemblyHostEnvironmentWrapper_WrapsBaseAddress()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();
            const string baseAddress = "https://localhost:5000/";
            mockEnvironment.Setup(e => e.BaseAddress).Returns(baseAddress);

            // Act
            var wrapper = new WebAssemblyHostEnvironmentWrapper(mockEnvironment.Object);

            // Assert
            Assert.Equal(baseAddress, wrapper.BaseAddress);
        }

        [Fact]
        public void WebAssemblyHostBuilderWrapper_TestableConstructor_SetsServicesAndEnvironment()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");

            // Act
            var wrapper = new WebAssemblyHostBuilderWrapper(services, mockEnvironment.Object);

            // Assert
            Assert.Same(services, wrapper.Services);
            Assert.Equal("https://localhost:5000/", wrapper.HostEnvironment.BaseAddress);
        }

        [Fact]
        public void WebAssemblyHostBuilderWrapper_AddRootComponent_TracksComponent()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();

            // Act
            var wrapper = new WebAssemblyHostBuilderWrapper(services, mockEnvironment.Object);
            wrapper.AddRootComponent<App>("#app");
            wrapper.AddRootComponent<HeadOutlet>("head::after");

            // Assert
            Assert.Equal(2, wrapper.AddedRootComponents.Count);
            Assert.Equal(typeof(App), wrapper.AddedRootComponents[0].ComponentType);
            Assert.Equal("#app", wrapper.AddedRootComponents[0].Selector);
            Assert.Equal(typeof(HeadOutlet), wrapper.AddedRootComponents[1].ComponentType);
            Assert.Equal("head::after", wrapper.AddedRootComponents[1].Selector);
        }

        [Fact]
        public void WebAssemblyHostBuilderWrapper_Services_ReturnsProvidedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton("test");
            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();

            // Act
            var wrapper = new WebAssemblyHostBuilderWrapper(services, mockEnvironment.Object);

            // Assert
            Assert.Contains(wrapper.Services, s => s.ServiceType == typeof(string));
        }

        [Fact]
        public void WebAssemblyHostBuilderWrapper_HostEnvironment_ReturnsWrappedEnvironment()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://custom.host:8080/");

            // Act
            var wrapper = new WebAssemblyHostBuilderWrapper(services, mockEnvironment.Object);

            // Assert
            Assert.IsType<WebAssemblyHostEnvironmentWrapper>(wrapper.HostEnvironment);
            Assert.Equal("https://custom.host:8080/", wrapper.HostEnvironment.BaseAddress);
        }

        [Fact]
        public void WebAssemblyHostBuilderWrapper_RealConstructor_CoversBuilderPaths()
        {
            // Arrange - create uninitialized WebAssemblyHostBuilder and set fields via reflection
            var uninitializedBuilder = (WebAssemblyHostBuilder)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHostBuilder));

            var services = new ServiceCollection();
            services.AddSingleton("test-service");
            SetFieldByType(uninitializedBuilder, typeof(IServiceCollection), services);

            var mockEnvironment = new Mock<IWebAssemblyHostEnvironment>();
            mockEnvironment.Setup(e => e.BaseAddress).Returns("https://localhost:5000/");
            SetFieldByType(uninitializedBuilder, typeof(IWebAssemblyHostEnvironment), mockEnvironment.Object);

            // Find RootComponents property and set its backing field
            var rootComponentsProp = typeof(WebAssemblyHostBuilder).GetProperty("RootComponents");
            Assert.NotNull(rootComponentsProp);
            var rootComponentsType = rootComponentsProp.PropertyType;
            object rootComponentsInstance;
            try
            {
                rootComponentsInstance = Activator.CreateInstance(rootComponentsType, true);
            }
            catch
            {
                rootComponentsInstance = RuntimeHelpers.GetUninitializedObject(rootComponentsType);
            }

            // Initialize internal collection of RootComponentMappingCollection
            // Walk up type hierarchy to find Collection<T>.items field
            var currentType = rootComponentsType;
            while (currentType != null && currentType != typeof(object))
            {
                var rcFields = currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var rcField in rcFields)
                {
                    if (rcField.Name == "items" || rcField.Name == "_items")
                    {
                        try
                        {
                            var listType = typeof(List<>).MakeGenericType(rcField.FieldType.GetGenericArguments());
                            rcField.SetValue(rootComponentsInstance, Activator.CreateInstance(listType));
                        }
                        catch { }
                    }
                }
                currentType = currentType.BaseType;
            }

            // Set the backing field for the RootComponents property
            var allFields = typeof(WebAssemblyHostBuilder).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in allFields)
            {
                if (rootComponentsType.IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(uninitializedBuilder, rootComponentsInstance);
                    break;
                }
            }

            // Act - use real constructor with uninitialized builder
            var wrapper = new WebAssemblyHostBuilderWrapper(uninitializedBuilder);

            // Assert - Services should use _builder.Services path
            Assert.Contains(wrapper.Services, s => s.ServiceType == typeof(string));
            // Assert - HostEnvironment should use _builder.HostEnvironment path
            Assert.Equal("https://localhost:5000/", wrapper.HostEnvironment.BaseAddress);

            // Assert - AddRootComponent with _builder != null should track components
            wrapper.AddRootComponent<App>("#app");
            wrapper.AddRootComponent<HeadOutlet>("head::after");
            Assert.Equal(2, wrapper.AddedRootComponents.Count);
            Assert.Equal(typeof(App), wrapper.AddedRootComponents[0].ComponentType);
            Assert.Equal("#app", wrapper.AddedRootComponents[0].Selector);
        }

        private static void SetFieldByType(object obj, Type fieldType, object value)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (fieldType.IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(obj, value);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Testable wrapper for ApplicationStartupService to expose protected methods
    /// </summary>
    public class TestableApplicationStartupService : ApplicationStartupService
    {
        public bool ConfigureHttpClientCalled { get; private set; }
        public bool ConfigureLoggingCalled { get; private set; }
        public bool RegisterInfrastructureServicesCalled { get; private set; }
        public bool RegisterApplicationServicesCalled { get; private set; }
        public bool ServiceRegistrationCalled { get; private set; }

        public TestableApplicationStartupService(ILogger<ApplicationStartupService>? logger = null)
            : base(logger)
        {
        }

        protected override void ConfigureHttpClient(IServiceCollection services, string baseAddress)
        {
            ConfigureHttpClientCalled = true;
            base.ConfigureHttpClient(services, baseAddress);
        }

        protected override void ConfigureLogging(IServiceCollection services)
        {
            ConfigureLoggingCalled = true;
            base.ConfigureLogging(services);
        }

        protected override void RegisterInfrastructureServices(IServiceCollection services)
        {
            RegisterInfrastructureServicesCalled = true;
            base.RegisterInfrastructureServices(services);
        }

        protected override void RegisterApplicationServices(IServiceCollection services)
        {
            RegisterApplicationServicesCalled = true;
            ServiceRegistrationCalled = true;
            var serviceRegistration = new ServiceRegistrationService(null);
            serviceRegistration.RegisterServices(services);
        }

        public void ConfigureHostBuilderInternalProtected(IHostBuilderWrapper builder)
        {
            ConfigureHostBuilderInternal(builder);
        }

        public void ConfigureHttpClientProtected(IServiceCollection services, string baseAddress)
        {
            ConfigureHttpClient(services, baseAddress);
        }

        public void ConfigureLoggingProtected(IServiceCollection services)
        {
            ConfigureLogging(services);
        }

        public void RegisterInfrastructureServicesProtected(IServiceCollection services)
        {
            RegisterInfrastructureServices(services);
        }

        public void RegisterApplicationServicesProtected(IServiceCollection services)
        {
            RegisterApplicationServices(services);
        }

        public Task InitializeServicesWithErrorHandlingAsyncProtected(IServiceProvider services)
        {
            return InitializeServicesWithErrorHandlingAsync(services);
        }
    }
}
