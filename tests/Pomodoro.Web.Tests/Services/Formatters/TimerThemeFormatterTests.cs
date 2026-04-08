using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for TimerThemeFormatter service
/// </summary>
public class TimerThemeFormatterTests
{
    private readonly TimerThemeFormatter _formatter;

    public TimerThemeFormatterTests()
    {
        _formatter = new TimerThemeFormatter();
    }

    [Fact]
    public void GetTimerThemeClass_PomodoroSession_ReturnsPomodoroTheme()
    {
        // Act
        var result = _formatter.GetTimerThemeClass(SessionType.Pomodoro);

        // Assert
        Assert.Equal(Constants.SessionTypes.PomodoroTheme, result);
    }

    [Fact]
    public void GetTimerThemeClass_ShortBreakSession_ReturnsShortBreakTheme()
    {
        // Act
        var result = _formatter.GetTimerThemeClass(SessionType.ShortBreak);

        // Assert
        Assert.Equal(Constants.SessionTypes.ShortBreakTheme, result);
    }

    [Fact]
    public void GetTimerThemeClass_LongBreakSession_ReturnsLongBreakTheme()
    {
        // Act
        var result = _formatter.GetTimerThemeClass(SessionType.LongBreak);

        // Assert
        Assert.Equal(Constants.SessionTypes.LongBreakTheme, result);
    }

    [Fact]
    public void GetTimerThemeClass_UnknownSessionType_ReturnsPomodoroTheme()
    {
        // Act
        var result = _formatter.GetTimerThemeClass((SessionType)999);

        // Assert
        Assert.Equal(Constants.SessionTypes.PomodoroTheme, result);
    }
}
