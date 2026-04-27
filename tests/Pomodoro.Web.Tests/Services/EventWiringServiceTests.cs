using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class EventWiringServiceTests
{
    private readonly Mock<ILogger<EventWiringService>> _mockLogger;
    private readonly EventWiringService _service;

    public EventWiringServiceTests()
    {
        _mockLogger = new Mock<ILogger<EventWiringService>>();
        _service = new EventWiringService(_mockLogger.Object);
    }

    [Fact]
    public void WireEventSubscribers_WiresAllSubscribers()
    {
        var mockTimer = new Mock<ITimerEventPublisher>();
        var mockTask = new Mock<ITimerEventSubscriber>();
        var mockActivity = new Mock<ITimerEventSubscriber>();
        var mockConsent = new Mock<ITimerEventSubscriber>();
        var mockPip = new Mock<ITimerEventPublisherSubscriber>();

        var services = new ServiceCollection();
        services.AddSingleton<ITimerService>(mockTimer.As<ITimerService>().Object);
        services.AddSingleton<ITaskService>(mockTask.As<ITaskService>().Object);
        services.AddSingleton<IActivityService>(mockActivity.As<IActivityService>().Object);
        services.AddSingleton<IConsentService>(mockConsent.As<IConsentService>().Object);
        services.AddSingleton<IPipTimerService>(mockPip.As<IPipTimerService>().Object);
        var sp = services.BuildServiceProvider();

        _service.WireEventSubscribers(sp);

        mockTimer.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Exactly(3));
        mockTimer.VerifyAdd(x => x.OnTick += It.IsAny<Action>(), Times.Once);
        mockTimer.VerifyAdd(x => x.OnTimerStateChanged += It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public void WireEventSubscribers_SkipsMissingServices()
    {
        var mockTimer = new Mock<ITimerEventPublisher>();
        var services = new ServiceCollection();
        services.AddSingleton<ITimerService>(mockTimer.As<ITimerService>().Object);
        var sp = services.BuildServiceProvider();

        _service.WireEventSubscribers(sp);

        mockTimer.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Never);
    }
}
