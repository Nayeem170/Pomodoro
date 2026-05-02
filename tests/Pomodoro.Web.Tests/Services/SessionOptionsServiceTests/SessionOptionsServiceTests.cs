using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SessionOptionsServiceTests;

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
        return new SessionOptionsService(appState, Mock.Of<ITimerService>());
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, 3)]
    [InlineData(SessionType.ShortBreak, 1)]
    [InlineData(SessionType.LongBreak, 1)]
    public void GetOptionsForSessionType_ReturnsCorrectCount(SessionType sessionType, int expectedCount)
    {
        var service = CreateService();

        var options = service.GetOptionsForSessionType(sessionType);

        Assert.Equal(expectedCount, options.Count);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.ShortBreak)]
    [InlineData(SessionType.Pomodoro, SessionType.LongBreak)]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro)]
    [InlineData(SessionType.ShortBreak, SessionType.Pomodoro)]
    [InlineData(SessionType.LongBreak, SessionType.Pomodoro)]
    public void GetOptionsForSessionType_ContainsExpectedOption(SessionType completedType, SessionType expectedOption)
    {
        var service = CreateService();

        var options = service.GetOptionsForSessionType(completedType);

        Assert.Contains(options, o => o.SessionType == expectedOption);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro, true)]
    [InlineData(SessionType.Pomodoro, SessionType.ShortBreak, false)]
    [InlineData(SessionType.Pomodoro, SessionType.LongBreak, false)]
    [InlineData(SessionType.ShortBreak, SessionType.Pomodoro, true)]
    [InlineData(SessionType.LongBreak, SessionType.Pomodoro, true)]
    public void GetOptionsForSessionType_DefaultOptionIsCorrect(
        SessionType completedType, SessionType optionType, bool shouldBeDefault)
    {
        var service = CreateService();

        var options = service.GetOptionsForSessionType(completedType);

        var option = Assert.Single(options, o => o.SessionType == optionType);
        Assert.Equal(shouldBeDefault, option.IsDefault);
    }

    [Fact]
    public void GetOptionsForSessionType_Pomodoro_UsesSettingsDurations()
    {
        var settings = new TimerSettings
        {
            PomodoroMinutes = 30,
            ShortBreakMinutes = 10,
            LongBreakMinutes = 20
        };
        var service = CreateService(settings);

        var options = service.GetOptionsForSessionType(SessionType.Pomodoro);

        Assert.Contains(options, o => o.Duration.Contains("30"));
        Assert.Contains(options, o => o.Duration.Contains("10"));
        Assert.Contains(options, o => o.Duration.Contains("20"));
    }

    [Fact]
    public void GetOptionsForSessionType_UnknownType_ReturnsEmptyList()
    {
        var service = CreateService();

        var options = service.GetOptionsForSessionType((SessionType)999);

        Assert.Empty(options);
    }

    [Theory]
    [InlineData(SessionType.Pomodoro, SessionType.Pomodoro)]
    [InlineData(SessionType.ShortBreak, SessionType.Pomodoro)]
    [InlineData(SessionType.LongBreak, SessionType.Pomodoro)]
    public void GetDefaultOption_ReturnsSameType(SessionType completedType, SessionType expectedDefault)
    {
        var service = CreateService();

        var defaultOption = service.GetDefaultOption(completedType);

        Assert.Equal(expectedDefault, defaultOption);
    }

    [Fact]
    public void GetDefaultOption_UnknownType_ReturnsPomodoro()
    {
        var service = CreateService();

        var defaultOption = service.GetDefaultOption((SessionType)999);

        Assert.Equal(SessionType.Pomodoro, defaultOption);
    }

    [Fact]
    public void GetOptionsForSessionType_WithInterruptedPomodoro_WithSeconds_ShowsMinutesColonSeconds()
    {
        var service = CreateService();
        var interrupted = new TimerSession
        {
            Type = SessionType.Pomodoro,
            RemainingSeconds = 90
        };

        var options = service.GetOptionsForSessionType(SessionType.ShortBreak, interrupted);

        var resumeOption = Assert.Single(options, o => o.IsResume);
        Assert.Contains("1:30", resumeOption.Duration);
    }

    [Fact]
    public void GetOptionsForSessionType_WithInterruptedPomodoro_ZeroSeconds_ShowsMinutesOnly()
    {
        var service = CreateService();
        var interrupted = new TimerSession
        {
            Type = SessionType.Pomodoro,
            RemainingSeconds = 300
        };

        var options = service.GetOptionsForSessionType(SessionType.LongBreak, interrupted);

        var resumeOption = Assert.Single(options, o => o.IsResume);
        Assert.Contains("5m left", resumeOption.Duration);
    }

    [Fact]
    public void GetOptionsForSessionType_WithInterruptedPomodoro_SetsAsDefault()
    {
        var service = CreateService();
        var interrupted = new TimerSession
        {
            Type = SessionType.Pomodoro,
            RemainingSeconds = 300
        };

        var options = service.GetOptionsForSessionType(SessionType.ShortBreak, interrupted);

        var resumeOption = Assert.Single(options, o => o.IsResume);
        Assert.True(resumeOption.IsDefault);
        Assert.All(options.Where(o => !o.IsResume), o => Assert.False(o.IsDefault));
    }
}
