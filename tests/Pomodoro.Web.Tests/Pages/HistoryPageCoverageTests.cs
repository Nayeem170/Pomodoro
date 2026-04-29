using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class HistoryPageCoverageTests : TestHelper
{
    private void SetupHistoryMocks()
    {
        ActivityServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        ActivityServiceMock.Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);
        ActivityServiceMock.Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        ActivityServiceMock.Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        StatisticsServiceMock.Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);
    }

    [Fact]
    public void HistoryPage_RendersDailyView()
    {
        SetupHistoryMocks();

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Markup.Should().Contain("hist-body");
        cut.Markup.Should().Contain("Today");
    }

    [Fact]
    public void HistoryPage_RendersWeeklyView()
    {
        SetupHistoryMocks();

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        cut.Find("#weekly-tab").Click();

        cut.Markup.Should().Contain("This week");
    }

    [Fact]
    public void HistoryPage_SwitchesBetweenTabs()
    {
        SetupHistoryMocks();

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        cut.Find("#weekly-tab").Click();
        cut.Markup.Should().Contain("This week");

        cut.Find("#daily-tab").Click();
        cut.Markup.Should().Contain("Today");
    }

    [Fact]
    public void HistoryPage_RendersDateNavigation()
    {
        SetupHistoryMocks();

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Markup.Should().Contain("nav-arr");
        cut.Markup.Should().Contain("Previous day");
        cut.Markup.Should().Contain("Next day");
    }

    [Fact]
    public void HistoryPage_RendersWeekNavigation()
    {
        SetupHistoryMocks();

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        cut.Find("#weekly-tab").Click();

        cut.Markup.Should().Contain("Previous week");
        cut.Markup.Should().Contain("Next week");
    }
}
