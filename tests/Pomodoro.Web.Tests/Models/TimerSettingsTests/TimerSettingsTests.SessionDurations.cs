using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Tests for TimerSettings session duration methods.
/// </summary>
public partial class TimerSettingsTests
{
    #region GetDurationMinutes Tests

    [Fact]
    public void GetDurationMinutes_Pomodoro_ReturnsPomodoroMinutes()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var duration = settings.GetDurationMinutes(SessionType.Pomodoro);

        // Assert
        Assert.Equal(30, duration);
    }

    [Fact]
    public void GetDurationMinutes_ShortBreak_ReturnsShortBreakMinutes()
    {
        // Arrange
        var settings = CreateCustomSettings(shortBreakMinutes: 10);

        // Act
        var duration = settings.GetDurationMinutes(SessionType.ShortBreak);

        // Assert
        Assert.Equal(10, duration);
    }

    [Fact]
    public void GetDurationMinutes_LongBreak_ReturnsLongBreakMinutes()
    {
        // Arrange
        var settings = CreateCustomSettings(longBreakMinutes: 20);

        // Act
        var duration = settings.GetDurationMinutes(SessionType.LongBreak);

        // Assert
        Assert.Equal(20, duration);
    }

    [Fact]
    public void GetDurationMinutes_UnknownType_ReturnsPomodoroMinutes()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 45);

        // Act
        var duration = settings.GetDurationMinutes((SessionType)999);

        // Assert
        Assert.Equal(45, duration);
    }

    [Fact]
    public void GetDurationMinutes_UsesClampedValues()
    {
        // Arrange
        var settings = new TimerSettings();
        settings.PomodoroMinutes = 200; // Will be clamped to 120

        // Act
        var duration = settings.GetDurationMinutes(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Constants.Timer.MaxPomodoroMinutes, duration);
    }

    #endregion

    #region GetDurationSeconds Tests

    [Fact]
    public void GetDurationSeconds_Pomodoro_ReturnsPomodoroMinutesTimes60()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 25);

        // Act
        var duration = settings.GetDurationSeconds(SessionType.Pomodoro);

        // Assert
        Assert.Equal(25 * 60, duration);
    }

    [Fact]
    public void GetDurationSeconds_ShortBreak_ReturnsShortBreakMinutesTimes60()
    {
        // Arrange
        var settings = CreateCustomSettings(shortBreakMinutes: 5);

        // Act
        var duration = settings.GetDurationSeconds(SessionType.ShortBreak);

        // Assert
        Assert.Equal(5 * 60, duration);
    }

    [Fact]
    public void GetDurationSeconds_LongBreak_ReturnsLongBreakMinutesTimes60()
    {
        // Arrange
        var settings = CreateCustomSettings(longBreakMinutes: 15);

        // Act
        var duration = settings.GetDurationSeconds(SessionType.LongBreak);

        // Assert
        Assert.Equal(15 * 60, duration);
    }

    [Fact]
    public void GetDurationSeconds_UnknownType_ReturnsPomodoroMinutesTimes60()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var duration = settings.GetDurationSeconds((SessionType)999);

        // Assert
        Assert.Equal(30 * 60, duration);
    }

    [Fact]
    public void GetDurationSeconds_ConversionIsCorrect()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 1);

        // Act
        var duration = settings.GetDurationSeconds(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Constants.TimeConversion.SecondsPerMinute, duration);
    }

    [Fact]
    public void GetDurationSeconds_MatchesGetDurationMinutesTimes60()
    {
        // Arrange
        var settings = CreateCustomSettings(pomodoroMinutes: 25, shortBreakMinutes: 5, longBreakMinutes: 15);

        // Act & Assert
        Assert.Equal(
            settings.GetDurationMinutes(SessionType.Pomodoro) * Constants.TimeConversion.SecondsPerMinute,
            settings.GetDurationSeconds(SessionType.Pomodoro));
        Assert.Equal(
            settings.GetDurationMinutes(SessionType.ShortBreak) * Constants.TimeConversion.SecondsPerMinute,
            settings.GetDurationSeconds(SessionType.ShortBreak));
        Assert.Equal(
            settings.GetDurationMinutes(SessionType.LongBreak) * Constants.TimeConversion.SecondsPerMinute,
            settings.GetDurationSeconds(SessionType.LongBreak));
    }

    #endregion
}
