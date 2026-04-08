using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.Pages;

/// <summary>
/// Tests for Index page keyboard shortcut functionality.
/// These tests ensure that keyboard shortcut handlers are properly registered.
/// Note: Due to SafeTaskRunner.RunAndForget using Task.Run for async execution,
/// we focus on verifying shortcut registration rather than execution.
/// </summary>
public class IndexKeyboardShortcutTests : TestHelper
{
    public IndexKeyboardShortcutTests()
    {
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

        // Setup JSRuntime for URL parameter handling
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
    }

    #region Play/Pause Shortcut Tests

    [Fact]
    public async Task KeyboardShortcut_PlayPause_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify play/pause shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "space",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once, 
            "Play/Pause shortcut should be registered");
    }

    #endregion

    #region Reset Shortcut Tests

    [Fact]
    public async Task KeyboardShortcut_Reset_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify reset shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "r",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once,
            "Reset shortcut should be registered");
    }

    #endregion

    #region Session Switching Shortcuts Tests

    [Fact]
    public async Task KeyboardShortcut_Pomodoro_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify pomodoro shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "p",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once,
            "Pomodoro shortcut should be registered");
    }

    [Fact]
    public async Task KeyboardShortcut_ShortBreak_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify short break shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "s",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once,
            "Short break shortcut should be registered");
    }

    [Fact]
    public async Task KeyboardShortcut_LongBreak_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify long break shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "l",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once,
            "Long break shortcut should be registered");
    }

    #endregion

    #region Help Shortcut Tests

    [Fact]
    public async Task KeyboardShortcut_Help_IsRegistered()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert - Verify help shortcut is registered
        KeyboardShortcutServiceMock.Verify(x => x.RegisterShortcut(
            "?",
            It.IsAny<Action>(),
            It.IsAny<string>()), Times.Once,
            "Help shortcut should be registered");
    }

    #endregion

    #region Multiple Shortcut Registration Tests

    [Fact]
    public async Task KeyboardShortcut_AllShortcutsAreRegistered()
    {
        // Arrange
        var registeredKeys = new List<string>();
        KeyboardShortcutServiceMock
            .Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()))
            .Callback<string, Action, string>((key, action, description) =>
            {
                registeredKeys.Add(key);
            });

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100); // Allow async initialization

        // Assert
        registeredKeys.Should().Contain("space", "Play/Pause shortcut should be registered");
        registeredKeys.Should().Contain("r", "Reset shortcut should be registered");
        registeredKeys.Should().Contain("p", "Pomodoro shortcut should be registered");
        registeredKeys.Should().Contain("s", "Short break shortcut should be registered");
        registeredKeys.Should().Contain("l", "Long break shortcut should be registered");
        registeredKeys.Should().Contain("?", "Help shortcut should be registered");
    }

    #endregion
}
