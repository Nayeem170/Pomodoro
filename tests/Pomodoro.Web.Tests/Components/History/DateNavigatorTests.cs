using Bunit;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Xunit;
using System.Reflection;

namespace Pomodoro.Web.Tests.History;

[Trait("Category", "Component")]
public class DateNavigatorTests : TestContext
{
    public DateNavigatorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var commonServices = TestBase.CreateCommonServiceCollection();
        foreach (var service in commonServices)
        {
            Services.Add(service);
        }
    }

    #region Rendering Tests

    [Fact]
    public void DateNavigator_RendersWithSelectedDate()
    {
        var testDate = new DateTime(2026, 3, 1);

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, testDate));

        Assert.NotNull(cut.Instance);
        Assert.Contains("nav-arr", cut.Markup);
    }

    [Fact]
    public void DateNavigator_ShowsNavigationButtons()
    {
        var testDate = new DateTime(2026, 3, 1);

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, testDate));

        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2);
    }

    [Fact]
    public void DateNavigator_DisplaysTodayText_WhenSelectedDateIsToday()
    {
        var today = DateTime.Now.Date;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, today));

        Assert.Contains("Today", cut.Markup);
    }

    [Fact]
    public void DateNavigator_DisplaysYesterdayText_WhenSelectedDateIsYesterday()
    {
        var yesterday = DateTime.Now.AddDays(-1).Date;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, yesterday));

        Assert.Contains("Yesterday", cut.Markup);
    }

    [Fact]
    public void DateNavigator_DisplaysFormattedDate_WhenSelectedDateIsOlder()
    {
        var olderDate = DateTime.Now.AddDays(-10).Date;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, olderDate));

        Assert.DoesNotContain("Yesterday", cut.Markup);
    }

    #endregion

    #region Navigation Callback Tests

    [Fact]
    public void DateNavigator_ClickPrevious_InvokesOnDateChangedWithPreviousDay()
    {
        var currentDate = new DateTime(2026, 3, 15);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 3, 14), newDate.Value.Date);
    }

    [Fact]
    public void DateNavigator_ClickNext_InvokesOnDateChangedWithNextDay()
    {
        var currentDate = new DateTime(2026, 3, 15);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 3, 16), newDate.Value.Date);
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void DateNavigator_WhenSelectedDateChanges_UpdatesComponent()
    {
        var initialDate = new DateTime(2026, 3, 1);
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, initialDate));

        Assert.NotNull(cut.Instance);

        var newDate = new DateTime(2026, 3, 15);
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedDate, newDate));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_NavigatingAcrossMonthBoundary_WorksCorrectly()
    {
        var currentDate = new DateTime(2026, 3, 1);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 2, 28), newDate.Value.Date);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void DateNavigator_WithMinValue_RendersWithoutError()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.MinValue));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_WithMaxValue_RendersWithoutError()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.MaxValue));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_WithFutureDate_RendersWithoutError()
    {
        var futureDate = DateTime.Now.AddYears(1);

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, futureDate));

        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region FormatDate Tests (Synchronous)

    [Fact]
    public void FormatDate_WhenToday_ReturnsTodayText()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now.Date));

        var method = typeof(DateNavigatorBase).GetMethod("FormatDate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(DateTime) }, null);
        var result = (string)method!.Invoke(cut.Instance, new object[] { DateTime.Now.Date })!;

        Assert.Contains("Today", result);
    }

    [Fact]
    public void FormatDate_WhenYesterday_ReturnsYesterdayText()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now.Date));

        var method = typeof(DateNavigatorBase).GetMethod("FormatDate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(DateTime) }, null);
        var result = (string)method!.Invoke(cut.Instance, new object[] { DateTime.Now.AddDays(-1).Date })!;

        Assert.Contains("Yesterday", result);
    }

    [Fact]
    public void FormatDate_WhenOlderDate_ReturnsFormattedDate()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now.Date));

        var method = typeof(DateNavigatorBase).GetMethod("FormatDate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(DateTime) }, null);
        var olderDate = new DateTime(2025, 6, 15);
        var result = (string)method!.Invoke(cut.Instance, new object[] { olderDate })!;

        Assert.DoesNotContain("Today", result);
        Assert.DoesNotContain("Yesterday", result);
        Assert.Contains("2025", result);
    }

    #endregion

    #region IsToday Property Tests

    [Fact]
    public void IsToday_WhenSelectedDateIsToday_ReturnsTrue()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.Now.Date));

        var property = typeof(DateNavigatorBase).GetProperty("IsToday", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (bool)property!.GetValue(cut.Instance)!;

        Assert.True(result);
    }

    [Fact]
    public void IsToday_WhenSelectedDateIsNotToday_ReturnsFalse()
    {
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, new DateTime(2025, 1, 1)));

        var property = typeof(DateNavigatorBase).GetProperty("IsToday", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var result = (bool)property!.GetValue(cut.Instance)!;

        Assert.False(result);
    }

    #endregion

    #region GoToToday Tests

    [Fact]
    public async Task GoToToday_InvokesCallbackWithLocalDate()
    {
        var expectedToday = DateTime.Now.Date;
        var currentDate = new DateTime(2026, 3, 15);
        DateTime? receivedDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, d => receivedDate = d)));

        var method = typeof(DateNavigatorBase).GetMethod("GoToToday", BindingFlags.Instance | BindingFlags.NonPublic);
        await (Task)method!.Invoke(cut.Instance, null)!;

        Assert.NotNull(receivedDate);
        Assert.Equal(expectedToday, receivedDate.Value);
    }

    #endregion
}
