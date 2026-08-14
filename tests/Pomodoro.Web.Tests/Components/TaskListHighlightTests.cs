using Bunit;
using FluentAssertions;
using Pomodoro.Web.Components.Tasks;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests;

[Trait("Category", "Component")]
public class TaskListHighlightTests : TestContext
{
    public TaskListHighlightTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void HighlightTaskId_ThreadsIsNewlyAdded_ToMatchingRow()
    {
        // Arrange
        var highlight = new TaskItem { Id = Guid.NewGuid(), Name = "New one", CreatedAt = DateTime.UtcNow };
        var other = new TaskItem { Id = Guid.NewGuid(), Name = "Other", CreatedAt = DateTime.UtcNow };

        // Act
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem> { highlight, other })
            .Add(x => x.HighlightTaskId, highlight.Id));

        // Assert
        cut.FindAll(".task-row--new").Should().HaveCount(1);
        cut.Find(".task-row--new").TextContent.Should().Contain("New one");
    }

    [Fact]
    public void HighlightTaskId_Null_NoRowHighlighted()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Plain", CreatedAt = DateTime.UtcNow };

        // Act
        var cut = RenderComponent<TaskList>(p => p
            .Add(x => x.Tasks, new List<TaskItem> { task })
            .Add(x => x.HighlightTaskId, (Guid?)null));

        // Assert
        cut.FindAll(".task-row--new").Should().BeEmpty();
    }
}
