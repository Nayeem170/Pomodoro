using Moq;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Tests for SettingsRepository concurrent access scenarios.
/// </summary>
[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    [Trait("Category", "Service")]
    public class ConcurrentTests : SettingsRepositoryTests
    {
        [Fact]
        public async Task GetAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var repository = CreateRepository();

            // Act - Call GetAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => repository.GetAsync())
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert - All calls should complete successfully
            Assert.All(tasks, task => Assert.Null(task.Result));
        }

        [Fact]
        public async Task SaveAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var callCount = 0;
            MockIndexedDb
                .Setup(x => x.PutAsync(
                    Constants.Storage.SettingsStore,
                    It.IsAny<object>()))
                .Callback<string, object>((_, _) =>
                {
                    Interlocked.Increment(ref callCount);
                    // Simulate a small delay to increase chance of race conditions
                    Thread.Sleep(10);
                })
                .ReturnsAsync(true);

            var repository = CreateRepository();

            // Act - Call SaveAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 5)
                .Select(i => repository.SaveAsync(CreateTestSettings(pomodoroMinutes: 30 + i)))
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert - All calls should complete successfully
            Assert.All(tasks, task => Assert.True(task.Result));
            Assert.Equal(5, callCount);
        }

        [Fact]
        public async Task GetAsyncAndSaveAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            MockIndexedDb
                .Setup(x => x.PutAsync(
                    Constants.Storage.SettingsStore,
                    It.IsAny<object>()))
                .ReturnsAsync(true);

            var repository = CreateRepository();

            // Act - Mix GetAsync and SaveAsync calls concurrently
            var tasks = new List<Task>();
            
            // Add 5 GetAsync calls
            tasks.AddRange(Enumerable.Range(0, 5)
                .Select(_ => repository.GetAsync()));
            
            // Add 5 SaveAsync calls
            tasks.AddRange(Enumerable.Range(0, 5)
                .Select(i => repository.SaveAsync(CreateTestSettings(pomodoroMinutes: 30 + i))));
            
            await Task.WhenAll(tasks);

            // Assert - All calls should complete successfully
            var getTasks = tasks.OfType<Task<Pomodoro.Web.Models.TimerSettings?>>().ToArray();
            var saveTasks = tasks.OfType<Task<bool>>().ToArray();
            
            Assert.Equal(5, getTasks.Length);
            Assert.Equal(5, saveTasks.Length);
            
            Assert.All(saveTasks, task => Assert.True(task.Result));
        }

        [Fact]
        public async Task ResetToDefaultsAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var callCount = 0;
            MockIndexedDb
                .Setup(x => x.PutAsync(
                    Constants.Storage.SettingsStore,
                    It.IsAny<object>()))
                .Callback<string, object>((_, _) =>
                {
                    Interlocked.Increment(ref callCount);
                    // Simulate a small delay to increase chance of race conditions
                    Thread.Sleep(10);
                })
                .ReturnsAsync(true);

            var repository = CreateRepository();

            // Act - Call ResetToDefaultsAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => repository.ResetToDefaultsAsync())
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert - All calls should complete successfully
            Assert.All(tasks, task => Assert.Null(task.Exception));
            Assert.Equal(5, callCount);
        }

        [Fact]
        public async Task GetAsync_WithConcurrentAccessAndNullStorage_ReturnsNullForAll()
        {
            // Arrange
            var repository = CreateRepository();

            // Act - Call GetAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => repository.GetAsync())
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert - All calls should return null
            Assert.All(tasks, task => Assert.Null(task.Result));
        }

        [Fact]
        public async Task SaveAsync_WithConcurrentAccessAndStorageFailure_PropagatesException()
        {
            // Arrange - Setup to throw exception on every call
            MockIndexedDb
                .Setup(x => x.PutAsync(
                    Constants.Storage.SettingsStore,
                    It.IsAny<object>()))
                .ThrowsAsync(new InvalidOperationException("Storage operation failed"));

            var repository = CreateRepository();

            // Act - Call SaveAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => repository.SaveAsync(CreateTestSettings()))
                .ToArray();

            // Assert - All calls should throw
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            {
                await Task.WhenAll(tasks);
            });
            
            // Verify all individual tasks threw exceptions
            Assert.All(tasks, task => 
            {
                var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
                Assert.Contains("Storage operation failed", exception.Result.Message);
            });
        }
    }
}

