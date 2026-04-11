using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for SettingsPresenterService
/// </summary>
public class SettingsPresenterServiceTests
{
    private readonly SettingsPresenterService _service;

    public SettingsPresenterServiceTests()
    {
        var mockLogger = new Mock<ILogger<SettingsPresenterService>>();
        _service = new SettingsPresenterService(mockLogger.Object);
    }

    [Fact]
    public void IsAtDefaults_AllDefaults_ReturnsTrue()
    {
        // Arrange
        var settings = new TimerSettings();

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAtDefaults_AllDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 30,
            ShortBreakMinutes = 6,
            LongBreakMinutes = 20,
            SoundEnabled = false,
            NotificationsEnabled = false,
            AutoStartEnabled = false,
            AutoStartDelaySeconds = 10
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_PomodoroDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 30, // Different from default (25)
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_ShortBreakDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 10, // Different from default (5)
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_LongBreakDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 25, // Different from default (15)
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_SoundDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = false, // Different from default (true)
            NotificationsEnabled = true,
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_NotificationsDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = false, // Different from default (true)
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_AutoStartDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartEnabled = false, // Different from default (true)
            AutoStartDelaySeconds = 5
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtDefaults_AutoStartDelayDifferent_ReturnsFalse()
    {
        // Arrange
        var settings = new TimerSettings
        {
            PomodoroMinutes = 25,
            ShortBreakMinutes = 5,
            LongBreakMinutes = 15,
            SoundEnabled = true,
            NotificationsEnabled = true,
            AutoStartEnabled = true,
            AutoStartDelaySeconds = 15 // Different from default (10)
        };

        // Act
        var result = _service.IsAtDefaults(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BuildImportSuccessMessage_OnlyImported_ReturnsCorrectMessage()
    {
        // Arrange
        var totalImported = 5;
        var totalSkipped = 0;

        // Act
        var result = _service.BuildImportSuccessMessage(totalImported, totalSkipped);

        // Assert
        Assert.Equal("Import complete: imported 5 records.", result);
    }

    [Fact]
    public void BuildImportSuccessMessage_OnlySkipped_ReturnsCorrectMessage()
    {
        // Arrange
        var totalImported = 0;
        var totalSkipped = 3;

        // Act
        var result = _service.BuildImportSuccessMessage(totalImported, totalSkipped);

        // Assert
        Assert.Equal("Import complete: skipped 3 duplicates.", result);
    }

    [Fact]
    public void BuildImportSuccessMessage_BothImportedAndSkipped_ReturnsCorrectMessage()
    {
        // Arrange
        var totalImported = 10;
        var totalSkipped = 2;

        // Act
        var result = _service.BuildImportSuccessMessage(totalImported, totalSkipped);

        // Assert
        Assert.Equal("Import complete: imported 10 records, skipped 2 duplicates.", result);
    }

    [Fact]
    public void BuildImportSuccessMessage_None_ReturnsNoRecordsMessage()
    {
        // Arrange
        var totalImported = 0;
        var totalSkipped = 0;

        // Act
        var result = _service.BuildImportSuccessMessage(totalImported, totalSkipped);

        // Assert
        Assert.Equal("Import complete: no new records to import.", result);
    }

    [Fact]
    public void ValidateSettings_ValidSettings_ReturnsValid()
    {
        var settings = new TimerSettings { PomodoroMinutes = 25, ShortBreakMinutes = 5, LongBreakMinutes = 15, AutoStartDelaySeconds = 10 };
        var result = _service.ValidateSettings(settings);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateSettings_InvalidPomodoro_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_pomodoroMinutes", 0);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Pomodoro"));
    }

    [Fact]
    public void ValidateSettings_InvalidShortBreak_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_shortBreakMinutes", 61);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Short"));
    }

    [Fact]
    public void ValidateSettings_ShortBreakZero_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_shortBreakMinutes", 0);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Short"));
    }

    [Fact]
    public void ValidateSettings_InvalidLongBreak_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_longBreakMinutes", 65);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Long"));
    }

    [Fact]
    public void ValidateSettings_LongBreakZero_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_longBreakMinutes", 0);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Long"));
    }

    [Fact]
    public void ValidateSettings_InvalidAutoStartDelay_ReturnsInvalid()
    {
        var settings = new TimerSettings();
        SetPrivateField(settings, "_autoStartDelaySeconds", -1);
        var result = _service.ValidateSettings(settings);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Auto-start delay"));
    }
    
    // Helper to bypass clamping during unit tests and test validation logic
    private void SetPrivateField(TimerSettings obj, string fieldName, int value)
    {
        var field = typeof(TimerSettings).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field!.SetValue(obj, value);
    }

    [Fact]
    public void HasChanges_WhenChanged_ReturnsTrue()
    {
        var current = new TimerSettings { PomodoroMinutes = 30 };
        var original = new TimerSettings { PomodoroMinutes = 25 };
        var result = _service.HasChanges(current, original);
        Assert.True(result);
    }

    [Fact]
    public void HasChanges_WhenNotChanged_ReturnsFalse()
    {
        var current = new TimerSettings { PomodoroMinutes = 25 };
        var original = new TimerSettings { PomodoroMinutes = 25 };
        var result = _service.HasChanges(current, original);
        Assert.False(result);
    }

    [Fact]
    public void CreateToastMessage_SuccessWithoutCustomMessage_ReturnsDefaultSuccess()
    {
        var result = _service.CreateToastMessage(true);
        Assert.Equal("Settings saved successfully!", result);
    }

    [Fact]
    public void CreateToastMessage_FailedWithoutCustomMessage_ReturnsDefaultFail()
    {
        var result = _service.CreateToastMessage(false);
        Assert.Equal("Failed to save settings", result);
    }

    [Fact]
    public void CreateToastMessage_WithCustomMessage_ReturnsCustomMessage()
    {
        var result = _service.CreateToastMessage(true, "Custom Toast");
        Assert.Equal("Custom Toast", result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ShouldShowAutoStartDelay_MatchesAutoStartEnabled(bool autoStartEnabled, bool expected)
    {
        var result = _service.ShouldShowAutoStartDelay(autoStartEnabled);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateImportFileSize_ZeroBytes_ReturnsInvalid()
    {
        var result = _service.ValidateImportFileSize(0);
        Assert.False(result.IsValid);
        Assert.Equal("File is empty", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImportFileSize_TooLarge_ReturnsInvalid()
    {
        var result = _service.ValidateImportFileSize(Constants.Validation.MaxImportFileSizeBytes + 1);
        Assert.False(result.IsValid);
        Assert.Contains("File too large", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImportFileSize_ValidSize_ReturnsValid()
    {
        var result = _service.ValidateImportFileSize(1024); // 1KB
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void CreateExportFilename_ContainsCurrentDate()
    {
        var expectedDate = System.DateTime.Today.ToString("yyyy-MM-dd");
        var result = _service.CreateExportFilename();
        Assert.Contains(expectedDate, result);
        Assert.EndsWith(".json", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task DownloadFileAsync_InvokesJSInterop()
    {
        // Arrange
        var jsInteropMock = new Mock<IJSInteropService>();
        var filename = "test.json";
        var content = "{}";
        var mimeType = "application/json";

        // Act
        await _service.DownloadFileAsync(jsInteropMock.Object, filename, content, mimeType);

        // Assert
        jsInteropMock.Verify(js => js.InvokeVoidAsync("fileInterop.downloadFile", filename, It.IsAny<string>(), mimeType), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DownloadFileAsync_WhenThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var jsInteropMock = new Mock<IJSInteropService>();
        var mockLogger = new Mock<ILogger<SettingsPresenterService>>();
        var service = new SettingsPresenterService(mockLogger.Object);
        jsInteropMock.Setup(js => js.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .ThrowsAsync(new System.Exception("JS Interop Error"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => service.DownloadFileAsync(jsInteropMock.Object, "t.json", "{}", "mime"));
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<System.Exception>(),
                It.Is<System.Func<It.IsAnyType, System.Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ShouldShowClearConfirmation_IsOppositeOfIsClearing(bool isClearing, bool expected)
    {
        var result = _service.ShouldShowClearConfirmation(isClearing);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CreateDelayedToastHideAction_ExecutesShowAndQueuesHide()
    {
        // Arrange
        bool showCalled = false;
        System.Action<bool> showAction = (val) => showCalled = val;
        System.Action hideAction = () => { };

        // Act
        var delayedAction = _service.CreateDelayedToastHideAction(showAction, hideAction, "message");
        delayedAction.Invoke();

        // Assert
        Assert.True(showCalled);
        // We can't easily wait for the SafeTaskRunner's Task.Delay, 
        // so we just verify show was called, which covers the start of the lambda.
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateDelayedToastHideAction_CallsHideAfterDelay()
    {
        // Arrange
        bool hideCalled = false;
        System.Action<bool> showAction = (val) => { };
        System.Action hideAction = () => hideCalled = true;

        // Act
        var delayedAction = _service.CreateDelayedToastHideAction(showAction, hideAction, "message");
        delayedAction.Invoke();

        // Wait for the toast duration plus buffer
        await System.Threading.Tasks.Task.Delay(Constants.UI.ToastDurationMs + 500);

        // Assert - hideToast() should have been called (line 260)
        Assert.True(hideCalled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void StateHelperProperties_ReturnCorrectValues(bool state, bool expected)
    {
        Assert.Equal(expected, _service.ShouldDisableImportButton(state));
        Assert.Equal(expected, _service.ShouldDisableExportButton(state));
        Assert.Equal(expected, _service.ShouldDisableClearButton(state));
        
        // These are inversions or aliases of other states
        Assert.Equal(!state, _service.ShouldDisableSaveButton(state)); // e.g., state = hasChanges. If hasChanges=true, disable=false.
        Assert.Equal(state, _service.ShouldDisableResetButton(state)); // e.g., state = isAtDefaults. If atDefaults=true, disable=true.
    }
}
