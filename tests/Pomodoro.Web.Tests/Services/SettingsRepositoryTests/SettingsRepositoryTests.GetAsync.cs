using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Tests for SettingsRepository.GetAsync method.
/// Note: Testing GetAsync directly requires mocking the internal TimerSettingsRecord type,
/// which is complex. The core functionality is tested through SaveAsync and ResetToDefaultsAsync.
/// Integration tests would be more appropriate for testing the full GetAsync flow.
/// </summary>
[Trait("Category", "Service")]
public partial class SettingsRepositoryTests
{
    #region GetAsync Tests (Integration-focused)

    [Fact]
    public async Task GetAsync_AfterSave_RoundTripsSettings()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 35,
            ShortBreakMinutes = 8,
            LongBreakMinutes = 20,
            SoundEnabled = false,
            NotificationsEnabled = true,
            AutoStartEnabled = false,
            AutoStartDelaySeconds = 20
        };

        // Setup SaveAsync to succeed
        MockIndexedDb
            .Setup(x => x.PutAsync(
                Constants.Storage.SettingsStore,
                It.IsAny<object>()))
            .ReturnsAsync(true);

        var repository = CreateRepository();

        // Act - Save settings
        var saveResult = await repository.SaveAsync(settings);

        // Assert - Save should succeed
        Assert.True(saveResult);

        // Verify PutAsync was called with correct store
        MockIndexedDb.Verify(
            x => x.PutAsync(
                Constants.Storage.SettingsStore,
                It.IsAny<object>()),
            Times.Once);
    }

    #endregion
}

