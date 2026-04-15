using Moq;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Tests for SettingsRepository.SaveAsync method.
/// </summary>
[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_CallsPutAsync_WithCorrectStore()
    {
        // Arrange
        var settings = CreateTestSettings();
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
        await repository.SaveAsync(settings);
        
        // Assert
        Assert.Equal(Constants.Storage.SettingsStore, capturedStoreName);
    }

    [Fact]
    public async Task SaveAsync_ReturnsTrue_WhenSaveSucceeds()
    {
        // Arrange
        var settings = CreateTestSettings();
        
        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(true);
        
        var repository = CreateRepository();
        
        // Act
        var result = await repository.SaveAsync(settings);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SaveAsync_ReturnsFalse_WhenSaveFails()
    {
        // Arrange
        var settings = CreateTestSettings();
        
        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(false);
        
        var repository = CreateRepository();
        
        // Act
        var result = await repository.SaveAsync(settings);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SaveAsync_MapsAllPropertiesToRecord()
    {
        // Arrange
        var settings = CreateTestSettings(
            pomodoroMinutes: 45,
            shortBreakMinutes: 12,
            longBreakMinutes: 25,
            soundEnabled: false,
            notificationsEnabled: false,
            autoStartEnabled: false,
            autoStartDelaySeconds: 30);
        
        object? capturedRecord = null;
        
        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Callback<string, object>((_, record) =>
            {
                capturedRecord = record;
            })
            .ReturnsAsync(true);
        
        var repository = CreateRepository();
        
        // Act
        await repository.SaveAsync(settings);
        
        // Assert
        Assert.NotNull(capturedRecord);
        var recordType = capturedRecord!.GetType();
        
        Assert.Equal(Constants.Storage.DefaultSettingsId, 
            recordType.GetProperty("Id")?.GetValue(capturedRecord));
        Assert.Equal(45, 
            recordType.GetProperty("PomodoroMinutes")?.GetValue(capturedRecord));
        Assert.Equal(12, 
            recordType.GetProperty("ShortBreakMinutes")?.GetValue(capturedRecord));
        Assert.Equal(25, 
            recordType.GetProperty("LongBreakMinutes")?.GetValue(capturedRecord));
        Assert.False((bool?)recordType.GetProperty("SoundEnabled")?.GetValue(capturedRecord)!);
        Assert.False((bool?)recordType.GetProperty("NotificationsEnabled")?.GetValue(capturedRecord)!);
        Assert.False((bool?)recordType.GetProperty("AutoStartEnabled")?.GetValue(capturedRecord)!);
        Assert.Equal(30, 
            recordType.GetProperty("AutoStartDelaySeconds")?.GetValue(capturedRecord));
    }

    [Fact]
    public async Task SaveAsync_CreatesRecordWithDefaultId()
    {
        // Arrange
        var settings = CreateTestSettings();
        object? capturedRecord = null;
        
        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Callback<string, object>((_, record) =>
            {
                capturedRecord = record;
            })
            .ReturnsAsync(true);
        
        var repository = CreateRepository();
        
        // Act
        await repository.SaveAsync(settings);
        
        // Assert
        Assert.NotNull(capturedRecord);
        var recordType = capturedRecord!.GetType();
        var id = recordType.GetProperty("Id")?.GetValue(capturedRecord);
        Assert.Equal(Constants.Storage.DefaultSettingsId, id);
    }

    [Fact]
    public async Task SaveAsync_WithDefaultSettings_MapsCorrectly()
    {
        // Arrange
        var settings = new Pomodoro.Web.Models.TimerSettings();
        object? capturedRecord = null;
        
        MockIndexedDb
            .Setup(x => x.PutAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Callback<string, object>((_, record) =>
            {
                capturedRecord = record;
            })
            .ReturnsAsync(true);
        
        var repository = CreateRepository();
        
        // Act
        await repository.SaveAsync(settings);
        
        // Assert
        Assert.NotNull(capturedRecord);
        var recordType = capturedRecord!.GetType();
        
        Assert.Equal(Constants.Timer.DefaultPomodoroMinutes, 
            recordType.GetProperty("PomodoroMinutes")?.GetValue(capturedRecord));
        Assert.Equal(Constants.Timer.DefaultShortBreakMinutes, 
            recordType.GetProperty("ShortBreakMinutes")?.GetValue(capturedRecord));
        Assert.Equal(Constants.Timer.DefaultLongBreakMinutes, 
            recordType.GetProperty("LongBreakMinutes")?.GetValue(capturedRecord));
    }

    #endregion
}

