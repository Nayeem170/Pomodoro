using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for SettingsPresenterService
/// </summary>
[Trait("Category", "Service")]
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
            AutoStartPomodoros = false,
            AutoStartBreaks = false,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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
            AutoStartPomodoros = false,
            AutoStartBreaks = false,
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
            AutoStartPomodoros = true,
            AutoStartBreaks = true,
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

}

