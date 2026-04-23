using Bunit;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Xunit;

namespace Pomodoro.Web.Tests.History;

[Trait("Category", "Component")]
public class WeekNavigatorTests : TestContext
{
    public WeekNavigatorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void WeekNavigator_RendersWithSelectedWeekStart()
    {
        var testWeekStart = new DateTime(2026, 2, 28);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, testWeekStart));

        Assert.NotNull(cut.Instance);
        Assert.Contains("nav-arr", cut.Markup);
    }

    [Fact]
    public void WeekNavigator_ShowsNavigationButtons()
    {
        var testWeekStart = GetSaturdayOfWeek(DateTime.Now);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, testWeekStart));

        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2);
    }

    [Fact]
    public void WeekNavigator_DisplaysFormattedWeekRange()
    {
        var testWeekStart = new DateTime(2026, 2, 28);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, testWeekStart));

        Assert.NotNull(cut.Markup);
        Assert.Contains("period-lbl", cut.Markup);
    }

    #endregion

    #region Navigation Callback Tests

    [Fact]
    public void WeekNavigator_ClickPrevious_InvokesOnWeekChangedWithPreviousWeek()
    {
        var currentWeekStart = new DateTime(2026, 2, 28);
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 2, 21), newWeekStart.Value.Date);
    }

    [Fact]
    public void WeekNavigator_ClickNext_InvokesOnWeekChangedWithNextWeek()
    {
        var currentWeekStart = new DateTime(2026, 2, 28);
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 3, 7), newWeekStart.Value.Date);
    }

    #endregion

    #region GetWeekStart Static Method Tests

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForSunday()
    {
        var sunday = new DateTime(2026, 3, 1);

        var weekStart = WeekNavigator.GetWeekStart(sunday);

        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSameDayForSaturday()
    {
        var saturday = new DateTime(2026, 2, 28);

        var weekStart = WeekNavigator.GetWeekStart(saturday);

        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(saturday.Date, weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForFriday()
    {
        var friday = new DateTime(2026, 3, 6);

        var weekStart = WeekNavigator.GetWeekStart(friday);

        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForMonday()
    {
        var monday = new DateTime(2026, 3, 2);

        var weekStart = WeekNavigator.GetWeekStart(monday);

        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForWednesday()
    {
        var wednesday = new DateTime(2026, 3, 4);

        var weekStart = WeekNavigator.GetWeekStart(wednesday);

        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void WeekNavigator_WhenSelectedWeekChanges_UpdatesDisplay()
    {
        var initialWeek = new DateTime(2026, 2, 28);
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, initialWeek));

        var newWeek = new DateTime(2026, 1, 3);
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedWeekStart, newWeek));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_NavigatingAcrossYearBoundary_WorksCorrectly()
    {
        var currentWeekStart = new DateTime(2025, 12, 27);
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 1, 3), newWeekStart.Value.Date);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WeekNavigator_WithMinValue_RendersWithoutError()
    {
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, DateTime.MinValue));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_WithFarFutureDate_RendersWithoutError()
    {
        var futureWeek = new DateTime(2100, 1, 1);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, futureWeek));

        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_WithFutureDate_RendersWithoutError()
    {
        var futureWeek = DateTime.Now.AddYears(1);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, futureWeek));

        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Helper Methods

    private static DateTime GetSaturdayOfWeek(DateTime date)
    {
        int dayOfWeek = (int)date.DayOfWeek;
        int daysSinceSaturday = (dayOfWeek + 1) % 7;
        return date.AddDays(-daysSinceSaturday).Date;
    }

    #endregion
}
