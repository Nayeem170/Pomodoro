using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Tests for TimerSettings value clamping behavior.
/// Properties use Math.Clamp to ensure values stay within valid ranges.
/// </summary>
public partial class TimerSettingsTests
{
    #region PomodoroMinutes Tests

    [Fact]
    public void PomodoroMinutes_DefaultValue_Is25()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, settings.PomodoroMinutes);
    }

    [Fact]
    public void PomodoroMinutes_ClampsToMinimum_WhenValueIsZero()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.PomodoroMinutes = 0;

        // Assert
        Assert.Equal(Constants.Timer.MinPomodoroMinutes, settings.PomodoroMinutes);
    }

    [Fact]
    public void PomodoroMinutes_ClampsToMinimum_WhenValueIsNegative()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.PomodoroMinutes = -10;

        // Assert
        Assert.Equal(Constants.Timer.MinPomodoroMinutes, settings.PomodoroMinutes);
    }

    [Fact]
    public void PomodoroMinutes_ClampsToMaximum_WhenValueExceedsMax()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.PomodoroMinutes = 200;

        // Assert
        Assert.Equal(Constants.Timer.MaxPomodoroMinutes, settings.PomodoroMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(120)]
    public void PomodoroMinutes_AcceptsValidValues(int value)
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.PomodoroMinutes = value;

        // Assert
        Assert.Equal(value, settings.PomodoroMinutes);
    }

    #endregion

    #region ShortBreakMinutes Tests

    [Fact]
    public void ShortBreakMinutes_DefaultValue_Is5()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.Equal(Constants.Timer.DefaultShortBreakMinutes, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ShortBreakMinutes_ClampsToMinimum_WhenValueIsZero()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.ShortBreakMinutes = 0;

        // Assert
        Assert.Equal(Constants.Timer.MinBreakMinutes, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ShortBreakMinutes_ClampsToMinimum_WhenValueIsNegative()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.ShortBreakMinutes = -5;

        // Assert
        Assert.Equal(Constants.Timer.MinBreakMinutes, settings.ShortBreakMinutes);
    }

    [Fact]
    public void ShortBreakMinutes_ClampsToMaximum_WhenValueExceedsMax()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.ShortBreakMinutes = 100;

        // Assert
        Assert.Equal(Constants.Timer.MaxBreakMinutes, settings.ShortBreakMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(60)]
    public void ShortBreakMinutes_AcceptsValidValues(int value)
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.ShortBreakMinutes = value;

        // Assert
        Assert.Equal(value, settings.ShortBreakMinutes);
    }

    #endregion

    #region LongBreakMinutes Tests

    [Fact]
    public void LongBreakMinutes_DefaultValue_Is15()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.Equal(Constants.Timer.DefaultLongBreakMinutes, settings.LongBreakMinutes);
    }

    [Fact]
    public void LongBreakMinutes_ClampsToMinimum_WhenValueIsZero()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.LongBreakMinutes = 0;

        // Assert
        Assert.Equal(Constants.Timer.MinBreakMinutes, settings.LongBreakMinutes);
    }

    [Fact]
    public void LongBreakMinutes_ClampsToMinimum_WhenValueIsNegative()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.LongBreakMinutes = -20;

        // Assert
        Assert.Equal(Constants.Timer.MinBreakMinutes, settings.LongBreakMinutes);
    }

    [Fact]
    public void LongBreakMinutes_ClampsToMaximum_WhenValueExceedsMax()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.LongBreakMinutes = 100;

        // Assert
        Assert.Equal(Constants.Timer.MaxBreakMinutes, settings.LongBreakMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void LongBreakMinutes_AcceptsValidValues(int value)
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.LongBreakMinutes = value;

        // Assert
        Assert.Equal(value, settings.LongBreakMinutes);
    }

    #endregion

    #region AutoStartDelaySeconds Tests

    [Fact]
    public void AutoStartDelaySeconds_DefaultValue_Is10()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.Equal(Constants.Timer.DefaultAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Fact]
    public void AutoStartDelaySeconds_ClampsToMinimum_WhenValueIsZero()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartDelaySeconds = 0;

        // Assert
        Assert.Equal(Constants.Timer.MinAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Fact]
    public void AutoStartDelaySeconds_ClampsToMinimum_WhenValueIsNegative()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartDelaySeconds = -5;

        // Assert
        Assert.Equal(Constants.Timer.MinAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Fact]
    public void AutoStartDelaySeconds_ClampsToMaximum_WhenValueExceedsMax()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartDelaySeconds = 120;

        // Assert
        Assert.Equal(Constants.Timer.MaxAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    public void AutoStartDelaySeconds_AcceptsValidValues(int value)
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartDelaySeconds = value;

        // Assert
        Assert.Equal(value, settings.AutoStartDelaySeconds);
    }

    #endregion
}
