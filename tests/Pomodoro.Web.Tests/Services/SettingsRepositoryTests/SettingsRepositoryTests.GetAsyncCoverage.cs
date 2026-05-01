using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    public class GetAsyncCoverageTests : SettingsRepositoryTests
    {
        [Fact]
        public async Task GetAsync_WhenRecordIsNull_ReturnsNull()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync((TimerSettingsRecord?)null);

            var repository = CreateRepository();

            var result = await repository.GetAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasValues_ReturnsSettings()
        {
            var expectedSettings = new TimerSettings
            {
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartSession = false,
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
                AutoStartSession = expectedSettings.AutoStartSession,
                AutoStartDelaySeconds = expectedSettings.AutoStartDelaySeconds
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            var result = await repository.GetAsync();

            Assert.NotNull(result);
            Assert.Equal(expectedSettings.PomodoroMinutes, result.PomodoroMinutes);
            Assert.Equal(expectedSettings.ShortBreakMinutes, result.ShortBreakMinutes);
            Assert.Equal(expectedSettings.LongBreakMinutes, result.LongBreakMinutes);
            Assert.Equal(expectedSettings.SoundEnabled, result.SoundEnabled);
            Assert.Equal(expectedSettings.NotificationsEnabled, result.NotificationsEnabled);
            Assert.Equal(expectedSettings.AutoStartSession, result.AutoStartSession);
            Assert.Equal(expectedSettings.AutoStartDelaySeconds, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasPartialValues_ReturnsSettingsWithDefaults()
        {
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 30,
                ShortBreakMinutes = 10,
                LongBreakMinutes = 20,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartSession = false,
                AutoStartDelaySeconds = 15
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            var result = await repository.GetAsync();

            Assert.NotNull(result);
            Assert.Equal(30, result.PomodoroMinutes);
            Assert.Equal(10, result.ShortBreakMinutes);
            Assert.Equal(20, result.LongBreakMinutes);
            Assert.True(result.SoundEnabled);
            Assert.True(result.NotificationsEnabled);
            Assert.False(result.AutoStartSession);
            Assert.Equal(15, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasAllZeroValues_ReturnsSettingsWithClampedValues()
        {
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 0,
                ShortBreakMinutes = 0,
                LongBreakMinutes = 0,
                SoundEnabled = false,
                NotificationsEnabled = false,
                AutoStartSession = false,
                AutoStartDelaySeconds = 0
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            var result = await repository.GetAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.PomodoroMinutes);
            Assert.Equal(1, result.ShortBreakMinutes);
            Assert.Equal(1, result.LongBreakMinutes);
            Assert.False(result.SoundEnabled);
            Assert.False(result.NotificationsEnabled);
            Assert.False(result.AutoStartSession);
            Assert.Equal(3, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenRecordHasAllMaxValues_ReturnsSettingsWithMaxValues()
        {
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 60,
                ShortBreakMinutes = 15,
                LongBreakMinutes = 30,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartSession = true,
                AutoStartDelaySeconds = 30
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            var result = await repository.GetAsync();

            Assert.NotNull(result);
            Assert.Equal(60, result.PomodoroMinutes);
            Assert.Equal(15, result.ShortBreakMinutes);
            Assert.Equal(30, result.LongBreakMinutes);
            Assert.True(result.SoundEnabled);
            Assert.True(result.NotificationsEnabled);
            Assert.True(result.AutoStartSession);
            Assert.Equal(30, result.AutoStartDelaySeconds);
        }

        [Fact]
        public async Task GetAsync_WhenStorageThrowsException_Throws()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ThrowsAsync(new Exception("Storage error"));

            var repository = CreateRepository();

            await Assert.ThrowsAsync<Exception>(() => repository.GetAsync());
        }

        [Fact]
        public async Task GetAsync_WhenStorageThrowsException_LogsError()
        {
            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ThrowsAsync(new Exception("Storage error"));

            var repository = CreateRepository();

            await Assert.ThrowsAsync<Exception>(() => repository.GetAsync());
        }

        [Fact]
        public async Task GetAsync_MultipleConcurrentCalls_AllSucceed()
        {
            var record = new TimerSettingsRecord
            {
                Id = Constants.Storage.DefaultSettingsId,
                PomodoroMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15,
                SoundEnabled = true,
                NotificationsEnabled = true,
                AutoStartSession = false,
                AutoStartDelaySeconds = 10
            };

            MockIndexedDb
                .Setup(x => x.GetAsync<TimerSettingsRecord>(
                    Constants.Storage.SettingsStore,
                    Constants.Storage.DefaultSettingsId))
                .ReturnsAsync(record);

            var repository = CreateRepository();

            var tasks = Enumerable.Range(0, 10)
                .Select(_ => repository.GetAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                Assert.NotNull(result);
            }
        }
    }
}
