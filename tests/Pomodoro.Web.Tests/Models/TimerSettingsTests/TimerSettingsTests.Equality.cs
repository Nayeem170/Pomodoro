using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Tests for TimerSettings equality, inequality operators, and hash code.
/// </summary>
[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    #region Equals Method Tests

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        var result = settings.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        var result = settings.Equals(settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var settings1 = CreateCustomSettings(30, 10, 20, true, true, true, 15);
        var settings2 = CreateCustomSettings(30, 10, 20, true, true, true, 15);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DefaultInstances_AreEqual()
    {
        // Arrange
        var settings1 = CreateDefaultSettings();
        var settings2 = CreateDefaultSettings();

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentPomodoroMinutes_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(pomodoroMinutes: 25);
        var settings2 = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentShortBreakMinutes_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(shortBreakMinutes: 5);
        var settings2 = CreateCustomSettings(shortBreakMinutes: 10);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentLongBreakMinutes_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(longBreakMinutes: 15);
        var settings2 = CreateCustomSettings(longBreakMinutes: 20);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentSoundEnabled_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(soundEnabled: true);
        var settings2 = CreateCustomSettings(soundEnabled: false);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentNotificationsEnabled_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(notificationsEnabled: true);
        var settings2 = CreateCustomSettings(notificationsEnabled: false);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentAutoStartEnabled_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(autoStartEnabled: true);
        var settings2 = CreateCustomSettings(autoStartEnabled: false);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DifferentAutoStartDelaySeconds_ReturnsFalse()
    {
        // Arrange
        var settings1 = CreateCustomSettings(autoStartDelaySeconds: 10);
        var settings2 = CreateCustomSettings(autoStartDelaySeconds: 15);

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Object.Equals Override Tests

    [Fact]
    public void ObjectEquals_Null_ReturnsFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        var result = settings.Equals((object?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ObjectEquals_DifferentType_ReturnsFalse()
    {
        // Arrange
        var settings = CreateDefaultSettings();

        // Act
        var result = settings.Equals("not a TimerSettings");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ObjectEquals_SameValues_ReturnsTrue()
    {
        // Arrange
        var settings1 = CreateDefaultSettings();
        object settings2 = CreateDefaultSettings();

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Equality Operator Tests

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        // Arrange
        TimerSettings? left = null;
        TimerSettings? right = null;

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualityOperator_LeftNull_ReturnsFalse()
    {
        // Arrange
        TimerSettings? left = null;
        var right = CreateDefaultSettings();

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_RightNull_ReturnsFalse()
    {
        // Arrange
        var left = CreateDefaultSettings();
        TimerSettings? right = null;

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_SameValues_ReturnsTrue()
    {
        // Arrange
        var left = CreateCustomSettings(30, 10, 20);
        var right = CreateCustomSettings(30, 10, 20);

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualityOperator_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var left = CreateCustomSettings(pomodoroMinutes: 25);
        var right = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Inequality Operator Tests

    [Fact]
    public void InequalityOperator_BothNull_ReturnsFalse()
    {
        // Arrange
        TimerSettings? left = null;
        TimerSettings? right = null;

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InequalityOperator_LeftNull_ReturnsTrue()
    {
        // Arrange
        TimerSettings? left = null;
        var right = CreateDefaultSettings();

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_RightNull_ReturnsTrue()
    {
        // Arrange
        var left = CreateDefaultSettings();
        TimerSettings? right = null;

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_SameValues_ReturnsFalse()
    {
        // Arrange
        var left = CreateCustomSettings(30, 10, 20);
        var right = CreateCustomSettings(30, 10, 20);

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        // Arrange
        var left = CreateCustomSettings(pomodoroMinutes: 25);
        var right = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValues_SameHashCode()
    {
        // Arrange
        var settings1 = CreateCustomSettings(30, 10, 20, true, false, true, 15);
        var settings2 = CreateCustomSettings(30, 10, 20, true, false, true, 15);

        // Act
        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DefaultInstances_SameHashCode()
    {
        // Arrange
        var settings1 = CreateDefaultSettings();
        var settings2 = CreateDefaultSettings();

        // Act
        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_DifferentHashCode()
    {
        // Arrange
        var settings1 = CreateCustomSettings(pomodoroMinutes: 25);
        var settings2 = CreateCustomSettings(pomodoroMinutes: 30);

        // Act
        var hash1 = settings1.GetHashCode();
        var hash2 = settings2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ConsistentAcrossMultipleCalls()
    {
        // Arrange
        var settings = CreateCustomSettings(30, 10, 20);

        // Act
        var hash1 = settings.GetHashCode();
        var hash2 = settings.GetHashCode();
        var hash3 = settings.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    #endregion
}

