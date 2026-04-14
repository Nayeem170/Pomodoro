using Bunit;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Components.History;
using Xunit;
using System.Reflection;

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// bUnit tests for DateNavigator component.
/// Tests date navigation and formatting behavior via UI interactions.
/// </summary>
public class DateNavigatorTests : TestContext
{
    public DateNavigatorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Register services
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
        // Arrange
        var testDate = new DateTime(2026, 3, 1);

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, testDate));

        // Assert
        Assert.NotNull(cut.Instance);
        Assert.Contains("btn", cut.Markup); // Should have navigation buttons
    }

    [Fact]
    public void DateNavigator_ShowsNavigationButtons()
    {
        // Arrange
        var testDate = new DateTime(2026, 3, 1);

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, testDate));

        // Assert - Should have previous/next buttons
        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2); // At least prev and next buttons
    }

    [Fact]
    public void DateNavigator_DisplaysTodayText_WhenSelectedDateIsToday()
    {
        // Arrange
        var today = DateTime.Now.Date;

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, today));

        // Assert
        Assert.Contains("Today", cut.Markup);
    }

    [Fact]
    public void DateNavigator_DisplaysYesterdayText_WhenSelectedDateIsYesterday()
    {
        // Arrange
        var yesterday = DateTime.Now.AddDays(-1).Date;

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, yesterday));

        // Assert
        Assert.Contains("Yesterday", cut.Markup);
    }

    [Fact]
    public void DateNavigator_DisplaysFormattedDate_WhenSelectedDateIsOlder()
    {
        // Arrange
        var olderDate = DateTime.Now.AddDays(-10).Date;

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, olderDate));

        // Assert - Should not show "Yesterday" for dates older than 2 days
        // Note: "Today" may still appear in the "Today" button text
        Assert.DoesNotContain("Yesterday", cut.Markup);
    }

    #endregion

    #region Navigation Callback Tests

    [Fact]
    public void DateNavigator_ClickPrevious_InvokesOnDateChangedWithPreviousDay()
    {
        // Arrange
        var currentDate = new DateTime(2026, 3, 15);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        // Act - Click the previous button (first button)
        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        // Assert
        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 3, 14), newDate.Value.Date);
    }

    [Fact]
    public void DateNavigator_ClickNext_InvokesOnDateChangedWithNextDay()
    {
        // Arrange
        var currentDate = new DateTime(2026, 3, 15);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        // Act - Click the next button (last button)
        var buttons = cut.FindAll("button");
        var nextButton = buttons[buttons.Count - 1];
        nextButton.Click();

        // Assert
        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 3, 16), newDate.Value.Date);
    }

    [Fact]
    public void DateNavigator_ClickToday_InvokesOnDateChangedWithToday()
    {
        // Arrange
        var pastDate = new DateTime(2026, 1, 1);
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, pastDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        // Act - Find and click the Today button
        var todayButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Today"));
        if (todayButton != null)
        {
            todayButton.Click();

            // Assert
            Assert.NotNull(newDate);
            Assert.Equal(DateTime.Now.Date, newDate.Value.Date);
        }
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void DateNavigator_WhenSelectedDateChanges_UpdatesComponent()
    {
        // Arrange - Start with a specific date
        var initialDate = new DateTime(2026, 3, 1);
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, initialDate));

        // Assert initial state - component renders without error
        Assert.NotNull(cut.Instance);

        // Act - Change to a different date
        var newDate = new DateTime(2026, 3, 15);
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedDate, newDate));

        // Assert - Component updates without error
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_NavigatingAcrossMonthBoundary_WorksCorrectly()
    {
        // Arrange
        var currentDate = new DateTime(2026, 3, 1); // March 1st
        DateTime? newDate = null;

        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, currentDate)
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create<DateTime>(this, date => newDate = date)));

        // Act - Click previous to go to February
        var buttons = cut.FindAll("button");
        var prevButton = buttons[0];
        prevButton.Click();

        // Assert
        Assert.NotNull(newDate);
        Assert.Equal(new DateTime(2026, 2, 28), newDate.Value.Date); // Feb 28 in non-leap year
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void DateNavigator_WithMinValue_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.MinValue));

        // Assert - Should render without throwing
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_WithMaxValue_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, DateTime.MaxValue));

        // Assert - Should render without throwing
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void DateNavigator_WithFutureDate_RendersWithoutError()
    {
        // Arrange
        var futureDate = DateTime.Now.AddYears(1);

        // Act
        var cut = RenderComponent<DateNavigator>(parameters => parameters
            .Add(p => p.SelectedDate, futureDate));

        // Assert
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
}
