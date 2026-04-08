using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public partial class ActivityService
{
    private delegate int DailyStatsSelector(DailyStatsCache stats);

    private Dictionary<DateTime, int> GetDailyStats(DateTime from, DateTime to, DailyStatsSelector selector)
    {
        var fromDate = from.Date;
        var toDate = to.Date;

        lock (_cacheLock)
        {
            var result = new Dictionary<DateTime, int>();
            var uncachedDates = new List<DateTime>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (_dailyStatsCache.TryGetValue(date, out var cached))
                {
                    var value = selector(cached);
                    if (value > 0)
                    {
                        result[date] = value;
                    }
                }
                else
                {
                    uncachedDates.Add(date);
                }
            }

            if (uncachedDates.Count > 0)
            {
                ComputeDailyStatsForRange(uncachedDates);

                foreach (var date in uncachedDates)
                {
                    var value = selector(_dailyStatsCache[date]);
                    if (value > 0)
                    {
                        result[date] = value;
                    }
                }
            }

            return result;
        }
    }

    public Dictionary<DateTime, int> GetDailyPomodoroCounts(DateTime from, DateTime to)
        => GetDailyStats(from, to, static s => s.PomodoroCount);

    public Dictionary<DateTime, int> GetDailyFocusMinutes(DateTime from, DateTime to)
        => GetDailyStats(from, to, static s => s.FocusMinutes);

    public Dictionary<DateTime, int> GetDailyBreakMinutes(DateTime from, DateTime to)
        => GetDailyStats(from, to, static s => s.BreakMinutes);
}
