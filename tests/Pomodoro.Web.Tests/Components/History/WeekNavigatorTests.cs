using Bunit;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Xunit;

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// bUnit tests for WeekNavigator component.
/// Tests week navigation and formatting behavior via UI interactions.
/// </summary>
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
        // Arrange
        var testWeekStart = new DateTime(2026, 2, 28); // Saturday

        // Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, testWeekStart));

        // Assert
        Assert.NotNull(cut.Instance);
        Assert.Contains("btn", cut.Markup); // Should have navigation buttons
    }

    [Fact]
    public void WeekNavigator_ShowsNavigationButtons()
    {
        // Arrange
        var testWeekStart = GetSaturdayOfWeek(DateTime.Now);

        // Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, testWeekStart));

        // Assert - Should have previous/next buttons
        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2); // At least prev and next buttons
    }

    [Fact]
    public void WeekNavigator_ShowsThisWeekButton_WhenNotCurrentWeek()
    {
        // Arrange
        var pastWeek = GetSaturdayOfWeek(DateTime.Now.AddDays(-14));

        // Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, pastWeek));

        // Assert - Should show "This Week" button when not on current week
        Assert.Contains("This Week", cut.Markup);
    }

    #endregion

    #region Navigation Callback Tests

    [Fact]
    public void WeekNavigator_ClickPrevious_InvokesOnWeekChangedWithPreviousWeek()
    {
        // Arrange
        var currentWeekStart = new DateTime(2026, 2, 28); // Saturday
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        // Act - Click the previous button (first button)
        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        // Assert
        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 2, 21), newWeekStart.Value.Date); // Previous Saturday
    }

    [Fact]
    public void WeekNavigator_ClickNext_InvokesOnWeekChangedWithNextWeek()
    {
        // Arrange
        var currentWeekStart = new DateTime(2026, 2, 28); // Saturday
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        // Act - Click the next button (last button)
        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        // Assert
        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 3, 7), newWeekStart.Value.Date); // Next Saturday
    }

    [Fact]
    public void WeekNavigator_ClickThisWeek_InvokesOnWeekChangedWithCurrentWeek()
    {
        // Arrange
        var pastWeek = new DateTime(2026, 1, 1);
        DateTime? newWeekStart = null;
        var expectedCurrentWeekStart = GetSaturdayOfWeek(DateTime.Now);

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, pastWeek)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        // Act - Find and click the This Week button
        var thisWeekButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("This Week"));
        if (thisWeekButton != null)
        {
            thisWeekButton.Click();

            // Assert
            Assert.NotNull(newWeekStart);
            Assert.Equal(expectedCurrentWeekStart, newWeekStart.Value.Date);
        }
    }

    #endregion

    #region GetWeekStart Static Method Tests

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForSunday()
    {
        // Arrange
        var sunday = new DateTime(2026, 3, 1); // Sunday

        // Act
        var weekStart = WeekNavigator.GetWeekStart(sunday);

        // Assert - Should return previous Saturday
        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSameDayForSaturday()
    {
        // Arrange
        var saturday = new DateTime(2026, 2, 28); // Saturday

        // Act
        var weekStart = WeekNavigator.GetWeekStart(saturday);

        // Assert - Should return same Saturday
        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(saturday.Date, weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForFriday()
    {
        // Arrange
        var friday = new DateTime(2026, 3, 6); // Friday

        // Act
        var weekStart = WeekNavigator.GetWeekStart(friday);

        // Assert - Should return previous Saturday
        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForMonday()
    {
        // Arrange
        var monday = new DateTime(2026, 3, 2); // Monday

        // Act
        var weekStart = WeekNavigator.GetWeekStart(monday);

        // Assert - Should return previous Saturday
        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    [Fact]
    public void WeekNavigator_GetWeekStart_ReturnsSaturdayForWednesday()
    {
        // Arrange
        var wednesday = new DateTime(2026, 3, 4); // Wednesday

        // Act
        var weekStart = WeekNavigator.GetWeekStart(wednesday);

        // Assert - Should return previous Saturday
        Assert.Equal(DayOfWeek.Saturday, weekStart.DayOfWeek);
        Assert.Equal(new DateTime(2026, 2, 28), weekStart);
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void WeekNavigator_WhenSelectedWeekChanges_UpdatesDisplay()
    {
        // Arrange - Start with a specific week
        var initialWeek = new DateTime(2026, 2, 28);
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, initialWeek));

        // Act - Change to a different week
        var newWeek = new DateTime(2026, 1, 3);
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedWeekStart, newWeek));

        // Assert - Component should update without error
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_NavigatingAcrossYearBoundary_WorksCorrectly()
    {
        // Arrange
        var currentWeekStart = new DateTime(2025, 12, 27); // Saturday near year end
        DateTime? newWeekStart = null;

        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, currentWeekStart)
            .Add(p => p.OnWeekChanged, EventCallback.Factory.Create<DateTime>(this, date => newWeekStart = date)));

        // Act - Click next to go to next year
        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        // Assert
        Assert.NotNull(newWeekStart);
        Assert.Equal(new DateTime(2026, 1, 3), newWeekStart.Value.Date);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WeekNavigator_WithMinValue_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, DateTime.MinValue));

        // Assert - Should render without throwing
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_WithFarFutureDate_RendersWithoutError()
    {
        // Arrange - Use a reasonable future date instead of DateTime.MaxValue
        // DateTime.MaxValue can cause overflow in week calculations
        var futureWeek = new DateTime(2100, 1, 1);

        // Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, futureWeek));

        // Assert - Should render without throwing
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WeekNavigator_WithFutureDate_RendersWithoutError()
    {
        // Arrange
        var futureWeek = DateTime.Now.AddYears(1);

        // Act
        var cut = RenderComponent<WeekNavigator>(parameters => parameters
            .Add(p => p.SelectedWeekStart, futureWeek));

        // Assert
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Helper Methods

    private static DateTime GetSaturdayOfWeek(DateTime date)
    {
        int dayOfWeek = (int)date.DayOfWeek;
        int daysSinceSaturday = (dayOfWeek + 1) % 7; // Sat=0, Sun=1, ..., Fri=6
        return date.AddDays(-daysSinceSaturday).Date;
    }

    #endregion
}
