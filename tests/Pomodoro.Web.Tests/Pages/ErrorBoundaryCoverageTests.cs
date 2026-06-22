using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Component")]
public class HistoryErrorBoundaryTests : TestHelper
{
    public HistoryErrorBoundaryTests()
    {
        ActivityServiceMock.Setup(x => x.GetActivitiesPagedAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivityCountAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        ActivityServiceMock.Setup(x => x.GetDailyFocusMinutes(
                It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        ActivityServiceMock.Setup(x => x.GetDailyBreakMinutes(
                It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        StatisticsServiceMock.Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());
        ActivityServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        InfiniteScrollInteropMock.Setup(x => x.IsSupportedAsync()).ReturnsAsync(false);
    }

    [Fact]
    public void DailyViewErrorBoundary_RendersErrorContent_WhenRenderThrows()
    {
        var mockPresenter = new Mock<HistoryPagePresenterService>(
            new Mock<ILogger<HistoryPagePresenterService>>().Object);
        mockPresenter.Setup(x => x.FormatFocusTime(It.IsAny<int>()))
            .Throws(new Exception("Render error"));
        Services.AddSingleton(mockPresenter.Object);

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Find(".section-error").Should().NotBeNull();
        cut.Find("button.btn-secondary").Should().NotBeNull();
    }

    [Fact]
    public void DailyViewErrorBoundary_RetryButton_CallsRecover()
    {
        var callCount = 0;
        var mockPresenter = new Mock<HistoryPagePresenterService>(
            new Mock<ILogger<HistoryPagePresenterService>>().Object);
        mockPresenter.Setup(x => x.FormatFocusTime(It.IsAny<int>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 1) throw new Exception("Render error");
                return "1h 30m";
            });
        Services.AddSingleton(mockPresenter.Object);

        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        var retryButton = cut.Find("button.btn-secondary");
        retryButton.Click();

        cut.WaitForAssertion(() =>
        {
            var errorSections = cut.FindAll(".section-error");
            errorSections.Should().BeEmpty();
        });
    }

    [Fact]
    public void WeeklyViewErrorBoundary_RendersErrorContent_WhenChildInitThrows()
    {
        ActivityServiceMock.Setup(x => x.GetAllActivities())
            .Throws(new Exception("Init error"));

        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialActiveTab, HistoryTab.Weekly));

        cut.Find(".section-error").Should().NotBeNull();
        cut.Find("button.btn-secondary").Should().NotBeNull();
    }

    [Fact]
    public void WeeklyViewErrorBoundary_RetryButton_CallsRecover()
    {
        var callCount = 0;
        ActivityServiceMock.Setup(x => x.GetAllActivities())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 1) throw new Exception("Init error");
                return new List<ActivityRecord>();
            });

        var cut = RenderComponent<Pomodoro.Web.Pages.History>(parameters =>
            parameters.Add(p => p.InitialActiveTab, HistoryTab.Weekly));

        var retryButton = cut.Find("button.btn-secondary");
        retryButton.Click();

        cut.WaitForAssertion(() =>
        {
            var errorSections = cut.FindAll(".section-error");
            errorSections.Should().BeEmpty();
        });
    }
}
