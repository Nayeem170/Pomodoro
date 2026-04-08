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

    public class FormattedFocusTime : DailyStatsSummaryTests
    {
        [Fact]
        public void ShouldReturnMinutesOnly_WhenLessThanOneHour()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 45 };

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert - Format is like "45m"
            Assert.Contains("45", formatted);
            Assert.Contains("m", formatted.ToLower());
        }

        [Fact]
        public void ShouldReturnZeroMinutes_WhenZeroFocusTime()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 0 };

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert
            Assert.Contains("0", formatted);
        }

        [Fact]
        public void ShouldReturnHoursAndMinutes_WhenExactlyOneHour()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 60 };

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert - Format is like "1h 0m"
            Assert.Contains("1", formatted);
            Assert.Contains("h", formatted.ToLower());
            Assert.Contains("0", formatted);
        }

        [Fact]
        public void ShouldReturnHoursAndMinutes_WhenMoreThanOneHour()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 90 }; // 1h 30m

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert
            Assert.Contains("1", formatted);
            Assert.Contains("30", formatted);
        }

        [Fact]
        public void ShouldReturnCorrectFormat_ForLargeValues()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 250 }; // 4h 10m

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert
            Assert.Contains("4", formatted);
            Assert.Contains("10", formatted);
        }

        [Fact]
        public void ShouldReturnCorrectFormat_ForMultipleHoursNoMinutes()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 120 }; // 2h 0m

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert - Format is like "2h 0m"
            Assert.Contains("2", formatted);
            Assert.Contains("h", formatted.ToLower());
        }

        [Fact]
        public void ShouldReturnCorrectFormat_ForMultipleHoursWithMinutes()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 185 }; // 3h 5m

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert
            Assert.Contains("3", formatted);
            Assert.Contains("5", formatted);
        }

        [Fact]
        public void ShouldHandleBoundaryCase_At59Minutes()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 59 };

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert - Should show minutes only (59m)
            Assert.Contains("59", formatted);
            Assert.DoesNotContain("h", formatted.ToLower());
        }

        [Fact]
        public void ShouldHandleBoundaryCase_At61Minutes()
        {
            // Arrange
            var summary = new DailyStatsSummary { FocusMinutes = 61 };

            // Act
            var formatted = summary.FormattedFocusTime;

            // Assert - Should show hours and minutes (1h 1m)
            Assert.Contains("1", formatted);
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
