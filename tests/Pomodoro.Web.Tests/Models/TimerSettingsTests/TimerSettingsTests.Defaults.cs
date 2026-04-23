using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Tests for TimerSettings default values.
/// </summary>
[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void SoundEnabled_DefaultIsTrue()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.True(settings.SoundEnabled);
    }

    [Fact]
    public void NotificationsEnabled_DefaultIsTrue()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.True(settings.NotificationsEnabled);
    }

    [Fact]
    public void AutoStartPomodoros_DefaultIsTrue()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.True(settings.AutoStartPomodoros);
    }

    [Fact]
    public void AutoStartBreaks_DefaultIsTrue()
    {
        // Arrange & Act
        var settings = CreateDefaultSettings();

        // Assert
        Assert.True(settings.AutoStartBreaks);
    }

    [Fact]
    public void NewInstance_HasAllDefaultValues()
    {
        // Arrange & Act
        var settings = new Pomodoro.Web.Models.TimerSettings();

        // Assert
        Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, settings.PomodoroMinutes);
        Assert.Equal(Constants.Timer.DefaultShortBreakMinutes, settings.ShortBreakMinutes);
        Assert.Equal(Constants.Timer.DefaultLongBreakMinutes, settings.LongBreakMinutes);
        Assert.True(settings.SoundEnabled);
        Assert.True(settings.NotificationsEnabled);
        Assert.True(settings.AutoStartPomodoros);
        Assert.True(settings.AutoStartBreaks);
        Assert.Equal(Constants.Timer.DefaultAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Fact]
    public void SoundEnabled_CanBeSetToFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.SoundEnabled = false;

        // Assert
        Assert.False(settings.SoundEnabled);
    }

    [Fact]
    public void NotificationsEnabled_CanBeSetToFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.NotificationsEnabled = false;

        // Assert
        Assert.False(settings.NotificationsEnabled);
    }

    [Fact]
    public void AutoStartPomodoros_CanBeSetToFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartPomodoros = false;

        // Assert
        Assert.False(settings.AutoStartPomodoros);
    }

    [Fact]
    public void AutoStartBreaks_CanBeSetToFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        settings.AutoStartBreaks = false;

        // Assert
        Assert.False(settings.AutoStartBreaks);
    }

    #endregion
}

