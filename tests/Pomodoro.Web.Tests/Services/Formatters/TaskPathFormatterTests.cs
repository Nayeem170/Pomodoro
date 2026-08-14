using FluentAssertions;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Services.Formatters;

[Trait("Category", "Service")]
public class TaskPathFormatterTests
{
    private static TaskItem Task(Guid id, string name, Guid? parentId = null,
        string? googleId = null, string? googleParentId = null) => new()
        {
            Id = id,
            Name = name,
            ParentTaskId = parentId,
            GoogleTaskId = googleId,
            GoogleParentTaskId = googleParentId
        };

    [Fact]
    public void BuildPath_RootTask_ReturnsLeafOnly()
    {
        // Arrange
        var root = Task(Guid.NewGuid(), "root");

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { root }, root.Id);

        // Assert
        path.Should().Be("root");
    }

    [Fact]
    public void BuildPath_ThreeDeepLocalParent_ReturnsFullPath()
    {
        // Arrange
        var a = Task(Guid.NewGuid(), "a");
        var b = Task(Guid.NewGuid(), "b", parentId: a.Id);
        var c = Task(Guid.NewGuid(), "c", parentId: b.Id);

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { a, b, c }, c.Id);

        // Assert
        path.Should().Be("a / b / c");
    }

    [Fact]
    public void BuildPath_GoogleParentedNoLocalGuid_ResolvesAncestor()
    {
        // Arrange: child parented only via GoogleParentTaskId; parent matched by GoogleTaskId
        var parent = Task(Guid.NewGuid(), "parent", googleId: "g-parent");
        var child = Task(Guid.NewGuid(), "child", googleParentId: "g-parent");

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { parent, child }, child.Id);

        // Assert
        path.Should().Be("parent / child");
    }

    [Fact]
    public void BuildPath_MixedLocalAndGoogleEdges_ResolvesFullChain()
    {
        // Arrange: leaf -> local parent -> that parent via Google edge
        var gp = Task(Guid.NewGuid(), "gp", googleId: "g-gp");
        var mid = Task(Guid.NewGuid(), "mid", googleParentId: "g-gp");
        var leaf = Task(Guid.NewGuid(), "leaf", parentId: mid.Id);

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { gp, mid, leaf }, leaf.Id);

        // Assert
        path.Should().Be("gp / mid / leaf");
    }

    [Fact]
    public void BuildPath_CycleInParentLinks_DoesNotLoop()
    {
        // Arrange: A.Parent = B, B.Parent = A (cycle)
        var aId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        var a = Task(aId, "A", parentId: bId);
        var b = Task(bId, "B", parentId: aId);

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { a, b }, aId);

        // Assert
        path.Should().Be("B / A");
    }

    [Fact]
    public void BuildPath_DeepChain_BoundedToMaxSubtaskDepthPlus1()
    {
        // Arrange: a 6-deep chain (exceeds MaxSubtaskDepth=4 -> MaxSegments=5)
        var ids = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        var tasks = new List<TaskItem>
        {
            Task(ids[0], "n0"),
            Task(ids[1], "n1", parentId: ids[0]),
            Task(ids[2], "n2", parentId: ids[1]),
            Task(ids[3], "n3", parentId: ids[2]),
            Task(ids[4], "n4", parentId: ids[3]),
            Task(ids[5], "n5", parentId: ids[4])
        };

        // Act
        var path = TaskPathFormatter.BuildPath(tasks, ids[5]);

        // Assert
        var segments = path!.Split(" / ");
        segments.Length.Should().Be(5);
        segments.Last().Should().Be("n5");
    }

    [Fact]
    public void BuildPath_UnknownTaskId_ReturnsEmpty()
    {
        // Arrange
        var root = Task(Guid.NewGuid(), "root");

        // Act
        var path = TaskPathFormatter.BuildPath(new[] { root }, Guid.NewGuid());

        // Assert
        path.Should().BeNullOrEmpty();
    }

    [Fact]
    public void BuildPath_EmptyTaskList_ReturnsEmpty()
    {
        // Act
        var path = TaskPathFormatter.BuildPath(Array.Empty<TaskItem>(), Guid.NewGuid());

        // Assert
        path.Should().BeNullOrEmpty();
    }

    [Fact]
    public void BuildAriaLabel_ThreeDeep_ReturnsUnderJoined()
    {
        // Arrange
        var a = Task(Guid.NewGuid(), "a");
        var b = Task(Guid.NewGuid(), "b", parentId: a.Id);
        var c = Task(Guid.NewGuid(), "c", parentId: b.Id);

        // Act
        var aria = TaskPathFormatter.BuildAriaLabel(new[] { a, b, c }, c.Id);

        // Assert
        aria.Should().Be("a under b under c");
    }

    [Fact]
    public void BuildSegments_ThreeDeep_ReturnsRootToLeafOrdered()
    {
        // Arrange
        var a = Task(Guid.NewGuid(), "a");
        var b = Task(Guid.NewGuid(), "b", parentId: a.Id);
        var c = Task(Guid.NewGuid(), "c", parentId: b.Id);

        // Act
        var segments = TaskPathFormatter.BuildSegments(new[] { a, b, c }, c.Id);

        // Assert
        segments.Should().NotBeNull();
        segments!.Count.Should().Be(3);
        segments[0].Should().Be("a");
        segments[^1].Should().Be("c");
    }
}
