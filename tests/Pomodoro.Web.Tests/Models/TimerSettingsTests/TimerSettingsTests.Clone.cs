using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Clone Tests

    [Fact]
    public void Clone_CreatesNewInstance()
    {
        var original = CreateDefaultSettings();

        var clone = original.Clone();

        Assert.NotSame(original, clone);
    }

    [Fact]
    public void Clone_PreservesAllValues()
    {
        var original = CreateCustomSettings(
            pomodoroMinutes: 35,
            shortBreakMinutes: 8,
            longBreakMinutes: 20,
            soundEnabled: false,
            notificationsEnabled: true,
            autoStartSession: false,
            autoStartDelaySeconds: 25);

        var clone = original.Clone();

        Assert.Equal(original.PomodoroMinutes, clone.PomodoroMinutes);
        Assert.Equal(original.ShortBreakMinutes, clone.ShortBreakMinutes);
        Assert.Equal(original.LongBreakMinutes, clone.LongBreakMinutes);
        Assert.Equal(original.SoundEnabled, clone.SoundEnabled);
        Assert.Equal(original.NotificationsEnabled, clone.NotificationsEnabled);
        Assert.Equal(original.AutoStartSession, clone.AutoStartSession);
        Assert.Equal(original.AutoStartDelaySeconds, clone.AutoStartDelaySeconds);
    }

    [Fact]
    public void Clone_ModifyingOriginalDoesNotAffectClone()
    {
        var original = CreateCustomSettings(pomodoroMinutes: 25, shortBreakMinutes: 10);
        var clone = original.Clone();

        original.PomodoroMinutes = 50;
        original.ShortBreakMinutes = 15;
        original.SoundEnabled = false;

        Assert.Equal(25, clone.PomodoroMinutes);
        Assert.Equal(10, clone.ShortBreakMinutes);
        Assert.True(clone.SoundEnabled);
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        var original = CreateCustomSettings(pomodoroMinutes: 25, shortBreakMinutes: 10);
        var clone = original.Clone();

        clone.PomodoroMinutes = 50;
        clone.ShortBreakMinutes = 15;
        clone.SoundEnabled = false;

        Assert.Equal(25, original.PomodoroMinutes);
        Assert.Equal(10, original.ShortBreakMinutes);
        Assert.True(original.SoundEnabled);
    }

    [Fact]
    public void Clone_OfClonedInstance_HasSameValues()
    {
        var original = CreateCustomSettings(
            pomodoroMinutes: 40,
            shortBreakMinutes: 10,
            longBreakMinutes: 25,
            soundEnabled: false,
            notificationsEnabled: false,
            autoStartSession: true,
            autoStartDelaySeconds: 20);

        var clone1 = original.Clone();
        var clone2 = clone1.Clone();

        Assert.Equal(original.PomodoroMinutes, clone2.PomodoroMinutes);
        Assert.Equal(original.ShortBreakMinutes, clone2.ShortBreakMinutes);
        Assert.Equal(original.LongBreakMinutes, clone2.LongBreakMinutes);
        Assert.Equal(original.SoundEnabled, clone2.SoundEnabled);
        Assert.Equal(original.NotificationsEnabled, clone2.NotificationsEnabled);
        Assert.Equal(original.AutoStartSession, clone2.AutoStartSession);
        Assert.Equal(original.AutoStartDelaySeconds, clone2.AutoStartDelaySeconds);
    }

    #endregion
}
