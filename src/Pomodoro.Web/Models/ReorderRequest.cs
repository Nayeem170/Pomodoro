namespace Pomodoro.Web.Models;

public record ReorderRequest(Guid TaskId, Guid TargetId, bool InsertBefore);
