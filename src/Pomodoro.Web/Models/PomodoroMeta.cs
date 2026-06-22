namespace Pomodoro.Web.Models;

public record PomodoroMeta(string GoogleTaskId, int PomodoroCount, int TotalFocusMinutes, Priority Priority);
