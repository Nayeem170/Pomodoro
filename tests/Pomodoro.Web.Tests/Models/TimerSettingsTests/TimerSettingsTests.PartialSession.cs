using FluentAssertions;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    [Fact]
    public void Equals_WithDifferentRecordPartialSessions_ReturnsFalse()
    {
        // Arrange
        var settings1 = new TimerSettings { RecordPartialSessions = false };
        var settings2 = new TimerSettings { RecordPartialSessions = true };

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameRecordPartialSessions_ReturnsTrue()
    {
        // Arrange
        var settings1 = new TimerSettings { RecordPartialSessions = true };
        var settings2 = new TimerSettings { RecordPartialSessions = true };

        // Act
        var result = settings1.Equals(settings2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Clone_CopiesRecordPartialSessions()
    {
        // Arrange
        var original = new TimerSettings { RecordPartialSessions = true };

        // Act
        var clone = original.Clone();

        // Assert
        clone.RecordPartialSessions.Should().BeTrue();
    }

    [Fact]
    public void RecordPartialSessions_DefaultsToTrue()
    {
        var settings = new TimerSettings();

        settings.RecordPartialSessions.Should().BeTrue();
    }
}
