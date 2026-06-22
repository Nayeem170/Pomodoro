using Bunit;
using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using System.Reflection;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class HistoryCoverageTests : TestHelper
{
    public HistoryCoverageTests()
    {
        ActivityServiceMock.Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        ActivityServiceMock.Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        ActivityServiceMock.Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        StatisticsServiceMock.Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());
        ActivityServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        InfiniteScrollInteropMock.Setup(x => x.IsSupportedAsync()).ReturnsAsync(false);
    }

    [Fact]
    public void UpdateFormattedDate_OtherDate_ShowsOnlyDate()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialSelectedDate, new DateTime(2023, 1, 5)));

        var method = typeof(HistoryBase).GetMethod("UpdateFormattedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        var formattedProp = typeof(HistoryBase).GetProperty("FormattedSelectedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        var todayProp = typeof(HistoryBase).GetProperty("IsSelectedDateToday", BindingFlags.Instance | BindingFlags.NonPublic);
        formattedProp!.GetValue(cut.Instance).Should().Be(new DateTime(2023, 1, 5).ToString("MMM d"));
        todayProp!.GetValue(cut.Instance).Should().Be(false);
    }

    [Fact]
    public void UpdateFormattedDate_Today_ShowsTodayPrefix()
    {
        var today = DateTime.Now.Date;
        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialSelectedDate, today));

        var method = typeof(HistoryBase).GetMethod("UpdateFormattedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        var formattedProp = typeof(HistoryBase).GetProperty("FormattedSelectedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        var todayProp = typeof(HistoryBase).GetProperty("IsSelectedDateToday", BindingFlags.Instance | BindingFlags.NonPublic);
        formattedProp!.GetValue(cut.Instance).Should().Be($"Today, {today:MMM d}");
        todayProp!.GetValue(cut.Instance).Should().Be(true);
    }

    [Fact]
    public void UpdateFormattedDate_Yesterday_ShowsYesterdayPrefix()
    {
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialSelectedDate, yesterday));

        var method = typeof(HistoryBase).GetMethod("UpdateFormattedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(cut.Instance, null);

        var formattedProp = typeof(HistoryBase).GetProperty("FormattedSelectedDate", BindingFlags.Instance | BindingFlags.NonPublic);
        formattedProp!.GetValue(cut.Instance).Should().Be($"Yesterday, {yesterday:MMM d}");
    }

    [Fact]
    public async Task HandleWeekChanged_UpdatesWeekStart()
    {
        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialSelectedWeekStart, new DateTime(2023, 1, 2)));

        await cut.InvokeAsync(() =>
        {
            var method = typeof(HistoryBase).GetMethod("HandleWeekChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            return (Task)method!.Invoke(cut.Instance, [new DateTime(2023, 2, 6)])!;
        });

        var prop = typeof(HistoryBase).GetProperty("SelectedWeekStart", BindingFlags.Instance | BindingFlags.NonPublic);
        prop!.GetValue(cut.Instance).Should().Be(new DateTime(2023, 2, 6));
    }

}
