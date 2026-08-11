namespace Pomodoro.Web.Models;

public sealed record DemoteRequest(Guid TaskId, Guid TargetSiblingId);
