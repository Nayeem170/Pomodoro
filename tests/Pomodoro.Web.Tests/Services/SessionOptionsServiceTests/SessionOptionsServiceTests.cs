using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SessionOptionsServiceTests;

/// <summary>
/// Tests for SessionOptionsService.
/// SessionOptionsService generates session options based on completed session type.
/// </summary>
[Trait("Category", "Service")]
public class SessionOptionsServiceTests
{
    private SessionOptionsService CreateService(TimerSettings? settings = null)
    {
        var appState = new AppState
        {
            Settings = settings ?? new TimerSettings
            {
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15
            }
        };
        return new SessionOptionsService(appState);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, 3)]
    [InlineData(SessionType.ShortBreak, 2)]
    [InlineData(SessionType.LongBreak, 2)]
    public void GetOptionsForSessionType_ReturnsCorrectCount(SessionType sessionType, int expectedCount)
    {
        // Arrange
        var service = CreateService();

        // Act
        var options = service.GetOptionsForSessionType(sessionType);

        // Assert
        Assert.Equal(expectedCount, options.Count);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.ShortBreak)]
    [InlineData(SessionType.Pomodoro, SessionType.LongBreak)]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro)]
    [InlineData(SessionType.ShortBreak, SessionType.Pomodoro)]
    [InlineData(SessionType.ShortBreak, SessionType.ShortBreak)]
    [InlineData(SessionType.LongBreak, SessionType.Pomodoro)]
    [InlineData(SessionType.LongBreak, SessionType.LongBreak)]
    public void GetOptionsForSessionType_ContainsExpectedOption(SessionType completedType, SessionType expectedOption)
    {
        // Arrange
        var service = CreateService();

        // Act
        var options = service.GetOptionsForSessionType(completedType);

        // Assert
        Assert.Contains(options, o => o.SessionType == expectedOption);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro, true)]
    [InlineData(SessionType.Pomodoro, SessionType.ShortBreak, false)]
    [InlineData(SessionType.Pomodoro, SessionType.LongBreak, false)]
    [InlineData(SessionType.ShortBreak, SessionType.ShortBreak, true)]
    [InlineData(SessionType.ShortBreak, SessionType.Pomodoro, false)]
    [InlineData(SessionType.LongBreak, SessionType.LongBreak, true)]
    [InlineData(SessionType.LongBreak, SessionType.Pomodoro, false)]
    public void GetOptionsForSessionType_DefaultOptionIsCorrect(
        SessionType completedType, SessionType optionType, bool shouldBeDefault)
    {
        // Arrange
        var service = CreateService();

        // Act
        var options = service.GetOptionsForSessionType(completedType);

        // Assert
        var option = Assert.Single(options, o => o.SessionType == optionType);
        Assert.Equal(shouldBeDefault, option.IsDefault);
    }

    [Fact]
    public void GetOptionsForSessionType_Pomodoro_UsesSettingsDurations()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 30,
            ShortBreakMinutes = 10,
            LongBreakMinutes = 20
        };
        var service = CreateService(settings);

        // Act
        var options = service.GetOptionsForSessionType(SessionType.Pomodoro);

        // Assert
        Assert.Contains(options, o => o.Duration.Contains("30"));
        Assert.Contains(options, o => o.Duration.Contains("10"));
        Assert.Contains(options, o => o.Duration.Contains("20"));
    }

    [Fact]
    public void GetOptionsForSessionType_UnknownType_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();

        // Act
        var options = service.GetOptionsForSessionType((SessionType)999);

        // Assert
        Assert.Empty(options);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro)]
    [InlineData(SessionType.ShortBreak, SessionType.ShortBreak)]
    [InlineData(SessionType.LongBreak, SessionType.LongBreak)]
    public void GetDefaultOption_ReturnsSameType(SessionType completedType, SessionType expectedDefault)
    {
        // Arrange
        var service = CreateService();

        // Act
        var defaultOption = service.GetDefaultOption(completedType);

        // Assert
        Assert.Equal(expectedDefault, defaultOption);
    }

    [Fact]
    public void GetDefaultOption_UnknownType_ReturnsPomodoro()
    {
        // Arrange
        var service = CreateService();

        // Act
        var defaultOption = service.GetDefaultOption((SessionType)999);

        // Assert
        Assert.Equal(SessionType.Pomodoro, defaultOption);
    }
}

