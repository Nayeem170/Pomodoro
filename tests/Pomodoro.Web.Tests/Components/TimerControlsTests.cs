using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using Pomodoro.Web.Components;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Components;

/// <summary>
/// bUnit tests for TimerControls component.
/// Tests rendering logic and user interactions.
/// </summary>
public class TimerControlsTests : TestContext
{
    public TimerControlsTests()
    {
        // Add JSInterop for Blazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void TimerControls_WhenNotStarted_ShowsStartButton()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.Contains("btn-start", cut.Markup);
        Assert.DoesNotContain("btn-pause", cut.Markup);
        Assert.DoesNotContain("btn-resume", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenRunning_ShowsPauseAndResetButtons()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, true)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.Contains("btn-pause", cut.Markup);
        Assert.Contains("btn-reset", cut.Markup);
        Assert.DoesNotContain("btn-start", cut.Markup);
        Assert.DoesNotContain("btn-resume", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenPaused_ShowsResumeAndResetButtons()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.Contains("btn-resume", cut.Markup);
        Assert.Contains("btn-reset", cut.Markup);
        Assert.DoesNotContain("btn-start", cut.Markup);
        Assert.DoesNotContain("btn-pause", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenCannotStart_DisablesStartButton()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, false)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        var startButton = cut.Find("button.btn-start");
        Assert.True(startButton.HasAttribute("disabled"));
    }

    [Fact]
    public void TimerControls_WhenCannotStart_ShowsTaskHint()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, false)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.Contains("task-hint", cut.Markup);
        Assert.Contains("Select a task to start", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenCanStart_DoesNotShowTaskHint()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.DoesNotContain("task-hint", cut.Markup);
    }

    #endregion

    #region Session Type Styling Tests

    [Fact]
    public void TimerControls_WhenPomodoroSession_AppliesPomodoroClass()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        Assert.Contains(Constants.SessionTypes.PomodoroClass, cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenShortBreakSession_AppliesShortBreakClass()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.ShortBreak));

        // Assert
        Assert.Contains(Constants.SessionTypes.ShortBreakClass, cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenLongBreakSession_AppliesLongBreakClass()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.LongBreak));

        // Assert
        Assert.Contains(Constants.SessionTypes.LongBreakClass, cut.Markup);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public void TimerControls_WhenStartClicked_InvokesOnStartCallback()
    {
        // Arrange
        var onStartInvoked = false;
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnStart, EventCallback.Factory.Create(this, () => onStartInvoked = true)));

        // Act
        cut.Find("button.btn-start").Click();

        // Assert
        Assert.True(onStartInvoked);
    }

    [Fact]
    public void TimerControls_WhenPauseClicked_InvokesOnPauseCallback()
    {
        // Arrange
        var onPauseInvoked = false;
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, true)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnPause, EventCallback.Factory.Create(this, () => onPauseInvoked = true)));

        // Act
        cut.Find("button.btn-pause").Click();

        // Assert
        Assert.True(onPauseInvoked);
    }

    [Fact]
    public void TimerControls_WhenResumeClicked_InvokesOnResumeCallback()
    {
        // Arrange
        var onResumeInvoked = false;
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnResume, EventCallback.Factory.Create(this, () => onResumeInvoked = true)));

        // Act
        cut.Find("button.btn-resume").Click();

        // Assert
        Assert.True(onResumeInvoked);
    }

    [Fact]
    public void TimerControls_WhenResetClickedWhileRunning_InvokesOnResetCallback()
    {
        // Arrange
        var onResetInvoked = false;
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, true)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnReset, EventCallback.Factory.Create(this, () => onResetInvoked = true)));

        // Act
        cut.Find("button.btn-reset").Click();

        // Assert
        Assert.True(onResetInvoked);
    }

    [Fact]
    public void TimerControls_WhenResetClickedWhilePaused_InvokesOnResetCallback()
    {
        // Arrange
        var onResetInvoked = false;
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro)
            .Add(p => p.OnReset, EventCallback.Factory.Create(this, () => onResetInvoked = true)));

        // Act
        cut.Find("button.btn-reset").Click();

        // Assert
        Assert.True(onResetInvoked);
    }

    #endregion

    #region Button Title Tests

    [Fact]
    public void TimerControls_WhenCanStart_ShowsStartTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        var startButton = cut.Find("button.btn-start");
        Assert.Equal("Start", startButton.GetAttribute("title"));
    }

    [Fact]
    public void TimerControls_WhenCannotStart_ShowsSelectTaskTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, false)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        var startButton = cut.Find("button.btn-start");
        Assert.Equal("Select a task first", startButton.GetAttribute("title"));
    }

    [Fact]
    public void TimerControls_WhenRunning_ShowsPauseTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, true)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        var pauseButton = cut.Find("button.btn-pause");
        Assert.Equal("Pause", pauseButton.GetAttribute("title"));
    }

    [Fact]
    public void TimerControls_WhenPaused_ShowsResumeTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, true)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert
        var resumeButton = cut.Find("button.btn-resume");
        Assert.Equal("Resume", resumeButton.GetAttribute("title"));
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void TimerControls_WhenParametersChange_UpdatesRender()
    {
        // Arrange - Start with not started state
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert - Initial state shows start button
        Assert.Contains("btn-start", cut.Markup);

        // Act - Change to running state
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IsRunning, true)
            .Add(p => p.IsStarted, true));

        // Assert - Now shows pause button
        Assert.Contains("btn-pause", cut.Markup);
        Assert.DoesNotContain("btn-start", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenSessionTypeChanges_UpdatesClass()
    {
        // Arrange - Start with Pomodoro
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert - Has Pomodoro class
        Assert.Contains(Constants.SessionTypes.PomodoroClass, cut.Markup);

        // Act - Change to ShortBreak
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SessionType, SessionType.ShortBreak));

        // Assert - Now has ShortBreak class
        Assert.Contains(Constants.SessionTypes.ShortBreakClass, cut.Markup);
        Assert.DoesNotContain(Constants.SessionTypes.PomodoroClass, cut.Markup);
    }

    #endregion

    #region IsPaused Parameter Test

    [Fact]
    public void TimerControls_WhenIsPausedTrue_CanBeRendered()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsPaused, true)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert - Component should render successfully with IsPaused set
        Assert.Contains("btn-start", cut.Markup);
    }

    [Fact]
    public void TimerControls_WhenIsPausedFalse_CanBeRendered()
    {
        // Arrange & Act
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsPaused, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Assert - Component should render successfully with IsPaused set to false
        Assert.Contains("btn-start", cut.Markup);
    }

    #endregion

    #region GetSessionLabel Method Tests (via reflection)

    [Fact]
    public void TimerControls_GetSessionLabel_ReturnsPomodoroDisplayName()
    {
        // Arrange
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.Pomodoro));

        // Act - Call protected method via reflection
        var method = typeof(TimerControlsBase).GetMethod("GetSessionLabel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(cut.Instance, null) as string;

        // Assert
        Assert.Equal(Constants.SessionTypes.PomodoroDisplayName, result);
    }

    [Fact]
    public void TimerControls_GetSessionLabel_ReturnsShortBreakDisplayName()
    {
        // Arrange
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.ShortBreak));

        // Act - Call protected method via reflection
        var method = typeof(TimerControlsBase).GetMethod("GetSessionLabel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(cut.Instance, null) as string;

        // Assert
        Assert.Equal(Constants.SessionTypes.ShortBreakDisplayName, result);
    }

    [Fact]
    public void TimerControls_GetSessionLabel_ReturnsLongBreakDisplayName()
    {
        // Arrange
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, SessionType.LongBreak));

        // Act - Call protected method via reflection
        var method = typeof(TimerControlsBase).GetMethod("GetSessionLabel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(cut.Instance, null) as string;

        // Assert
        Assert.Equal(Constants.SessionTypes.LongBreakDisplayName, result);
    }

    [Fact]
    public void TimerControls_GetSessionLabel_ReturnsEmptyForDefaultCase()
    {
        // Arrange - Use invalid SessionType
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, (SessionType)999));

        // Act - Call protected method via reflection
        var method = typeof(TimerControlsBase).GetMethod("GetSessionLabel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(cut.Instance, null) as string;

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region GetSessionClass Default Branch Test (via reflection)

    [Fact]
    public void TimerControls_GetSessionClass_ReturnsDefaultClassForInvalidSessionType()
    {
        // Arrange - Use invalid SessionType to test default case
        var cut = RenderComponent<TimerControls>(parameters => parameters
            .Add(p => p.IsRunning, false)
            .Add(p => p.IsStarted, false)
            .Add(p => p.CanStart, true)
            .Add(p => p.SessionType, (SessionType)999));

        // Act - Call protected method via reflection
        var method = typeof(TimerControlsBase).GetMethod("GetSessionClass", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(cut.Instance, null) as string;

        // Assert - Default case returns PomodoroClass
        Assert.Equal(Constants.SessionTypes.PomodoroClass, result);
    }

    #endregion
}
