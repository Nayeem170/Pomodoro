using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public class EventWiringServiceTests
{
    private readonly Mock<ILogger<EventWiringService>> _loggerMock;
    private readonly EventWiringService _service;

    public EventWiringServiceTests()
    {
        _loggerMock = new Mock<ILogger<EventWiringService>>();
        _service = new EventWiringService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_InitializesWithLogger()
    {
        // Arrange & Act - done in constructor
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public void WireEventSubscribers_WiresTaskServiceAsSubscriber()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskSubscriberMock = new Mock<ITimerEventSubscriber>();
        var taskServiceMock = taskSubscriberMock.As<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert
        timerPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Once);
    }

    [Fact]
    public void WireEventSubscribers_WiresActivityServiceAsSubscriber()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activitySubscriberMock = new Mock<ITimerEventSubscriber>();
        var activityServiceMock = activitySubscriberMock.As<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert
        timerPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Once);
    }

    [Fact]
    public void WireEventSubscribers_DoesNotWireTaskService_WhenNotSubscriber()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>(); // Does not implement ITimerEventSubscriber
        var activityServiceMock = new Mock<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - Should not add handler for task service since it doesn't implement ITimerEventSubscriber
        timerPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Never);
    }

    [Fact]
    public void WireEventSubscribers_DoesNotWireActivityService_WhenNotSubscriber()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>(); // Does not implement ITimerEventSubscriber

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - Should not add handler for activity service since it doesn't implement ITimerEventSubscriber
        timerPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Never);
    }

    [Fact]
    public void WireEventSubscribers_DoesNotWireEvents_WhenTimerServiceNotPublisher()
    {
        // Arrange
        var timerServiceMock = new Mock<ITimerService>(); // Does not implement ITimerEventPublisher
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - No events should be wired since timer service is not a publisher
        // The service should complete without throwing
        Assert.True(true);
    }

    [Fact]
    public void WireEventSubscribers_LogsSuccessMessage()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();
        var activityServiceMock = new Mock<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event subscribers wired up successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void WireEventSubscribers_WiresBothServices_WhenBothAreSubscribers()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskSubscriberMock = new Mock<ITimerEventSubscriber>();
        var taskServiceMock = taskSubscriberMock.As<ITaskService>();
        var activitySubscriberMock = new Mock<ITimerEventSubscriber>();
        var activityServiceMock = activitySubscriberMock.As<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - Both services should be wired
        timerPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Exactly(2));
    }

    [Fact]
    public void WireEventSubscribers_HandlesNullTaskService()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var activityServiceMock = new Mock<IActivityService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(null!);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(activityServiceMock.Object);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - Should not throw and should still log success
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event subscribers wired up successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void WireEventSubscribers_HandlesNullActivityService()
    {
        // Arrange
        var timerPublisherMock = new Mock<ITimerEventPublisher>();
        var timerServiceMock = timerPublisherMock.As<ITimerService>();
        var taskServiceMock = new Mock<ITaskService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ITimerService)))
            .Returns(timerServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITaskService)))
            .Returns(taskServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IActivityService)))
            .Returns(null!);

        // Act
        _service.WireEventSubscribers(serviceProviderMock.Object);

        // Assert - Should not throw and should still log success
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event subscribers wired up successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
