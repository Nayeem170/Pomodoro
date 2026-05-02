using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Equals Method Tests

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Equals(null);

        Assert.False(result);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Equals(settings);

        Assert.True(result);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var settings1 = CreateCustomSettings(30, 10, 20, true, true, true, 15);
        var settings2 = CreateCustomSettings(30, 10, 20, true, true, true, 15);

        var result = settings1.Equals(settings2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_DefaultInstances_AreEqual()
    {
        var settings1 = CreateDefaultSettings();
        var settings2 = CreateDefaultSettings();

        var result = settings1.Equals(settings2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentPomodoroMinutes_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(pomodoroMinutes: 25);
        var settings2 = CreateCustomSettings(pomodoroMinutes: 30);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentShortBreakMinutes_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(shortBreakMinutes: 5);
        var settings2 = CreateCustomSettings(shortBreakMinutes: 10);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentLongBreakMinutes_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(longBreakMinutes: 15);
        var settings2 = CreateCustomSettings(longBreakMinutes: 20);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentSoundEnabled_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(soundEnabled: true);
        var settings2 = CreateCustomSettings(soundEnabled: false);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentNotificationsEnabled_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(notificationsEnabled: true);
        var settings2 = CreateCustomSettings(notificationsEnabled: false);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentAutoStartSession_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(autoStartSession: true);
        var settings2 = CreateCustomSettings(autoStartSession: false);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentAutoStartDelaySeconds_ReturnsFalse()
    {
        var settings1 = CreateCustomSettings(autoStartDelaySeconds: 10);
        var settings2 = CreateCustomSettings(autoStartDelaySeconds: 15);

        var result = settings1.Equals(settings2);

        Assert.False(result);
    }

    #endregion

    #region Object.Equals Override Tests

    [Fact]
    public void ObjectEquals_Null_ReturnsFalse()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Equals((object?)null);

        Assert.False(result);
    }

    [Fact]
    public void ObjectEquals_DifferentType_ReturnsFalse()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Equals("not a TimerSettings");

        Assert.False(result);
    }

    [Fact]
    public void ObjectEquals_SameValues_ReturnsTrue()
    {
        var settings1 = CreateDefaultSettings();
        object settings2 = CreateDefaultSettings();

        var result = settings1.Equals(settings2);

        Assert.True(result);
    }

    #endregion

    #region Equality Operator Tests

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        TimerSettings? left = null;
        TimerSettings? right = null;

        var result = left == right;

        Assert.True(result);
    }

    [Fact]
    public void EqualityOperator_LeftNull_ReturnsFalse()
    {
        TimerSettings? left = null;
        var right = CreateDefaultSettings();

        var result = left == right;

        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_RightNull_ReturnsFalse()
    {
        var left = CreateDefaultSettings();
        TimerSettings? right = null;

        var result = left == right;

        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_SameValues_ReturnsTrue()
    {
        var left = CreateCustomSettings(30, 10, 20);
        var right = CreateCustomSettings(30, 10, 20);

        var result = left == right;

        Assert.True(result);
    }

    [Fact]
    public void EqualityOperator_DifferentValues_ReturnsFalse()
    {
        var left = CreateCustomSettings(pomodoroMinutes: 25);
        var right = CreateCustomSettings(pomodoroMinutes: 30);

        var result = left == right;

        Assert.False(result);
    }

    #endregion

    #region Inequality Operator Tests

    [Fact]
    public void InequalityOperator_BothNull_ReturnsFalse()
    {
        TimerSettings? left = null;
        TimerSettings? right = null;

        var result = left != right;

        Assert.False(result);
    }

    [Fact]
    public void InequalityOperator_LeftNull_ReturnsTrue()
    {
        TimerSettings? left = null;
        var right = CreateDefaultSettings();

        var result = left != right;

        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_RightNull_ReturnsTrue()
    {
        var left = CreateDefaultSettings();
        TimerSettings? right = null;

        var result = left != right;

        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_SameValues_ReturnsFalse()
    {
        var left = CreateCustomSettings(30, 10, 20);
        var right = CreateCustomSettings(30, 10, 20);

        var result = left != right;

        Assert.False(result);
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        var left = CreateCustomSettings(pomodoroMinutes: 25);
        var right = CreateCustomSettings(pomodoroMinutes: 30);

        var result = left != right;

        Assert.True(result);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValues_SameHashCode()
    {
        var settings1 = CreateCustomSettings(30, 10, 20, true, false, true, 15);
        var settings2 = CreateCustomSettings(30, 10, 20, true, false, true, 15);

        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DefaultInstances_SameHashCode()
    {
        var settings1 = CreateDefaultSettings();
        var settings2 = CreateDefaultSettings();

        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_DifferentHashCode()
    {
        var settings1 = CreateCustomSettings(pomodoroMinutes: 25);
        var settings2 = CreateCustomSettings(pomodoroMinutes: 30);

        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ConsistentAcrossMultipleCalls()
    {
        var settings = CreateCustomSettings(30, 10, 20);

        var hash1 = settings.GetHashCode();
        var hash2 = settings.GetHashCode();
        var hash3 = settings.GetHashCode();

        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    #endregion
}
