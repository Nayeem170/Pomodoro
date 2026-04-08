using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Base test class for ActivityService.
/// Contains shared setup and helper methods.
/// </summary>
public partial class ActivityServiceTests
{
    protected readonly Mock<IActivityRepository> MockActivityRepository;
    protected readonly Mock<ILogger<ActivityService>> MockLogger;

    public ActivityServiceTests()
    {
        MockActivityRepository = new Mock<IActivityRepository>();
        MockLogger = new Mock<ILogger<ActivityService>>();
    }

    /// <summary>
    /// Creates an ActivityService instance with mocked dependencies.
    /// </summary>
    protected ActivityService CreateService()
    {
        return new ActivityService(
            MockActivityRepository.Object,
            MockLogger.Object
        );
    }

    /// <summary>
    /// Creates a sample activity for testing.
    /// </summary>
    protected static ActivityRecord CreateSampleActivity(Guid? id = null, DateTime? completedAt = null, string taskName = "Sample Task")
    {
        return new ActivityRecord
        {
            Id = id ?? Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            TaskName = taskName,
            DurationMinutes = 25,
            CompletedAt = completedAt ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates sample activities for multiple dates.
    /// </summary>
    protected static List<ActivityRecord> CreateSampleActivitiesForDates(params DateTime[] dates)
    {
        return dates.Select(d => CreateSampleActivity(completedAt: d)).ToList();
    }
}
