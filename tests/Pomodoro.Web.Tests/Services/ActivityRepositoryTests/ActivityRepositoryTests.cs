using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Tests.Services.ActivityRepositoryTests;

/// <summary>
/// Base test class for ActivityRepository.
/// Contains shared setup and helper methods.
/// </summary>
using Xunit;
[Trait("Category", "Service")]
public partial class ActivityRepositoryTests
{
    protected readonly Mock<IIndexedDbService> MockIndexedDb;
    protected readonly Mock<ILogger<ActivityRepository>> MockLogger;

    public ActivityRepositoryTests()
    {
        MockIndexedDb = new Mock<IIndexedDbService>();
        MockLogger = new Mock<ILogger<ActivityRepository>>();
    }

    /// <summary>
    /// Creates an ActivityRepository instance with mocked dependencies.
    /// </summary>
    protected ActivityRepository CreateRepository()
    {
        return new ActivityRepository(MockIndexedDb.Object, MockLogger.Object);
    }

    /// <summary>
    /// Creates a sample ActivityRecord for testing.
    /// </summary>
    protected static ActivityRecord CreateSampleActivity(
        Guid? id = null,
        SessionType type = SessionType.Pomodoro,
        DateTime? completedAt = null,
        int durationMinutes = 25,
        string? taskName = null)
    {
        return new ActivityRecord
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            CompletedAt = completedAt ?? DateTime.UtcNow,
            DurationMinutes = durationMinutes,
            TaskName = taskName,
            WasCompleted = true
        };
    }

    /// <summary>
    /// Creates a list of sample activities for testing.
    /// </summary>
    protected static List<ActivityRecord> CreateSampleActivities(int count = 3)
    {
        var activities = new List<ActivityRecord>();
        var baseDate = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            activities.Add(new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = i % 2 == 0 ? SessionType.Pomodoro : SessionType.ShortBreak,
                CompletedAt = baseDate.AddDays(-i),
                DurationMinutes = i % 2 == 0 ? 25 : 5,
                TaskName = $"Task {i + 1}",
                WasCompleted = true
            });
        }

        return activities;
    }
}

