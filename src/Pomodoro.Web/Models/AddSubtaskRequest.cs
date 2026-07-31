namespace Pomodoro.Web.Models;

public sealed record AddSubtaskRequest(Guid ParentTaskId, string Name);
