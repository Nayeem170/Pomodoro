namespace Pomodoro.Web.Services;

public interface ITodayStatsService
{
    int GetTodayTotalFocusMinutes();
    int GetTodayPomodoroCount();
    int GetTodayTasksWorkedOn();
    (int TotalFocusMinutes, int PomodoroCount, int TasksWorkedOn) GetTodayStats();
}