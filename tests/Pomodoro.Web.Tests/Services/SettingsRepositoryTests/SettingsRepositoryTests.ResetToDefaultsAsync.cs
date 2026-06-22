using Moq;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Tests for SettingsRepository.ResetToDefaultsAsync method.
/// </summary>
[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    #region ResetToDefaultsAsync Tests

    [Fact]
    public async Task ResetToDefaultsAsync_CallsSaveAsync()
    {
        // Arrange
        MockIndexedDb
            .Setup(x => x.PutAsync(
                Constants.Storage.SettingsStore,
                It.IsAny<object>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.ResetToDefaultsAsync();

        // Assert
        MockIndexedDb.Verify(x => x.PutAsync(
            Constants.Storage.SettingsStore,
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_SavesDefaultSettings()
    {
        // Arrange
        object? capturedRecord = null;

        MockIndexedDb
            .Setup(x => x.PutAsync(
                Constants.Storage.SettingsStore,
                It.IsAny<object>()))
            .Callback<string, object>((_, record) =>
            {
                capturedRecord = record;
            })
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.ResetToDefaultsAsync();

        // Assert
        Assert.NotNull(capturedRecord);
        var recordType = capturedRecord!.GetType();

        // Verify all default values
        Assert.Equal(Constants.Timer.DefaultPomodoroMinutes,
            recordType.GetProperty("PomodoroMinutes")?.GetValue(capturedRecord));
        Assert.Equal(Constants.Timer.DefaultShortBreakMinutes,
            recordType.GetProperty("ShortBreakMinutes")?.GetValue(capturedRecord));
        Assert.Equal(Constants.Timer.DefaultLongBreakMinutes,
            recordType.GetProperty("LongBreakMinutes")?.GetValue(capturedRecord));
        Assert.True((bool?)recordType.GetProperty("SoundEnabled")?.GetValue(capturedRecord)!);
        Assert.True((bool?)recordType.GetProperty("NotificationsEnabled")?.GetValue(capturedRecord)!);
        Assert.True((bool?)recordType.GetProperty("AutoStartSession")?.GetValue(capturedRecord)!);
        Assert.Equal(Constants.Timer.DefaultAutoStartDelaySeconds,
            recordType.GetProperty("AutoStartDelaySeconds")?.GetValue(capturedRecord));
    }

    [Fact]
    public async Task ResetToDefaultsAsync_UsesCorrectStore()
    {
        // Arrange
        string? capturedStoreName = null;

        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Callback<string, object>((store, _) =>
            {
                capturedStoreName = store;
            })
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.ResetToDefaultsAsync();

        // Assert
        Assert.Equal(Constants.Storage.SettingsStore, capturedStoreName);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_SetsCorrectId()
    {
        // Arrange
        object? capturedRecord = null;

        MockIndexedDb
            .Setup(x => x.PutAsync(
                Constants.Storage.SettingsStore,
                It.IsAny<object>()))
            .Callback<string, object>((_, record) =>
            {
                capturedRecord = record;
            })
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act
        await repository.ResetToDefaultsAsync();

        // Assert
        Assert.NotNull(capturedRecord);
        var recordType = capturedRecord!.GetType();
        var id = recordType.GetProperty("Id")?.GetValue(capturedRecord);
        Assert.Equal(Constants.Storage.DefaultSettingsId, id);
    }

    #endregion
}

