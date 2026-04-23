using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class HistoryTabsTests : TestContext
{
    public HistoryTabsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void HistoryTabs_ShowsDailyTab()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        Assert.Contains("Daily", cut.Markup);
    }

    [Fact]
    public void HistoryTabs_ShowsWeeklyTab()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        Assert.Contains("Weekly", cut.Markup);
    }

    [Fact]
    public void HistoryTabs_WhenDailyActive_ShowsDailyAsSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        var dailyTab = cut.Find("#daily-tab");
        Assert.Contains("act", dailyTab.GetAttribute("class"));
    }

    [Fact]
    public void HistoryTabs_WhenWeeklyActive_ShowsWeeklyAsSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Weekly));

        // Assert
        var weeklyTab = cut.Find("#weekly-tab");
        Assert.Contains("act", weeklyTab.GetAttribute("class"));
    }

    [Fact]
    public void HistoryTabs_WhenDailyActive_WeeklyNotSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        var weeklyTab = cut.Find("#weekly-tab");
        Assert.DoesNotContain("act", weeklyTab.GetAttribute("class"));
    }

    [Fact]
    public void HistoryTabs_WhenWeeklyActive_DailyNotSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Weekly));

        // Assert
        var dailyTab = cut.Find("#daily-tab");
        Assert.DoesNotContain("act", dailyTab.GetAttribute("class"));
    }

    #endregion

    #region Tab Click Tests

    [Fact]
    public void HistoryTabs_ClickDailyTab_InvokesOnTabChangedCallback()
    {
        // Arrange
        var selectedTab = HistoryTab.Weekly;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Weekly)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, tab => selectedTab = tab)));

        // Act
        cut.Find("#daily-tab").Click();

        // Assert
        Assert.Equal(HistoryTab.Daily, selectedTab);
    }

    [Fact]
    public void HistoryTabs_ClickWeeklyTab_InvokesOnTabChangedCallback()
    {
        // Arrange
        var selectedTab = HistoryTab.Daily;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, tab => selectedTab = tab)));

        // Act
        cut.Find("#weekly-tab").Click();

        // Assert
        Assert.Equal(HistoryTab.Weekly, selectedTab);
    }

    [Fact]
    public void HistoryTabs_ClickActiveTab_DoesNotInvokeCallback()
    {
        // Arrange
        var callbackCount = 0;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, _ => callbackCount++)));

        // Act
        cut.Find("#daily-tab").Click();

        // Assert
        Assert.Equal(0, callbackCount);
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public void HistoryTabs_ArrowRight_SwitchesToNextTab()
    {
        // Arrange
        var selectedTab = HistoryTab.Daily;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, tab => selectedTab = tab)));

        // Act
        cut.Find("#daily-tab").KeyDown("ArrowRight");

        // Assert
        Assert.Equal(HistoryTab.Weekly, selectedTab);
    }

    [Fact]
    public void HistoryTabs_ArrowLeft_SwitchesToPreviousTab()
    {
        // Arrange
        var selectedTab = HistoryTab.Weekly;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Weekly)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, tab => selectedTab = tab)));

        // Act
        cut.Find("#weekly-tab").KeyDown("ArrowLeft");

        // Assert
        Assert.Equal(HistoryTab.Daily, selectedTab);
    }

    [Fact]
    public void HistoryTabs_OtherKey_DoesNotSwitchTab()
    {
        // Arrange
        var callbackCount = 0;
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily)
            .Add(p => p.OnTabChanged, EventCallback.Factory.Create<HistoryTab>(this, _ => callbackCount++)));

        // Act
        cut.Find("#daily-tab").KeyDown("Enter");

        // Assert
        Assert.Equal(0, callbackCount);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void HistoryTabs_HasCorrectAriaRoles()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        var tabList = cut.Find(".vtoggle");
        Assert.Equal("tablist", tabList.GetAttribute("role"));

        var dailyTab = cut.Find("#daily-tab");
        Assert.Equal("tab", dailyTab.GetAttribute("role"));

        var weeklyTab = cut.Find("#weekly-tab");
        Assert.Equal("tab", weeklyTab.GetAttribute("role"));
    }

    [Fact]
    public void HistoryTabs_ActiveTabHasTabIndexZero()
    {
        // Arrange & Act
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert
        var dailyTab = cut.Find("#daily-tab");
        Assert.Equal("0", dailyTab.GetAttribute("tabindex"));

        var weeklyTab = cut.Find("#weekly-tab");
        Assert.Equal("-1", weeklyTab.GetAttribute("tabindex"));
    }

    #endregion

    #region Parameter Change Tests

    [Fact]
    public void HistoryTabs_WhenActiveTabChanges_UpdatesSelection()
    {
        // Arrange - Start with Daily
        var cut = RenderComponent<HistoryTabs>(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Daily));

        // Assert - Daily is active
        Assert.Contains("act", cut.Find("#daily-tab").GetAttribute("class"));

        // Act - Change to Weekly
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.ActiveTab, HistoryTab.Weekly));

        // Assert - Weekly is now active
        Assert.Contains("act", cut.Find("#weekly-tab").GetAttribute("class"));
        Assert.DoesNotContain("act", cut.Find("#daily-tab").GetAttribute("class"));
    }

    #endregion
}
