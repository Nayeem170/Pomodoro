namespace Pomodoro.Web.Models;

public class ExportData
{
    public int Version { get; set; }
    public DateTime ExportDate { get; set; }
    public TimerSettings? Settings { get; set; }
    public List<ActivityRecord>? Activities { get; set; }
    public List<TaskItem>? Tasks { get; set; }
    public List<PomodoroMeta>? PomodoroMeta { get; set; }
    public GoogleTasksSettings? GoogleTasksSettings { get; set; }
}
