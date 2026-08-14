namespace Pomodoro.Web.Models;

public sealed record ReorderRequest(Guid TaskId, Guid TargetId, bool InsertBefore);
