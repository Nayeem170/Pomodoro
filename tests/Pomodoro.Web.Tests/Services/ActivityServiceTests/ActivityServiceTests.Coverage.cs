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
    public class CacheHitCoverageTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetDailyPomodoroCounts_SecondCallUsesCacheWithPositiveCount()
        {
            var date = new DateTime(2024, 6, 15);
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result1 = service.GetDailyPomodoroCounts(date, date);
            Assert.Equal(2, result1[date.Date]);

            var result2 = service.GetDailyPomodoroCounts(date, date);
            Assert.Equal(2, result2[date.Date]);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_SecondCallUsesCacheWithPositiveMinutes()
        {
            var date = new DateTime(2024, 6, 15);
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result1 = service.GetDailyBreakMinutes(date, date);
            Assert.Equal(20, result1[date.Date]);

            var result2 = service.GetDailyBreakMinutes(date, date);
            Assert.Equal(20, result2[date.Date]);
        }
    }
}

