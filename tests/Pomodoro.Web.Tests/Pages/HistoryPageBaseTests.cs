using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Tests;
using Xunit;
using System.Reflection;
using Bunit;

/// <summary>
/// Tests for History.razor.cs (HistoryBase class)
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageBaseTests
{
    /// <summary>
    /// Testable wrapper for HistoryBase to expose protected members
    /// </summary>
    public class TestableHistoryBase : Pomodoro.Web.Pages.HistoryBase
    {
        // Expose services for testing
        public new IActivityService ActivityService
        {
            get => base.ActivityService;
            set => base.ActivityService = value;
        }

        public new IJSRuntime JSRuntime
        {
            get => base.JSRuntime;
            set => base.JSRuntime = value;
        }

        public new IInfiniteScrollInterop InfiniteScrollInterop
        {
            get => base.InfiniteScrollInterop;
            set => base.InfiniteScrollInterop = value;
        }

        public new ILogger<Pomodoro.Web.Pages.HistoryBase> Logger
        {
            get => base.Logger;
            set => base.Logger = value;
        }

        public new IHistoryStatsService HistoryStatsService
        {
            get => base.HistoryStatsService;
            set => base.HistoryStatsService = value;
        }

        public new HistoryPagePresenterService HistoryPagePresenterService
        {
            get => base.HistoryPagePresenterService;
            set => base.HistoryPagePresenterService = value;
        }

        public new string FormatFocusTime(int minutes) => base.FormatFocusTime(minutes);

        // Expose protected state for testing
        public new DateTime SelectedDate
        {
            get => base.SelectedDate;
            set => base.SelectedDate = value;
        }

        public new DateTime SelectedWeekStart
        {
            get => base.SelectedWeekStart;
            set => base.SelectedWeekStart = value;
        }

        public new HistoryTab ActiveTab
        {
            get => base.ActiveTab;
            set => base.ActiveTab = value;
        }

        public new List<ActivityRecord> CurrentActivities
        {
            get => base.CurrentActivities;
            set => base.CurrentActivities = value;
        }

        public new DailyStatsSummary CurrentStats
        {
            get => base.CurrentStats;
            set => base.CurrentStats = value;
        }

        public new WeeklyStats? WeeklyStats
        {
            get => base.WeeklyStats;
            set => base.WeeklyStats = value;
        }

        public new Dictionary<DateTime, int> WeeklyFocusMinutes
        {
            get => base.WeeklyFocusMinutes;
            set => base.WeeklyFocusMinutes = value;
        }

        public new Dictionary<DateTime, int> WeeklyBreakMinutes
        {
            get => base.WeeklyBreakMinutes;
            set => base.WeeklyBreakMinutes = value;
        }

        public new int CurrentSkip
        {
            get => base.CurrentSkip;
            set => base.CurrentSkip = value;
        }


    }
}

