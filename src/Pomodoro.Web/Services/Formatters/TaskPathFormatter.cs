using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Formatters;

public static class TaskPathFormatter
{
    private const int MaxSegments = Constants.Tasks.MaxSubtaskDepth + 1;

    public static string? BuildPath(IReadOnlyList<TaskItem> allTasks, Guid taskId)
        => Join(BuildSegments(allTasks, taskId), Constants.TaskUI.PathSeparator);

    public static string? BuildAriaLabel(IReadOnlyList<TaskItem> allTasks, Guid taskId)
        => Join(BuildSegments(allTasks, taskId), Constants.TaskUI.PathAriaJoiner);

    private static string? Join(IReadOnlyList<string>? segments, string separator)
        => segments is null || segments.Count == 0 ? null : string.Join(separator, segments);

    public static IReadOnlyList<string>? BuildSegments(IReadOnlyList<TaskItem> allTasks, Guid taskId)
    {
        if (allTasks is null || allTasks.Count == 0) return null;

        var byId = new Dictionary<Guid, TaskItem>(allTasks.Count);
        var byGoogleId = new Dictionary<string, TaskItem>();
        foreach (var t in allTasks)
        {
            if (t.Id != Guid.Empty) byId[t.Id] = t;
            if (!string.IsNullOrEmpty(t.GoogleTaskId)) byGoogleId[t.GoogleTaskId] = t;
        }

        if (!byId.TryGetValue(taskId, out var leaf) || leaf is null) return null;

        var segments = new List<string>();
        var visited = new HashSet<Guid>();
        var current = leaf;
        while (current is not null
               && segments.Count < MaxSegments
               && visited.Add(current.Id))
        {
            segments.Add(current.Name);

            var nextId = current.ParentTaskId;
            if (!nextId.HasValue
                && !string.IsNullOrEmpty(current.GoogleParentTaskId)
                && byGoogleId.TryGetValue(current.GoogleParentTaskId, out var googleParent)
                && googleParent is not null)
            {
                nextId = googleParent.Id;
            }

            current = nextId.HasValue && byId.TryGetValue(nextId.Value, out var parent)
                ? parent
                : null;
        }

        segments.Reverse();
        return segments;
    }
}
