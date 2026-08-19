using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class TaskGroupingTests
{
    private static TaskItem NewTask(
        string name,
        int sortOrder = 0,
        DateTime? createdAt = null,
        Guid? parentId = null,
        string? googleTaskId = null,
        string? googleParentId = null,
        string? googlePosition = null) => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            SortOrder = sortOrder,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            ParentTaskId = parentId,
            GoogleTaskId = googleTaskId,
            GoogleParentTaskId = googleParentId,
            GooglePosition = googlePosition
        };

    [Fact]
    public void GetOrderedSiblingGroup_Roots_SortsSortOrderAscThenCreatedAtDesc()
    {
        // Arrange
        var mid = NewTask("Mid", sortOrder: 2000, createdAt: new DateTime(2026, 1, 1));
        var tieNewest = NewTask("TieNewest", sortOrder: 1000, createdAt: new DateTime(2026, 3, 1));
        var tieOldest = NewTask("TieOldest", sortOrder: 1000, createdAt: new DateTime(2026, 2, 1));
        var all = new List<TaskItem> { mid, tieNewest, tieOldest };

        // Act
        var ordered = TaskGrouping.GetOrderedSiblingGroup(all, mid);

        // Assert
        ordered.Select(t => t.Name).Should().Equal(["TieNewest", "TieOldest", "Mid"],
            "roots order by SortOrder ascending with newest-first tiebreak");
        ordered.Should().Contain(mid, "the group includes self");
    }

    [Fact]
    public void GetOrderedSiblingGroup_LocalChildren_SortsSortOrderAscThenCreatedAtAsc()
    {
        // Arrange
        var parent = NewTask("Parent");
        var mid = NewTask("Mid", sortOrder: 2000, createdAt: new DateTime(2026, 1, 1), parentId: parent.Id);
        var tieOldest = NewTask("TieOldest", sortOrder: 1000, createdAt: new DateTime(2026, 2, 1), parentId: parent.Id);
        var tieNewest = NewTask("TieNewest", sortOrder: 1000, createdAt: new DateTime(2026, 3, 1), parentId: parent.Id);
        var all = new List<TaskItem> { parent, mid, tieOldest, tieNewest };

        // Act
        var ordered = TaskGrouping.GetOrderedSiblingGroup(all, mid);

        // Assert
        ordered.Select(t => t.Name).Should().Equal(["TieOldest", "TieNewest", "Mid"],
            "children order by SortOrder ascending with oldest-first tiebreak, unlike roots");
    }

    [Fact]
    public void GetOrderedSiblingGroup_GoogleChildren_SortsByGooglePositionOrdinal()
    {
        // Arrange
        var googleParent = NewTask("GP", googleTaskId: "g-parent");
        var childB = NewTask("B", googleParentId: "g-parent", googlePosition: "10");
        var childA = NewTask("A", googleParentId: "g-parent", googlePosition: "2");
        var childNull = NewTask("N", googleParentId: "g-parent", googlePosition: null);
        var all = new List<TaskItem> { googleParent, childB, childA, childNull };

        // Act
        var ordered = TaskGrouping.GetOrderedSiblingGroup(all, childB);

        // Assert
        ordered.Select(t => t.Name).Should().Equal(["N", "B", "A"],
            "Google children sort by GooglePosition ordinal: null/empty first, '10' before '2' because ordinal compares char-by-char");
    }

    [Fact]
    public void GetOrderedSiblingGroup_SingleMember_ReturnsSelfOnly()
    {
        // Arrange
        var solo = NewTask("Solo");

        // Act
        var ordered = TaskGrouping.GetOrderedSiblingGroup(new List<TaskItem> { solo }, solo);

        // Assert
        ordered.Select(t => t.Name).Should().Equal(["Solo"]);
    }

    [Fact]
    public void GetOrderedSiblingGroup_MatchesRenderedRowOrder()
    {
        // Arrange
        var rootA = NewTask("RootA", sortOrder: 2000, createdAt: new DateTime(2026, 1, 1));
        var rootB = NewTask("RootB", sortOrder: 1000, createdAt: new DateTime(2026, 2, 1));
        var parent = NewTask("Parent", sortOrder: 1500, createdAt: new DateTime(2026, 1, 15));
        var childLate = NewTask("ChildLate", sortOrder: 2000, createdAt: new DateTime(2026, 1, 2), parentId: parent.Id);
        var childEarly = NewTask("ChildEarly", sortOrder: 1000, createdAt: new DateTime(2026, 1, 3), parentId: parent.Id);
        var all = new List<TaskItem> { rootA, rootB, parent, childLate, childEarly };

        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = ctx.RenderComponent<TaskList>(parameters => parameters
            .Add(p => p.Tasks, all)
            .Add(p => p.CurrentTaskId, null));

        // Act
        var rootOrder = TaskGrouping.GetOrderedSiblingGroup(all, rootA).Select(t => t.Name).ToList();
        var childOrder = TaskGrouping.GetOrderedSiblingGroup(all, childLate).Select(t => t.Name).ToList();
        var rendered = cut.FindAll(".task-row")
            .Select(r => r.QuerySelector(".task-text")!.TextContent);

        var expected = new List<string>();
        foreach (var rootName in rootOrder)
        {
            expected.Add(rootName);
            if (rootName == parent.Name) expected.AddRange(childOrder);
        }

        // Assert
        rendered.Should().Equal(expected,
            "the helper's ordering must match BuildTree's rendered depth-first sequence");
    }

    [Fact]
    public void OrderRootsForDisplay_SortsBySortOrderThenNewestFirst()
    {
        // Arrange
        var a = NewTask("A", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var b = NewTask("B", sortOrder: 0, createdAt: new DateTime(2026, 1, 2));
        var c = NewTask("C", sortOrder: 0, createdAt: new DateTime(2026, 1, 3));

        // Act
        var ordered = TaskGrouping.OrderRootsForDisplay([a, b, c]).Select(t => t.Name).ToList();

        // Assert - SortOrder ascending, CreatedAt descending within equal sort orders.
        ordered.Should().Equal(["C", "B", "A"]);
    }

    [Fact]
    public void OrderChildrenForDisplay_SortsBySortOrderThenOldestFirst()
    {
        // Arrange
        var a = NewTask("A", sortOrder: 1000, createdAt: new DateTime(2026, 1, 1));
        var b = NewTask("B", sortOrder: 0, createdAt: new DateTime(2026, 1, 2));
        var c = NewTask("C", sortOrder: 0, createdAt: new DateTime(2026, 1, 3));

        // Act
        var ordered = TaskGrouping.OrderChildrenForDisplay([a, b, c]).Select(t => t.Name).ToList();

        // Assert - SortOrder ascending, CreatedAt ascending within equal sort orders.
        ordered.Should().Equal(["B", "C", "A"]);
    }
}
