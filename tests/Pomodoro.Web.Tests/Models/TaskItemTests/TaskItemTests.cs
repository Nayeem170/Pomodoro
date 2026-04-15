using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

/// <summary>
/// Tests for TaskItem model
/// </summary>
[Trait("Category", "Model")]
public class TaskItemTests
{
    [Trait("Category", "Model")]
    public class Constructor : TaskItemTests
    {
        [Fact]
        public void ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.NotEqual(Guid.Empty, task.Id);
            Assert.Equal(string.Empty, task.Name);
            Assert.False(task.IsCompleted);
            Assert.Equal(0, task.TotalFocusMinutes);
            Assert.Equal(0, task.PomodoroCount);
            Assert.Null(task.LastWorkedOn);
            Assert.False(task.IsDeleted);
            Assert.Null(task.DeletedAt);
        }

        [Fact]
        public void ShouldGenerateUniqueIds()
        {
            // Arrange & Act
            var task1 = new TaskItem();
            var task2 = new TaskItem();

            // Assert
            Assert.NotEqual(task1.Id, task2.Id);
        }

        [Fact]
        public void ShouldSetCreatedAtToCurrentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var task = new TaskItem();
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(task.CreatedAt >= beforeCreation);
            Assert.True(task.CreatedAt <= afterCreation);
        }
    }

    [Trait("Category", "Model")]
    public class Name : TaskItemTests
    {
        [Theory]
        [InlineData("Test Task")]
        [InlineData("A")]
        [InlineData("Task with special characters !@#$%")]
        [InlineData("Task with numbers 123")]
        public void ShouldSetAndGetTaskName(string name)
        {
            // Arrange & Act
            var task = new TaskItem { Name = name };

            // Assert
            Assert.Equal(name, task.Name);
        }

        [Fact]
        public void ShouldAllowEmptyName()
        {
            // Arrange & Act
            var task = new TaskItem { Name = "" };

            // Assert
            Assert.Equal("", task.Name);
        }

        [Fact]
        public void ShouldAllowLongName()
        {
            // Arrange
            var longName = new string('A', 200);

            // Act
            var task = new TaskItem { Name = longName };

            // Assert
            Assert.Equal(longName, task.Name);
        }
    }

    [Trait("Category", "Model")]
    public class CompletionStatus : TaskItemTests
    {
        [Fact]
        public void ShouldDefaultToNotCompleted()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public void ShouldSetIsCompleted()
        {
            // Arrange & Act
            var task = new TaskItem { IsCompleted = true };

            // Assert
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void ShouldToggleCompletion()
        {
            // Arrange
            var task = new TaskItem { IsCompleted = false };

            // Act
            task.IsCompleted = true;

            // Assert
            Assert.True(task.IsCompleted);
        }
    }

    [Trait("Category", "Model")]
    public class FocusTracking : TaskItemTests
    {
        [Fact]
        public void ShouldDefaultToZeroFocusMinutes()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.Equal(0, task.TotalFocusMinutes);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(100)]
        [InlineData(1000)]
        public void ShouldSetAndGetTotalFocusMinutes(int minutes)
        {
            // Arrange & Act
            var task = new TaskItem { TotalFocusMinutes = minutes };

            // Assert
            Assert.Equal(minutes, task.TotalFocusMinutes);
        }

        [Fact]
        public void ShouldIncrementFocusMinutes()
        {
            // Arrange
            var task = new TaskItem { TotalFocusMinutes = 50 };

            // Act
            task.TotalFocusMinutes += 25;

            // Assert
            Assert.Equal(75, task.TotalFocusMinutes);
        }
    }

    [Trait("Category", "Model")]
    public class PomodoroCount : TaskItemTests
    {
        [Fact]
        public void ShouldDefaultToZeroPomodoros()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.Equal(0, task.PomodoroCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public void ShouldSetAndGetPomodoroCount(int count)
        {
            // Arrange & Act
            var task = new TaskItem { PomodoroCount = count };

            // Assert
            Assert.Equal(count, task.PomodoroCount);
        }

        [Fact]
        public void ShouldIncrementPomodoroCount()
        {
            // Arrange
            var task = new TaskItem { PomodoroCount = 3 };

            // Act
            task.PomodoroCount++;

            // Assert
            Assert.Equal(4, task.PomodoroCount);
        }
    }

    [Trait("Category", "Model")]
    public class LastWorkedOn : TaskItemTests
    {
        [Fact]
        public void ShouldDefaultToNull()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.Null(task.LastWorkedOn);
        }

        [Fact]
        public void ShouldSetAndGetLastWorkedOn()
        {
            // Arrange
            var lastWorked = DateTime.UtcNow.AddHours(-2);

            // Act
            var task = new TaskItem { LastWorkedOn = lastWorked };

            // Assert
            Assert.Equal(lastWorked, task.LastWorkedOn);
        }
    }

    [Trait("Category", "Model")]
    public class SoftDelete : TaskItemTests
    {
        [Fact]
        public void ShouldDefaultToNotDeleted()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.False(task.IsDeleted);
            Assert.Null(task.DeletedAt);
        }

        [Fact]
        public void ShouldSetIsDeleted()
        {
            // Arrange
            var task = new TaskItem { IsDeleted = true, DeletedAt = DateTime.UtcNow };

            // Assert
            Assert.True(task.IsDeleted);
            Assert.NotNull(task.DeletedAt);
        }

        [Fact]
        public void ShouldSetDeletedAtWhenDeleted()
        {
            // Arrange
            var beforeDelete = DateTime.UtcNow.AddSeconds(-1);
            var task = new TaskItem();

            // Act
            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            var afterDelete = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(task.DeletedAt >= beforeDelete);
            Assert.True(task.DeletedAt <= afterDelete);
        }
    }

    [Trait("Category", "Model")]
    public class AllProperties : TaskItemTests
    {
        [Fact]
        public void ShouldSetAllProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var lastWorkedOn = new DateTime(2024, 1, 16, 14, 0, 0, DateTimeKind.Utc);
            var deletedAt = new DateTime(2024, 1, 17, 9, 0, 0, DateTimeKind.Utc);

            // Act
            var task = new TaskItem
            {
                Id = id,
                Name = "Complete Task Item Tests",
                CreatedAt = createdAt,
                IsCompleted = true,
                TotalFocusMinutes = 150,
                PomodoroCount = 6,
                LastWorkedOn = lastWorkedOn,
                IsDeleted = true,
                DeletedAt = deletedAt
            };

            // Assert
            Assert.Equal(id, task.Id);
            Assert.Equal("Complete Task Item Tests", task.Name);
            Assert.Equal(createdAt, task.CreatedAt);
            Assert.True(task.IsCompleted);
            Assert.Equal(150, task.TotalFocusMinutes);
            Assert.Equal(6, task.PomodoroCount);
            Assert.Equal(lastWorkedOn, task.LastWorkedOn);
            Assert.True(task.IsDeleted);
            Assert.Equal(deletedAt, task.DeletedAt);
        }
    }
}

