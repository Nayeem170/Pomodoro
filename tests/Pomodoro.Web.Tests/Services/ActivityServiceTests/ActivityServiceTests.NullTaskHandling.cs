using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService null task name handling.
/// </summary>
public partial class ActivityServiceTests
{
    public class NullTaskHandlingTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetTaskPomodoroCounts_WithNullTaskName_SkipsActivity()
        {
            // Arrange
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(10) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = null, // This should be skipped
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(11) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "", // This should also be skipped
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(12) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Another Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(13) 
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(testDate, testDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Only 2 valid task names
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Another Valid Task", result.Keys);
            Assert.Equal(1, result["Valid Task"]);
            Assert.Equal(1, result["Another Valid Task"]);
        }

        [Fact]
        public async Task GetTaskPomodoroCounts_WithEmptyTaskName_SkipsActivity()
        {
            // This test specifically focuses on empty string task names
            // Arrange
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(10) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "", // This should be skipped
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(11) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Another Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(12) 
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(testDate, testDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Only 2 valid task names
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Another Valid Task", result.Keys);
            Assert.Equal(1, result["Valid Task"]);
            Assert.Equal(1, result["Another Valid Task"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithMixedNullAndValidTaskNames_GroupsNullTasksUnderFocusTimeLabel()
        {
            // Arrange
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(10) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = null, // This should use the focus time label
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(11) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.ShortBreak, 
                    DurationMinutes = 5, 
                    CompletedAt = testDate.AddHours(11).AddMinutes(25) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "", // This should also use the focus time label
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(12) 
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(testDate);

            // Assert
            Assert.NotNull(result);
            // null Pomodoro task names become "Focus time", empty strings stay as ""
            // Short break with null becomes "Short Breaks"
            Assert.Equal(4, result.Count); // "Valid Task", "Focus time", "", and "Short Breaks"
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Focus time", result.Keys); // null Pomodoro task name
            Assert.Contains("", result.Keys); // empty string task name
            Assert.Contains("Short Breaks", result.Keys);
            Assert.Equal(25, result["Valid Task"]);
            Assert.Equal(25, result["Focus time"]); // from the null task name Pomodoro
            Assert.Equal(25, result[""]); // from the empty string task name
            Assert.Equal(5, result["Short Breaks"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithEmptyTaskName_UsesFocusTimeLabel()
        {
            // This test specifically focuses on empty string task names
            // Arrange
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "Valid Task",
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(10) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = "", // This should use the focus time label
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(11) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.ShortBreak, 
                    DurationMinutes = 5, 
                    CompletedAt = testDate.AddHours(11).AddMinutes(25) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.LongBreak, 
                    DurationMinutes = 15, 
                    CompletedAt = testDate.AddHours(12).AddMinutes(30) 
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(testDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // "Valid Task", "", "Short Breaks", and "Long Breaks" (the empty string is used for empty task names)
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("", result.Keys);
            Assert.Contains("Short Breaks", result.Keys);
            Assert.Contains("Long Breaks", result.Keys);
            Assert.Equal(25, result["Valid Task"]);
            Assert.Equal(25, result[""]); // 25 from the empty task name activity
            Assert.Equal(5, result["Short Breaks"]);
            Assert.Equal(15, result["Long Breaks"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithAllNullTaskNames_UsesFocusTimeLabel()
        {
            // Edge case: All pomodoro activities have null task names
            // Arrange
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = null, // This should use the focus time label
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(10) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.Pomodoro, 
                    TaskName = null, // This should also use the focus time label
                    DurationMinutes = 25, 
                    CompletedAt = testDate.AddHours(11) 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Type = SessionType.ShortBreak, 
                    DurationMinutes = 5, 
                    CompletedAt = testDate.AddHours(11).AddMinutes(25) 
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(testDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // "Focus time" and "Short Breaks"
            Assert.Contains("Focus time", result.Keys);
            Assert.Contains("Short Breaks", result.Keys);
            Assert.Equal(50, result["Focus time"]); // 25 + 25 from the two null task name activities
            Assert.Equal(5, result["Short Breaks"]);
        }
    }
}