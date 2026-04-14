using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

/// <summary>
/// Tests for DailyStatsSummary model
/// </summary>
public class DailyStatsSummaryTests
{
    public class Constructor : DailyStatsSummaryTests
    {
        [Fact]
        public void ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var summary = new DailyStatsSummary();

            // Assert
            Assert.Equal(0, summary.PomodoroCount);
            Assert.Equal(0, summary.FocusMinutes);
            Assert.Equal(0, summary.TasksWorkedOn);
        }
    }

    public class PomodoroCount : DailyStatsSummaryTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ShouldSetAndGetPomodoroCount(int count)
        {
            // Arrange & Act
            var summary = new DailyStatsSummary { PomodoroCount = count };

            // Assert
            Assert.Equal(count, summary.PomodoroCount);
        }
    }

    public class FocusMinutes : DailyStatsSummaryTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(150)]
        [InlineData(500)]
        public void ShouldSetAndGetFocusMinutes(int minutes)
        {
            // Arrange & Act
            var summary = new DailyStatsSummary { FocusMinutes = minutes };

            // Assert
            Assert.Equal(minutes, summary.FocusMinutes);
        }
    }

    public class TasksWorkedOn : DailyStatsSummaryTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(20)]
        public void ShouldSetAndGetTasksWorkedOn(int count)
        {
            // Arrange & Act
            var summary = new DailyStatsSummary { TasksWorkedOn = count };

            // Assert
            Assert.Equal(count, summary.TasksWorkedOn);
        }
    }

    public class AllProperties : DailyStatsSummaryTests
    {
        [Fact]
        public void ShouldSetAllProperties()
        {
            // Arrange & Act
            var summary = new DailyStatsSummary
            {
                PomodoroCount = 8,
                FocusMinutes = 200,
                TasksWorkedOn = 3
            };

            // Assert
            Assert.Equal(8, summary.PomodoroCount);
            Assert.Equal(200, summary.FocusMinutes);
            Assert.Equal(3, summary.TasksWorkedOn);
        }

        [Fact]
        public void ShouldBeMutable()
        {
            // Arrange
            var summary = new DailyStatsSummary();

            // Act
            summary.PomodoroCount = 5;
            summary.FocusMinutes = 125;
            summary.TasksWorkedOn = 2;

            // Assert
            Assert.Equal(5, summary.PomodoroCount);
            Assert.Equal(125, summary.FocusMinutes);
            Assert.Equal(2, summary.TasksWorkedOn);
        }
    }
}
