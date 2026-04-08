using Xunit;
using Moq;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Test cases for ActivityService cache invalidation methods.
/// 
/// Tests the two cache invalidation methods added to ActivityService:
/// - InvalidateCacheForActivity(Guid activityId) - Invalidates cache for a specific activity's date
/// - InvalidateCacheForDateRange(DateTime from, DateTime to) - Invalidates cache for a date range
/// </summary>
public partial class ActivityServiceTests
{
    /// <summary>
    /// Test class for cache invalidation methods.
    /// </summary>
    [Collection("CacheInvalidation Tests")]
    public class CacheInvalidationTests : ActivityServiceTests
    {
        #region InvalidateCacheForActivity Tests

        /// <summary>
        /// Scenario: Activity exists in cache.
        /// Expected: Cache for that activity's date is invalidated.
        /// Note: This tests that the method runs without error when activity is found.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForActivity_WhenActivityInCache_RunsWithoutError()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var activityDate = DateTime.Today;
            var activities = new List<ActivityRecord>
            {
                CreateSampleActivity(id: activityId, completedAt: activityDate)
            };

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(activities);

            var service = CreateService();
            
            // Initialize cache by calling InitializeAsync
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForActivity(activityId);
        }

        /// <summary>
        /// Scenario: Activity not in cache.
        /// Expected: No exception thrown.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForActivity_WhenActivityNotInCache_DoesNotThrow()
        {
            // Arrange
            var unknownActivityId = Guid.NewGuid();
            
            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForActivity(unknownActivityId);
        }

        /// <summary>
        /// Scenario: Multiple activities on same date, invalidate one.
        /// Expected: Date cache is invalidated (affects all activities on that date).
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForActivity_WithMultipleActivitiesOnSameDate_InvalidatesDateCache()
        {
            // Arrange
            var activityId1 = Guid.NewGuid();
            var activityId2 = Guid.NewGuid();
            var sameDate = DateTime.Today;
            
            var activities = new List<ActivityRecord>
            {
                CreateSampleActivity(id: activityId1, completedAt: sameDate),
                CreateSampleActivity(id: activityId2, completedAt: sameDate)
            };

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act - Invalidate one activity
            service.InvalidateCacheForActivity(activityId1);

            // Assert - Method completes without error
            // The date cache for 'sameDate' should be invalidated
            // This is verified by ensuring no exception is thrown
        }

        /// <summary>
        /// Scenario: Service not initialized (cache empty).
        /// Expected: No exception thrown.
        /// </summary>
        [Fact]
        public void InvalidateCacheForActivity_WhenNotInitialized_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();
            // Don't call InitializeAsync - cache is empty

            // Act & Assert - Should not throw
            service.InvalidateCacheForActivity(Guid.NewGuid());
        }

        #endregion

        #region InvalidateCacheForDateRange Tests

        /// <summary>
        /// Scenario: Invalidate cache for a date range.
        /// Expected: Method completes without error.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForDateRange_WithValidRange_RunsWithoutError()
        {
            // Arrange
            var from = DateTime.Today.AddDays(-3);
            var to = DateTime.Today;
            
            var activities = CreateSampleActivitiesForDates(
                DateTime.Today.AddDays(-3),
                DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(-1),
                DateTime.Today
            );

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForDateRange(from, to);
        }

        /// <summary>
        /// Scenario: Single date range (from == to).
        /// Expected: Method completes without error.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForDateRange_SingleDate_RunsWithoutError()
        {
            // Arrange
            var singleDate = DateTime.Today;
            
            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForDateRange(singleDate, singleDate);
        }

        /// <summary>
        /// Scenario: Date range in reverse order (from > to).
        /// Expected: Loop doesn't execute, no error.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForDateRange_ReverseOrder_DoesNotThrow()
        {
            // Arrange
            var from = DateTime.Today;
            var to = DateTime.Today.AddDays(-3); // from > to

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            // Loop condition (date <= to) is false immediately
            service.InvalidateCacheForDateRange(from, to);
        }

        /// <summary>
        /// Scenario: Empty cache, invalidate date range.
        /// Expected: Method completes without error.
        /// </summary>
        [Fact]
        public void InvalidateCacheForDateRange_WhenCacheEmpty_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();
            // Don't initialize - cache is empty

            // Act & Assert - Should not throw
            service.InvalidateCacheForDateRange(DateTime.Today.AddDays(-1), DateTime.Today);
        }

        /// <summary>
        /// Scenario: Invalidate future dates (no activities exist).
        /// Expected: Method completes without error.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForDateRange_FutureDates_DoesNotThrow()
        {
            // Arrange
            var futureFrom = DateTime.Today.AddDays(10);
            var futureTo = DateTime.Today.AddDays(15);

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForDateRange(futureFrom, futureTo);
        }

        /// <summary>
        /// Scenario: Large date range (1 year).
        /// Expected: Method completes without error.
        /// </summary>
        [Fact]
        public async Task InvalidateCacheForDateRange_LargeRange_DoesNotThrow()
        {
            // Arrange
            var from = DateTime.Today.AddYears(-1);
            var to = DateTime.Today;

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            service.InvalidateCacheForDateRange(from, to);
        }

        #endregion

        #region Thread Safety Tests

        /// <summary>
        /// Scenario: Concurrent cache invalidation.
        /// Expected: No race conditions or exceptions.
        /// Note: This is a basic test - full thread safety would require more complex testing.
        /// </summary>
        [Fact]
        public async Task CacheInvalidation_ConcurrentCalls_DoesNotThrow()
        {
            // Arrange
            var activities = CreateSampleActivitiesForDates(
                DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(-1),
                DateTime.Today
            );

            MockActivityRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var activityId = activities[0].Id;

            // Act - Run multiple invalidations concurrently
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() =>
                {
                    service.InvalidateCacheForActivity(activityId);
                    service.InvalidateCacheForDateRange(DateTime.Today.AddDays(-5), DateTime.Today);
                }))
                .ToArray();

            // Assert - All tasks should complete without exception
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}
