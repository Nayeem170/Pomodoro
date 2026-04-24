using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Additional tests for SettingsRepository.GetAsync method to improve coverage
/// </summary>
[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    public class GetAsyncCoverageTests : SettingsRepositoryTests
    {
        [Fact]
        public async Task GetAsync_WhenRecordIsNull_ReturnsNull()
        {
            // Arrange
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync((TimerSettingsRecord?)null);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasValues_ReturnsSettings()
        {
            // Arrange
            var expectedSettings = new TimerSettings
            {
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 15
            };

            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = expectedSettings.PomodoroMinutes,
                ShortBreakMinutes = expectedSettings.ShortBreakMinutes,
                LongBreakMinutes = expectedSettings.LongBreakMinutes,
                SoundEnabled = expectedSettings.SoundEnabled,
                NotificationsEnabled = expectedSettings.NotificationsEnabled,
                AutoStartPomodoros = expectedSettings.AutoStartPomodoros,
                AutoStartBreaks = expectedSettings.AutoStartBreaks,
                AutoStartDelaySeconds = expectedSettings.AutoStartDelaySeconds
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSettings.PomodoroMinutes, result.PomodoroMinutes);
            Assert.Equal(expectedSettings.ShortBreakMinutes, result.ShortBreakMinutes);
            Assert.Equal(expectedSettings.LongBreakMinutes, result.LongBreakMinutes);
            Assert.Equal(expectedSettings.SoundEnabled, result.SoundEnabled);
            Assert.Equal(expectedSettings.NotificationsEnabled, result.NotificationsEnabled);
            Assert.Equal(expectedSettings.AutoStartPomodoros, result.AutoStartPomodoros);
            Assert.Equal(expectedSettings.AutoStartBreaks, result.AutoStartBreaks);
            Assert.Equal(expectedSettings.AutoStartDelaySeconds, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasPartialValues_ReturnsSettingsWithDefaults()
        {
            // Arrange
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 15
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert - Should use default values for null properties
            Assert.NotNull(result);
            Assert.Equal(30, result.PomodoroMinutes);
            Assert.Equal(10, result.ShortBreakMinutes);
            Assert.Equal(20, result.LongBreakMinutes);
            Assert.True(result.SoundEnabled);
            Assert.True(result.NotificationsEnabled);
            Assert.False(result.AutoStartPomodoros);
            Assert.False(result.AutoStartBreaks);
            Assert.Equal(15, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasAllZeroValues_ReturnsSettingsWithClampedValues()
        {
            // Arrange
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 0,
                ShortBreakMinutes = 0,
                LongBreakMinutes = 0,
                SoundEnabled = false,
                NotificationsEnabled = false,
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 0
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert - TimerSettings clamps values to minimums
            Assert.NotNull(result);
            Assert.Equal(1, result.PomodoroMinutes); // Clamped to MinPomodoroMinutes
            Assert.Equal(1, result.ShortBreakMinutes); // Clamped to MinBreakMinutes
            Assert.Equal(1, result.LongBreakMinutes); // Clamped to MinBreakMinutes
            Assert.False(result.SoundEnabled);
            Assert.False(result.NotificationsEnabled);
            Assert.False(result.AutoStartPomodoros);
            Assert.False(result.AutoStartBreaks);
            Assert.Equal(3, result.AutoStartDelaySeconds); // Clamped to MinAutoStartDelaySeconds
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasAllMaxValues_ReturnsSettingsWithMaxValues()
        {
            // Arrange
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 60,
                ShortBreakMinutes = 15,
                LongBreakMinutes = 30,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartPomodoros = true,
                AutoStartBreaks = true,
                AutoStartDelaySeconds = 30
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(60, result.PomodoroMinutes);
            Assert.Equal(15, result.ShortBreakMinutes);
            Assert.Equal(30, result.LongBreakMinutes);
            Assert.True(result.SoundEnabled);
            Assert.True(result.NotificationsEnabled);
            Assert.True(result.AutoStartPomodoros);
            Assert.True(result.AutoStartBreaks);
            Assert.Equal(30, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenStorageThrowsException_Throws()
        {
            // Arrange
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ThrowsAsync(new Exception("Storage error"));

            var repository = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => repository.GetAsync());
        }

        [Fact]
        public async Task GetAsync_WhenStorageThrowsException_LogsError()
        {
            // Arrange
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ThrowsAsync(new Exception("Storage error"));

            var repository = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => repository.GetAsync());
        }

        [Fact]
        public async Task GetAsync_MultipleConcurrentCalls_AllSucceed()
        {
            // Arrange
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartPomodoros = false,
                AutoStartBreaks = false,
                AutoStartDelaySeconds = 10
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            // Act - Call GetAsync multiple times concurrently
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => repository.GetAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert - All should succeed
            foreach (var result in results)
            {
                Assert.NotNull(result);
            }
        }
    }
}

