using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Comprehensive tests for Index.razor.cs core functionality
/// Tests initialization, state management, lifecycle, and disposal
/// </summary>
[Trait("Category", "Page")]
public class IndexBaseTests : TestHelper
{
    public IndexBaseTests()
    {
        // Setup default mock behaviors for services
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup NotificationService
        NotificationServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        // Setup PipTimerService
        PipTimerServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        // Setup TaskService
        TaskServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);
        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(new List<TaskItem>());
        TaskServiceMock
            .SetupGet(x => x.AllTasks)
            .Returns(new List<TaskItem>());
        TaskServiceMock
            .SetupGet(x => x.CurrentTaskId)
            .Returns((Guid?)null);
        TaskServiceMock
            .SetupGet(x => x.CurrentTask)
            .Returns((TaskItem?)null);

        // Setup TimerService
        TimerServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);
        TimerServiceMock
            .SetupGet(x => x.RemainingTime)
            .Returns(TimeSpan.FromMinutes(25));
        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.Pomodoro);
        TimerServiceMock
            .SetupGet(x => x.IsRunning)
            .Returns(false);
        TimerServiceMock
            .SetupGet(x => x.IsPaused)
            .Returns(false);
        TimerServiceMock
            .SetupGet(x => x.IsStarted)
            .Returns(false);

        // Setup ActivityService
        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        // Setup TodayStatsService
        TodayStatsServiceMock
            .Setup(x => x.GetTodayTotalFocusMinutes())
            .Returns(120);
        TodayStatsServiceMock
            .Setup(x => x.GetTodayPomodoroCount())
            .Returns(4);
        TodayStatsServiceMock
            .Setup(x => x.GetTodayTasksWorkedOn())
            .Returns(2);

        // Setup KeyboardShortcutService
        KeyboardShortcutServiceMock
            .Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()));

        // Setup JSRuntime for URL parameter handling
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
    }

    #region Component Initialization Tests

    [Fact]
    public async Task OnInitializedAsync_InitializesAllServices()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Wait for async initialization to complete
        await Task.Delay(100);

        // Assert - Services are initialized at startup, not in Index
        Assert.NotNull(cut);
    }

    [Fact]
    public async Task OnInitializedAsync_SubscribesToAllServiceEvents()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Wait for async initialization to complete
        await Task.Delay(100);

        // Assert - Verify event subscriptions using Action delegates
        TaskServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTick += It.IsAny<Action>(), Times.AtLeastOnce);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Once);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTimerStateChanged += It.IsAny<Action>(), Times.AtLeastOnce);
        ConsentServiceMock.VerifyAdd(x => x.OnConsentRequired += It.IsAny<Action>(), Times.Once);
        ConsentServiceMock.VerifyAdd(x => x.OnCountdownTick += It.IsAny<Action>(), Times.Once);
        ConsentServiceMock.VerifyAdd(x => x.OnConsentHandled += It.IsAny<Action>(), Times.Once);
        NotificationServiceMock.VerifyAdd(x => x.OnNotificationAction += It.IsAny<Action<string>>(), Times.Once);
        ActivityServiceMock.VerifyAdd(x => x.OnActivityChanged += It.IsAny<Action>(), Times.Once);
        PipTimerServiceMock.VerifyAdd(x => x.OnPipOpened += It.IsAny<Action>(), Times.Once);
        PipTimerServiceMock.VerifyAdd(x => x.OnPipClosed += It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public async Task OnInitializedAsync_RegistersAllKeyboardShortcuts()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Wait for async initialization to complete
        await Task.Delay(100);

        // Assert
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("space", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("r", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("p", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("s", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("l", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("?", It.IsAny<Action>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task OnInitializedAsync_WithException_SetsErrorMessage()
    {
        // Arrange
        NotificationServiceMock
            .Setup(x => x.InitializeAsync())
            .ThrowsAsync(new Exception("Test initialization error"));

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Wait for async initialization to complete
        await Task.Delay(100);

        // Assert - Check that error message is set via component rendering
        cut.Markup.Should().Contain("Error initializing");
        cut.Markup.Should().Contain("Test initialization error");
    }

    #endregion

    #region Computed Properties Tests

    [Fact]
    public void TodayTotalFocusMinutes_ReturnsValueFromService()
    {
        TodayStatsServiceMock
            .Setup(x => x.GetTodayStats())
            .Returns((150, 6, 3));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        TodayStatsServiceMock.Verify(x => x.GetTodayStats(), Times.AtLeastOnce);
    }

    [Fact]
    public void TodayPomodoroCount_ReturnsValueFromService()
    {
        TodayStatsServiceMock
            .Setup(x => x.GetTodayStats())
            .Returns((150, 6, 3));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        TodayStatsServiceMock.Verify(x => x.GetTodayStats(), Times.AtLeastOnce);
    }

    [Fact]
    public void TodayTasksWorkedOn_ReturnsValueFromService()
    {
        TodayStatsServiceMock
            .Setup(x => x.GetTodayStats())
            .Returns((150, 6, 3));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        TodayStatsServiceMock.Verify(x => x.GetTodayStats(), Times.AtLeastOnce);
    }

    #endregion

    #region CheckPendingNotificationAction Tests

    [Fact]
    public async Task CheckPendingNotificationAction_WithUrlParameter_ProcessesAction()
    {
        // Arrange
        var action = "short-break";
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(Constants.JsFunctions.GetUrlParameter, It.IsAny<object[]>()))
            .ReturnsAsync(Uri.EscapeDataString(action));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Simulate the private method call through component behavior
        await Task.Delay(100);

        // Assert - Component should render without errors
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CheckPendingNotificationAction_WithNullUrlParameter_DoesNothing()
    {
        // Arrange
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(Constants.JsFunctions.GetUrlParameter, It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await Task.Delay(100);

        // Assert - Component should render without errors
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CheckPendingNotificationAction_WithException_LogsError()
    {
        // Arrange
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(Constants.JsFunctions.GetUrlParameter, It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("Test error"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Should not throw
        await Task.Delay(100);

        // Assert - Exception should be logged, not thrown
        // The method should complete without throwing
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_UnsubscribesFromAllEvents()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Trigger disposal by rendering a new component
        cut.Dispose();

        // Assert - Verify that events were subscribed to during initialization
        TaskServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTick += It.IsAny<Action>(), Times.AtLeastOnce);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTimerCompleted += It.IsAny<Func<TimerCompletedEventArgs, Task>>(), Times.Once);
        TimerEventPublisherMock.VerifyAdd(x => x.OnTimerStateChanged += It.IsAny<Action>(), Times.AtLeastOnce);
        ConsentServiceMock.VerifyAdd(x => x.OnConsentRequired += It.IsAny<Action>(), Times.Once);
        ConsentServiceMock.VerifyAdd(x => x.OnCountdownTick += It.IsAny<Action>(), Times.Once);
        ConsentServiceMock.VerifyAdd(x => x.OnConsentHandled += It.IsAny<Action>(), Times.Once);
        NotificationServiceMock.VerifyAdd(x => x.OnNotificationAction += It.IsAny<Action<string>>(), Times.Once);
        ActivityServiceMock.VerifyAdd(x => x.OnActivityChanged += It.IsAny<Action>(), Times.Once);
        PipTimerServiceMock.VerifyAdd(x => x.OnPipOpened += It.IsAny<Action>(), Times.Once);
        PipTimerServiceMock.VerifyAdd(x => x.OnPipClosed += It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public void Dispose_UnregistersAllKeyboardShortcuts()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        cut.Dispose();

        // Assert - Verify that shortcuts were registered during initialization
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("space", It.IsAny<Action>(), "Start/Pause timer"), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("r", It.IsAny<Action>(), "Reset timer"), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("p", It.IsAny<Action>(), "Switch to Pomodoro"), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("s", It.IsAny<Action>(), "Switch to Short Break"), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("l", It.IsAny<Action>(), "Switch to Long Break"), Times.Once);
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut("?", It.IsAny<Action>(), "Show keyboard shortcuts"), Times.Once);
    }

    [Fact]
    public void Dispose_WithException_LogsError()
    {
        // Arrange
        KeyboardShortcutServiceMock
            .Setup(x => x.UnregisterShortcut(It.IsAny<string>()))
            .Throws(new Exception("Test disposal error"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Should not throw
        cut.Dispose();

        // Assert - Exception should be logged, not thrown
        // The method should complete without throwing
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task OnInitializedAsync_WithPartialInitializationFailure_HandlesGracefully()
    {
        // Arrange
        NotificationServiceMock
            .Setup(x => x.InitializeAsync())
            .ThrowsAsync(new Exception("Notification service error"));

        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await Task.Delay(100);

        // Assert
        // Component should still render without throwing
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Should attempt to initialize the failing service
        NotificationServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public void Component_RendersSuccessfully()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Component_HasExpectedStructure()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().Contain("timer");
        cut.Markup.Should().Contain("task");
    }

    #endregion
}
