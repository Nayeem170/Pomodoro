namespace Pomodoro.Web.Models;

public class GoogleTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Due { get; set; }
    public string Status { get; set; } = "needsAction";
    public string Updated { get; set; } = string.Empty;
    public string? Parent { get; set; }
    public string? Position { get; set; }
    public string? ETag { get; set; }
    public bool Hidden { get; set; }
}

public record GoogleTaskPatch(string? Title = null, string? Notes = null, string? Status = null, string? Due = null);
