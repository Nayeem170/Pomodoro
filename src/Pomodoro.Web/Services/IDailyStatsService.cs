namespace Pomodoro.Web.Services;

public interface IDailyStatsService
{
    Task InitializeTodayStatsAsync();
    void CheckAndResetIfNeeded();
    void RecordPomodoroCompletion(int durationMinutes, Guid taskId);
}
