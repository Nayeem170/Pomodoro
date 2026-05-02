using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void SoundEnabled_DefaultIsTrue()
    {
        var settings = CreateDefaultSettings();

        Assert.True(settings.SoundEnabled);
    }

    [Fact]
    public void NotificationsEnabled_DefaultIsTrue()
    {
        var settings = CreateDefaultSettings();

        Assert.True(settings.NotificationsEnabled);
    }

    [Fact]
    public void AutoStartSession_DefaultIsTrue()
    {
        var settings = CreateDefaultSettings();

        Assert.True(settings.AutoStartSession);
    }

    [Fact]
    public void NewInstance_HasAllDefaultValues()
    {
        var settings = new Pomodoro.Web.Models.TimerSettings();

        Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, settings.PomodoroMinutes);
        Assert.Equal(Constants.Timer.DefaultShortBreakMinutes, settings.ShortBreakMinutes);
        Assert.Equal(Constants.Timer.DefaultLongBreakMinutes, settings.LongBreakMinutes);
        Assert.True(settings.SoundEnabled);
        Assert.True(settings.NotificationsEnabled);
        Assert.True(settings.AutoStartSession);
        Assert.Equal(Constants.Timer.DefaultAutoStartDelaySeconds, settings.AutoStartDelaySeconds);
    }

    [Fact]
    public void SoundEnabled_CanBeSetToFalse()
    {
        var settings = CreateDefaultSettings();

        settings.SoundEnabled = false;

        Assert.False(settings.SoundEnabled);
    }

    [Fact]
    public void NotificationsEnabled_CanBeSetToFalse()
    {
        var settings = CreateDefaultSettings();

        settings.NotificationsEnabled = false;

        Assert.False(settings.NotificationsEnabled);
    }

    [Fact]
    public void AutoStartSession_CanBeSetToFalse()
    {
        var settings = CreateDefaultSettings();

        settings.AutoStartSession = false;

        Assert.False(settings.AutoStartSession);
    }

    #endregion
}
