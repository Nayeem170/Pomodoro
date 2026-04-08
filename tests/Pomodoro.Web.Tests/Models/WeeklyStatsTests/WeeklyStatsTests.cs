using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

/// <summary>
/// Tests for WeeklyStats record model
/// </summary>
public class WeeklyStatsTests
{
    public class Constructor : WeeklyStatsTests
    {
        [Fact]
        public void ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var stats = new WeeklyStats();

            // Assert
            Assert.Equal(0, stats.TotalFocusMinutes);
            Assert.Equal(0, stats.TotalPomodoroCount);
            Assert.Equal(0, stats.UniqueTasksWorkedOn);
            Assert.Equal(0, stats.DailyAverageMinutes);
            Assert.Equal(DayOfWeek.Sunday, stats.MostProductiveDay); // default enum value
            Assert.Equal(0, stats.PreviousWeekFocusMinutes);
            Assert.Equal(0, stats.WeekOverWeekChange);
        }
    }

    public class TotalFocusMinutes : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void ShouldSetAndGetTotalFocusMinutes(int minutes)
        {
            // Arrange & Act
            var stats = new WeeklyStats { TotalFocusMinutes = minutes };

            // Assert
            Assert.Equal(minutes, stats.TotalFocusMinutes);
        }
    }

    public class TotalPomodoroCount : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void ShouldSetAndGetTotalPomodoroCount(int count)
        {
            // Arrange & Act
            var stats = new WeeklyStats { TotalPomodoroCount = count };

            // Assert
            Assert.Equal(count, stats.TotalPomodoroCount);
        }
    }

    public class UniqueTasksWorkedOn : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(20)]
        public void ShouldSetAndGetUniqueTasksWorkedOn(int count)
        {
            // Arrange & Act
            var stats = new WeeklyStats { UniqueTasksWorkedOn = count };

            // Assert
            Assert.Equal(count, stats.UniqueTasksWorkedOn);
        }
    }

    public class DailyAverageMinutes : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0.0)]
        [InlineData(25.5)]
        [InlineData(100.0)]
        [InlineData(250.75)]
        public void ShouldSetAndGetDailyAverageMinutes(double average)
        {
            // Arrange & Act
            var stats = new WeeklyStats { DailyAverageMinutes = average };

            // Assert
            Assert.Equal(average, stats.DailyAverageMinutes);
        }
    }

    public class MostProductiveDay : WeeklyStatsTests
    {
        [Theory]
        [InlineData(DayOfWeek.Monday)]
        [InlineData(DayOfWeek.Tuesday)]
        [InlineData(DayOfWeek.Wednesday)]
        [InlineData(DayOfWeek.Thursday)]
        [InlineData(DayOfWeek.Friday)]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public void ShouldSetAndGetMostProductiveDay(DayOfWeek day)
        {
            // Arrange & Act
            var stats = new WeeklyStats { MostProductiveDay = day };

            // Assert
            Assert.Equal(day, stats.MostProductiveDay);
        }
    }

    public class PreviousWeekFocusMinutes : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(500)]
        public void ShouldSetAndGetPreviousWeekFocusMinutes(int minutes)
        {
            // Arrange & Act
            var stats = new WeeklyStats { PreviousWeekFocusMinutes = minutes };

            // Assert
            Assert.Equal(minutes, stats.PreviousWeekFocusMinutes);
        }
    }

    public class WeekOverWeekChange : WeeklyStatsTests
    {
        [Theory]
        [InlineData(0.0)]
        [InlineData(10.5)]   // 10.5% increase
        [InlineData(-5.25)]  // 5.25% decrease
        [InlineData(100.0)]  // 100% increase (doubled)
        public void ShouldSetAndGetWeekOverWeekChange(double change)
        {
            // Arrange & Act
            var stats = new WeeklyStats { WeekOverWeekChange = change };

            // Assert
            Assert.Equal(change, stats.WeekOverWeekChange);
        }
    }

    public class RecordEquality : WeeklyStatsTests
    {
        [Fact]
        public void ShouldBeEqual_WhenAllPropertiesMatch()
        {
            // Arrange
            var stats1 = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 20,
                UniqueTasksWorkedOn = 5,
                DailyAverageMinutes = 71.4,
                MostProductiveDay = DayOfWeek.Wednesday,
                PreviousWeekFocusMinutes = 450,
                WeekOverWeekChange = 11.1
            };

            var stats2 = new WeeklyStats
            {
                TotalFocusMinutes = 500,
                TotalPomodoroCount = 20,
                UniqueTasksWorkedOn = 5,
                DailyAverageMinutes = 71.4,
                MostProductiveDay = DayOfWeek.Wednesday,
                PreviousWeekFocusMinutes = 450,
                WeekOverWeekChange = 11.1
            };

            // Act & Assert
            Assert.Equal(stats1, stats2);
        }

        [Fact]
        public void ShouldNotBeEqual_WhenPropertiesDiffer()
        {
            // Arrange
            var stats1 = new WeeklyStats { TotalFocusMinutes = 500 };
            var stats2 = new WeeklyStats { TotalFocusMinutes = 600 };

            // Act & Assert
            Assert.NotEqual(stats1, stats2);
        }

        [Fact]
        public void ShouldHaveSameHashCode_WhenEqual()
        {
            // Arrange
            var stats1 = new WeeklyStats { TotalFocusMinutes = 500, TotalPomodoroCount = 20 };
            var stats2 = new WeeklyStats { TotalFocusMinutes = 500, TotalPomodoroCount = 20 };

            // Act & Assert
            Assert.Equal(stats1.GetHashCode(), stats2.GetHashCode());
        }
    }

    public class AllProperties : WeeklyStatsTests
    {
        [Fact]
        public void ShouldSetAllProperties()
        {
            // Arrange & Act
            var stats = new WeeklyStats
            {
                TotalFocusMinutes = 750,
                TotalPomodoroCount = 30,
                UniqueTasksWorkedOn = 8,
                DailyAverageMinutes = 107.14,
                MostProductiveDay = DayOfWeek.Friday,
                PreviousWeekFocusMinutes = 600,
                WeekOverWeekChange = 25.0
            };

            // Assert
            Assert.Equal(750, stats.TotalFocusMinutes);
            Assert.Equal(30, stats.TotalPomodoroCount);
            Assert.Equal(8, stats.UniqueTasksWorkedOn);
            Assert.Equal(107.14, stats.DailyAverageMinutes);
            Assert.Equal(DayOfWeek.Friday, stats.MostProductiveDay);
            Assert.Equal(600, stats.PreviousWeekFocusMinutes);
            Assert.Equal(25.0, stats.WeekOverWeekChange);
        }
    }

    public class InitOnlyProperties : WeeklyStatsTests
    {
        [Fact]
        public void ShouldAllowInitDuringConstruction()
        {
            // Arrange & Act
            var stats = new WeeklyStats
            {
                TotalFocusMinutes = 300,
                TotalPomodoroCount = 12
            };

            // Assert
            Assert.Equal(300, stats.TotalFocusMinutes);
            Assert.Equal(12, stats.TotalPomodoroCount);
        }
    }
}
