using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    [Trait("Category", "Service")]
    public class ErrorHandlingTests : SettingsRepositoryTests
    {
        [Fact]
        public async Task GetAsync_WhenFirstTime_ReturnsNull()
        {
            // Arrange
            MockIndexedDb
                .Setup(s => s.GetAsync<TimerSettingsRecord>(Constants.Storage.SettingsStore, Constants.Storage.DefaultSettingsId))
                .ReturnsAsync((TimerSettingsRecord?)null);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WhenStorageThrowsException_PropagatesException()
        {
            // Arrange
            MockIndexedDb
                .Setup(s => s.GetAsync<TimerSettingsRecord>(Constants.Storage.SettingsStore, Constants.Storage.DefaultSettingsId))
                .ThrowsAsync(new InvalidOperationException("Storage error"));

            var repository = CreateRepository();

            // Act & Assert - Exception should propagate
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetAsync());
        }

        [Fact]
        public async Task SaveAsync_WhenSettingsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(null!));
        }

        [Fact]
        public async Task SaveAsync_WhenStorageFails_ReturnsFalse()
        {
            // Arrange
            var settings = new TimerSettings();
            MockIndexedDb
                .Setup(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
                .ReturnsAsync(false);

            var repository = CreateRepository();

            // Act
            var result = await repository.SaveAsync(settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SaveAsync_WhenStorageThrowsException_PropagatesException()
        {
            // Arrange
            var settings = new TimerSettings();
            MockIndexedDb
                .Setup(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
                .ThrowsAsync(new InvalidOperationException("Storage error"));

            var repository = CreateRepository();

            // Act & Assert - Exception should propagate
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.SaveAsync(settings));
        }

        [Fact]
        public async Task SaveAsync_WhenSettingsAreValid_ReturnsTrue()
        {
            // Arrange
            var settings = new TimerSettings
            {
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20,
                SoundEnabled = false,
                NotificationsEnabled = false,
                AutoStartEnabled = false
            };

            MockIndexedDb
                .Setup(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
                .ReturnsAsync(true);

            var repository = CreateRepository();

            // Act
            var result = await repository.SaveAsync(settings);

            // Assert
            Assert.True(result);
            MockIndexedDb.Verify(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()), Times.Once);
        }

        [Fact]
        public async Task ResetToDefaultsAsync_WhenStorageFails_CompletesWithoutError()
        {
            // Arrange
            MockIndexedDb
                .Setup(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
                .ReturnsAsync(false);

            var repository = CreateRepository();

            // Act & Assert - Should complete without throwing
            await repository.ResetToDefaultsAsync();
        }

        [Fact]
        public async Task ResetToDefaultsAsync_WhenStorageThrowsException_PropagatesException()
        {
            // Arrange
            MockIndexedDb
                .Setup(s => s.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
                .ThrowsAsync(new InvalidOperationException("Storage error"));

            var repository = CreateRepository();

            // Act & Assert - Exception should propagate
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ResetToDefaultsAsync());
        }
    }
}

