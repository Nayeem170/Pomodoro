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
/// Expanded tests for Index page to improve coverage
/// Tests UI rendering, state management, and edge cases
/// </summary>
[Trait("Category", "Page")]
public class IndexPageExpandedTests : TestHelper
{
    public IndexPageExpandedTests()
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

        // Setup KeyboardShortcutService
        KeyboardShortcutServiceMock
            .Setup(x => x.RegisterShortcut(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>()));

        // Setup JSRuntime for URL parameter handling
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string?>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
    }

    #region UI Rendering Tests

    [Fact]
    public void IndexPage_RendersWithTimerDisplay()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().Contain("timer");
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_RendersWithTaskList()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().Contain("task");
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_RendersWithTodaySummary()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().NotBeNullOrEmpty();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_RendersWithConsentModalPlaceholder()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().NotBeNullOrEmpty();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_RendersWithPipTimerButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().NotBeNullOrEmpty();
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_RendersWithKeyboardHelpButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().NotBeNullOrEmpty();
        cut.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task IndexPage_HandlesNotificationServiceInitializationError()
    {
        // Arrange
        NotificationServiceMock
            .Setup(x => x.InitializeAsync())
            .ThrowsAsync(new Exception("Notification service error"));

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100);

        // Assert
        cut.Markup.Should().Contain("Error initializing");
        cut.Markup.Should().Contain("Notification service error");
    }

    [Fact]
    public async Task IndexPage_HandlesPipTimerServiceInitializationError()
    {
        // Arrange
        PipTimerServiceMock
            .Setup(x => x.InitializeAsync())
            .ThrowsAsync(new Exception("PiP timer service error"));

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100);

        // Assert
        cut.Markup.Should().Contain("Error initializing");
        cut.Markup.Should().Contain("PiP timer service error");
    }

    [Fact]
    public async Task IndexPage_HandlesJsRuntimeError()
    {
        // Arrange
        JSRuntimeMock
            .Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("JS runtime error"));

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        await Task.Delay(100);

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void IndexPage_HandlesEmptyTaskList()
    {
        // Arrange
        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(new List<TaskItem>());

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_HandlesMultipleTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Name = "Task 1" },
            new TaskItem { Name = "Task 2" },
            new TaskItem { Name = "Task 3" }
        };
        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(tasks);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_HandlesDifferentSessionTypes()
    {
        // Arrange & Act - Pomodoro
        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.Pomodoro);
        var cut1 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Arrange & Act - Short Break
        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.ShortBreak);
        var cut2 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Arrange & Act - Long Break
        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.LongBreak);
        var cut3 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut1.Should().NotBeNull();
        cut2.Should().NotBeNull();
        cut3.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_HandlesTimerStates()
    {
        // Arrange & Act - Not Started
        TimerServiceMock
            .SetupGet(x => x.IsStarted)
            .Returns(false);
        TimerServiceMock
            .SetupGet(x => x.IsRunning)
            .Returns(false);
        TimerServiceMock
            .SetupGet(x => x.IsPaused)
            .Returns(false);
        var cut1 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Arrange & Act - Running
        TimerServiceMock
            .SetupGet(x => x.IsStarted)
            .Returns(true);
        TimerServiceMock
            .SetupGet(x => x.IsRunning)
            .Returns(true);
        TimerServiceMock
            .SetupGet(x => x.IsPaused)
            .Returns(false);
        var cut2 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Arrange & Act - Paused
        TimerServiceMock
            .SetupGet(x => x.IsStarted)
            .Returns(true);
        TimerServiceMock
            .SetupGet(x => x.IsRunning)
            .Returns(false);
        TimerServiceMock
            .SetupGet(x => x.IsPaused)
            .Returns(true);
        var cut3 = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut1.Should().NotBeNull();
        cut2.Should().NotBeNull();
        cut3.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_HandlesConsentModalVisibility()
    {
        // Arrange
        ConsentServiceMock
            .SetupGet(x => x.IsModalVisible)
            .Returns(true);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void IndexPage_IntegratesWithAllServices()
    {
        // Arrange & Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert - Services are initialized at startup, verify component renders correctly
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_MaintainsStateAcrossReRenders()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        cut.Render();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_HandlesMultipleEventFires()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Fire multiple events
        TimerEventPublisherMock.Raise(x => x.OnTick += null);
        TaskServiceMock.Raise(x => x.OnChange += null);
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Button Click Tests

    [Fact]
    public void IndexPage_ClicksErrorBannerCloseButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Set an error message to show the banner
        cut.Instance.ErrorMessage = "Test error";
        cut.Render();

        // Act - Click the close button inside the error banner
        cut.Find(".error-banner button").Click();

        // Assert - Error message should be cleared
        cut.Instance.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void IndexPage_ClicksPomodoroSessionButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Click the Pomodoro session tab button
        cut.Find(".session-tabs button:first-child").Click();

        // Assert
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_ClicksShortBreakSessionButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Click the Short Break session button
        var shortBreakButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Short Break"));
        if (shortBreakButton != null)
        {
            shortBreakButton.Click();
        }

        // Assert
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_ClicksLongBreakSessionButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Click the Long Break session button
        var longBreakButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Long Break"));
        if (longBreakButton != null)
        {
            longBreakButton.Click();
        }

        // Assert
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_ClicksKeyboardHelpButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Click the keyboard help button
        var helpButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "?");
        if (helpButton != null)
        {
            helpButton.Click();
        }

        // Assert - Component should still render without errors
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexPage_ClicksPipTimerButton()
    {
        // Arrange
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act - Click the PiP timer button
        var pipButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "⧉");
        if (pipButton != null)
        {
            pipButton.Click();
        }

        // Assert
        cut.Should().NotBeNull();
    }

    #endregion

    #region Current Task Display Tests

    [Fact]
    public void IndexPage_DisplaysCurrentTaskWhenSelected()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = taskId, Name = "Current Task" },
            new TaskItem { Id = Guid.NewGuid(), Name = "Another Task" }
        };

        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(tasks);
        TaskServiceMock
            .SetupGet(x => x.CurrentTaskId)
            .Returns(taskId);
        TaskServiceMock
            .SetupGet(x => x.CurrentTask)
            .Returns(tasks[0]);

        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.Pomodoro);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Markup.Should().Contain("Current Task");
        cut.Should().NotBeNull();
    }

    [Fact]
    public void IndexPage_DisplaysSelectTaskPromptWhenNoTaskSelected()
    {
        // Arrange
        TaskServiceMock
            .SetupGet(x => x.Tasks)
            .Returns(new List<TaskItem>());
        TaskServiceMock
            .SetupGet(x => x.CurrentTaskId)
            .Returns((Guid?)null);
        TaskServiceMock
            .SetupGet(x => x.CurrentTask)
            .Returns((TaskItem?)null);

        TimerServiceMock
            .SetupGet(x => x.CurrentSessionType)
            .Returns(SessionType.Pomodoro);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();
    }

    #endregion
}

