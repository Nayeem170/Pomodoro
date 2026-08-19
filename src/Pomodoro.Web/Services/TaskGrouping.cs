using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public static class TaskGrouping
{
    public sealed record Lookups(
        List<TaskItem> Live,
        Dictionary<Guid, TaskItem> ById,
        Dictionary<string, TaskItem> ByGoogleId);

    public static Lookups BuildLookups(IReadOnlyList<TaskItem> all)
    {
        var live = all.Where(t => !t.IsDeleted).ToList();
        var byId = live.ToDictionary(t => t.Id);
        var byGoogleId = live
            .Where(t => !string.IsNullOrEmpty(t.GoogleTaskId))
            .GroupBy(t => t.GoogleTaskId!)
            .ToDictionary(g => g.Key, g => g.First());
        return new Lookups(live, byId, byGoogleId);
    }

    public static bool HasKnownParent(TaskItem t, Lookups lookups) =>
        (t.ParentTaskId.HasValue && lookups.ById.ContainsKey(t.ParentTaskId.Value)) ||
        (!string.IsNullOrEmpty(t.GoogleParentTaskId) && lookups.ByGoogleId.ContainsKey(t.GoogleParentTaskId));

    public static IReadOnlyList<TaskItem> GetSiblingGroup(IReadOnlyList<TaskItem> all, TaskItem task)
    {
        var lookups = BuildLookups(all);

        if (!HasKnownParent(task, lookups))
            return lookups.Live.Where(t => !HasKnownParent(t, lookups)).ToList();

        if (task.ParentTaskId.HasValue && lookups.ById.ContainsKey(task.ParentTaskId.Value))
            return lookups.Live.Where(t => t.ParentTaskId == task.ParentTaskId).ToList();

        return lookups.Live.Where(t => t.GoogleParentTaskId == task.GoogleParentTaskId).ToList();
    }

    public static IReadOnlyList<TaskItem> GetOrderedSiblingGroup(IReadOnlyList<TaskItem> all, TaskItem task)
    {
        var group = GetSiblingGroup(all, task);
        var lookups = BuildLookups(all);

        if (!HasKnownParent(task, lookups))
            return OrderRootsForDisplay(group);

        if (task.ParentTaskId.HasValue && lookups.ById.ContainsKey(task.ParentTaskId.Value))
            return OrderChildrenForDisplay(group);

        return group
            .OrderBy(t => t.GooglePosition ?? string.Empty, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<TaskItem> OrderRootsForDisplay(IEnumerable<TaskItem> roots) =>
        roots
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToList();

    public static IReadOnlyList<TaskItem> OrderChildrenForDisplay(IEnumerable<TaskItem> children) =>
        children
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .ToList();
}
