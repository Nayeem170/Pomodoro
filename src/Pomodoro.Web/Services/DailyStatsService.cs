using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class DailyStatsService : IDailyStatsService
{
    private readonly IIndexedDbService _indexedDb;
    private readonly AppState _appState;
    private readonly ILogger<DailyStatsService> _logger;

    public DailyStatsService(IIndexedDbService indexedDb, AppState appState, ILogger<DailyStatsService> logger)
    {
        _indexedDb = indexedDb;
        _appState = appState;
        _logger = logger;
    }

    public async Task InitializeTodayStatsAsync()
    {
        var todayKey = AppState.GetCurrentDayKey().ToString(Constants.DateFormats.IsoFormat);
        var dailyStats = await _indexedDb.GetAsync<DailyStats>(Constants.Storage.DailyStatsStore, todayKey);

        if (dailyStats != null)
        {
            var currentDayKey = AppState.GetCurrentDayKey();
            if (dailyStats.Date == currentDayKey)
            {
                _appState.TodayTotalFocusMinutes = dailyStats.TotalFocusMinutes;
                _appState.TodayPomodoroCount = dailyStats.PomodoroCount;
                _appState.TodayTaskIdsWorkedOn = dailyStats.TaskIdsWorkedOn ?? new List<Guid>();
                _appState.LastResetDate = dailyStats.Date;
            }
            else
            {
                _appState.ResetDailyStats();
            }
        }
        else
        {
            _appState.ResetDailyStats();
        }
    }

    public void CheckAndResetIfNeeded()
    {
        if (_appState.NeedsDailyReset())
        {
            _appState.ResetDailyStats();
        }
    }

    public void RecordPomodoroCompletion(int durationMinutes, Guid taskId)
    {
        _appState.TodayTotalFocusMinutes += durationMinutes;
        _appState.TodayPomodoroCount++;
        _appState.AddTodayTaskId(taskId);
    }
}
