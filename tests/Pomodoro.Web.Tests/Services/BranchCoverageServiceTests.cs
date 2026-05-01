using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    public class NullActivitiesBranchTests : ActivityServiceTests
    {
        [Fact]
        public async Task InitializeAsync_WithNullActivities_UsesEmptyEnumerable()
        {
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ActivityRecord>?)null);

            var service = CreateService();
            await service.InitializeAsync();

            Assert.Empty(service.GetAllActivities());
        }
    }

    public class ShortBreakOnlyDistributionTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetTimeDistribution_OnlyShortBreaks_NoLongBreakEntry()
        {
            var date = new DateTime(2024, 6, 15);
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result = service.GetTimeDistribution(date);

            Assert.Contains(result, kvp => kvp.Key == Constants.Activity.BreaksLabel);
            Assert.Single(result);
            Assert.Equal(5, result[Constants.Activity.BreaksLabel]);
        }
    }
}

[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    public class MaxLengthNameTests : TaskServiceTests
    {
        [Fact]
        public async Task AddTaskAsync_NameExceedsMaxLength_DoesNotAdd()
        {
            MockTaskRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

            var longName = new string('a', Constants.UI.MaxTaskNameLength + 1);
            await CreateService().AddTaskAsync(longName);

            MockTaskRepository.Verify(r => r.SaveAsync(It.IsAny<TaskItem>()), Times.Never);
        }
    }
}
