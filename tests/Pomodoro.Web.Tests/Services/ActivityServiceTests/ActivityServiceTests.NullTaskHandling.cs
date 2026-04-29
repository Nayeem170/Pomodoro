using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    public class NullTaskHandlingTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetTaskPomodoroCounts_WithNullTaskName_SkipsActivity()
        {
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
                    TaskName = null,
                    DurationMinutes = 25,
                    CompletedAt = testDate.AddHours(11)
                },
                new() {
                    Id = Guid.NewGuid(),
                    Type = SessionType.Pomodoro,
                    TaskName = "",
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

            var result = service.GetTaskPomodoroCounts(testDate, testDate);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Another Valid Task", result.Keys);
            Assert.Equal(1, result["Valid Task"]);
            Assert.Equal(1, result["Another Valid Task"]);
        }

        [Fact]
        public async Task GetTaskPomodoroCounts_WithEmptyTaskName_SkipsActivity()
        {
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
                    TaskName = "",
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

            var result = service.GetTaskPomodoroCounts(testDate, testDate);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Another Valid Task", result.Keys);
            Assert.Equal(1, result["Valid Task"]);
            Assert.Equal(1, result["Another Valid Task"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithMixedNullAndValidTaskNames_GroupsNullTasksUnderFocusTimeLabel()
        {
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
                    TaskName = null,
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
                    TaskName = "",
                    DurationMinutes = 25,
                    CompletedAt = testDate.AddHours(12)
                }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result = service.GetTimeDistribution(testDate);

            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // "Valid Task", "Focus time", "", and "Breaks"
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("Focus time", result.Keys);
            Assert.Contains("", result.Keys);
            Assert.Contains("Breaks", result.Keys);
            Assert.Equal(25, result["Valid Task"]);
            Assert.Equal(25, result["Focus time"]);
            Assert.Equal(25, result[""]);
            Assert.Equal(5, result["Breaks"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithEmptyTaskName_UsesFocusTimeLabel()
        {
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
                    TaskName = "",
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

            var result = service.GetTimeDistribution(testDate);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // "Valid Task", "", and "Breaks" (combined short + long)
            Assert.Contains("Valid Task", result.Keys);
            Assert.Contains("", result.Keys);
            Assert.Contains("Breaks", result.Keys);
            Assert.Equal(25, result["Valid Task"]);
            Assert.Equal(25, result[""]);
            Assert.Equal(20, result["Breaks"]); // 5 + 15
        }

        [Fact]
        public async Task GetTimeDistribution_WithAllNullTaskNames_UsesFocusTimeLabel()
        {
            var testDate = new DateTime(2023, 06, 15);
            var activities = new List<ActivityRecord>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Type = SessionType.Pomodoro,
                    TaskName = null,
                    DurationMinutes = 25,
                    CompletedAt = testDate.AddHours(10)
                },
                new() {
                    Id = Guid.NewGuid(),
                    Type = SessionType.Pomodoro,
                    TaskName = null,
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

            var result = service.GetTimeDistribution(testDate);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // "Focus time" and "Breaks"
            Assert.Contains("Focus time", result.Keys);
            Assert.Contains("Breaks", result.Keys);
            Assert.Equal(50, result["Focus time"]);
            Assert.Equal(5, result["Breaks"]);
        }
    }
}
