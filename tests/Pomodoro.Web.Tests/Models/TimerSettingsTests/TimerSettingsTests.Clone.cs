using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Tests for TimerSettings Clone method.
/// </summary>
[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Clone Tests

    [Fact]
    public void Clone_CreatesNewInstance()
    {
        // Arrange
        var original = CreateDefaultSettings();

        // Act
        var clone = original.Clone();

        // Assert
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void Clone_PreservesAllValues()
    {
        // Arrange
        var original = CreateCustomSettings(
            pomodoroMinutes: 35,
            shortBreakMinutes: 8,
            longBreakMinutes: 20,
            soundEnabled: false,
            notificationsEnabled: true,
            autoStartEnabled: false,
            autoStartDelaySeconds: 25);

        // Act
        var clone = original.Clone();

        // Assert
        Assert.Equal(original.PomodoroMinutes, clone.PomodoroMinutes);
        Assert.Equal(original.ShortBreakMinutes, clone.ShortBreakMinutes);
        Assert.Equal(original.LongBreakMinutes, clone.LongBreakMinutes);
        Assert.Equal(original.SoundEnabled, clone.SoundEnabled);
        Assert.Equal(original.NotificationsEnabled, clone.NotificationsEnabled);
        Assert.Equal(original.AutoStartEnabled, clone.AutoStartEnabled);
        Assert.Equal(original.AutoStartDelaySeconds, clone.AutoStartDelaySeconds);
    }

    [Fact]
    public void Clone_ModifyingOriginalDoesNotAffectClone()
    {
        // Arrange
        var original = CreateCustomSettings(pomodoroMinutes: 25, shortBreakMinutes: 10);
        var clone = original.Clone();

        // Act
        original.PomodoroMinutes = 50;
        original.ShortBreakMinutes = 15;
        original.SoundEnabled = false;

        // Assert
        Assert.Equal(25, clone.PomodoroMinutes);
        Assert.Equal(10, clone.ShortBreakMinutes); // Value from CreateCustomSettings
        Assert.True(clone.SoundEnabled); // Value from CreateCustomSettings
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        // Arrange
        var original = CreateCustomSettings(pomodoroMinutes: 25, shortBreakMinutes: 10);
        var clone = original.Clone();

        // Act
        clone.PomodoroMinutes = 50;
        clone.ShortBreakMinutes = 15;
        clone.SoundEnabled = false;

        // Assert
        Assert.Equal(25, original.PomodoroMinutes);
        Assert.Equal(10, original.ShortBreakMinutes); // Value from CreateCustomSettings
        Assert.True(original.SoundEnabled); // Value from CreateCustomSettings
    }

    [Fact]
    public void Clone_OfClonedInstance_HasSameValues()
    {
        // Arrange
        var original = CreateCustomSettings(
            pomodoroMinutes: 40,
            shortBreakMinutes: 10,
            longBreakMinutes: 25,
            soundEnabled: false,
            notificationsEnabled: false,
            autoStartEnabled: true,
            autoStartDelaySeconds: 20);

        // Act
        var clone1 = original.Clone();
        var clone2 = clone1.Clone();

        // Assert
        Assert.Equal(original.PomodoroMinutes, clone2.PomodoroMinutes);
        Assert.Equal(original.ShortBreakMinutes, clone2.ShortBreakMinutes);
        Assert.Equal(original.LongBreakMinutes, clone2.LongBreakMinutes);
        Assert.Equal(original.SoundEnabled, clone2.SoundEnabled);
        Assert.Equal(original.NotificationsEnabled, clone2.NotificationsEnabled);
        Assert.Equal(original.AutoStartEnabled, clone2.AutoStartEnabled);
        Assert.Equal(original.AutoStartDelaySeconds, clone2.AutoStartDelaySeconds);
    }

    #endregion
}

