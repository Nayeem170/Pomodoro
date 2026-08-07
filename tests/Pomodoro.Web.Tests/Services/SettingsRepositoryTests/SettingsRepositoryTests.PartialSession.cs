using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    [Fact]
    public async Task SaveAsync_PersistsRecordPartialSessions()
    {
        // Arrange
        TimerSettingsRecord? capturedRecord = null;
        MockIndexedDb
            .Setup(x => x.PutAsync(Constants.Storage.SettingsStore, It.IsAny<TimerSettingsRecord>()))
            .Callback<string, object>((_, obj) => capturedRecord = (TimerSettingsRecord)obj)
            .ReturnsAsync(true);

        var settings = new TimerSettings { RecordPartialSessions = true };
        var repository = CreateRepository();

        // Act
        await repository.SaveAsync(settings);

        // Assert
        capturedRecord.Should().NotBeNull();
        capturedRecord!.RecordPartialSessions.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_MissingRecordPartialSessions_DefaultsToTrue()
    {
        var record = new TimerSettingsRecord
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartSession = true,
            AutoStartDelaySeconds = 10,
            LongBreakInterval = 4,
            DailyGoal = 8,
            ExpandTimerMobile = false,
            RecordPartialSessions = null
        };

        MockIndexedDb
            .Setup(x => x.GetAsync<TimerSettingsRecord>(
                Constants.Storage.SettingsStore,
                Constants.Storage.DefaultSettingsId))
            .ReturnsAsync(record);

        var repository = CreateRepository();

        var result = await repository.GetAsync();

        result.Should().NotBeNull();
        result!.RecordPartialSessions.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WithRecordPartialSessionsTrue_ReturnsTrue()
    {
        // Arrange
        var record = new TimerSettingsRecord
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartSession = true,
            AutoStartDelaySeconds = 10,
            LongBreakInterval = 4,
            DailyGoal = 8,
            ExpandTimerMobile = false,
            RecordPartialSessions = true
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
        result.Should().NotBeNull();
        result!.RecordPartialSessions.Should().BeTrue();
    }
}
