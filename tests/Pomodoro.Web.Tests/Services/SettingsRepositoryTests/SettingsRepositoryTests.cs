using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Tests.Services.SettingsRepositoryTests;

/// <summary>
/// Base test class for SettingsRepository tests.
/// SettingsRepository uses IIndexedDbService for persistence.
/// </summary>
public partial class SettingsRepositoryTests
{
    protected readonly Mock<IIndexedDbService> MockIndexedDb;
    
    public SettingsRepositoryTests()
    {
        MockIndexedDb = new Mock<IIndexedDbService>(MockBehavior.Loose);
    }
    
    /// <summary>
    /// Creates a SettingsRepository instance with mocked dependencies.
    /// </summary>
    protected SettingsRepository CreateRepository()
    {
        return new SettingsRepository(MockIndexedDb.Object);
    }
    
    /// <summary>
    /// Creates test settings with custom values.
    /// </summary>
    protected static TimerSettings CreateTestSettings(
        int pomodoroMinutes = 30,
        int shortBreakMinutes = 10,
        int longBreakMinutes = 20,
        bool soundEnabled = true,
        bool notificationsEnabled = true,
        bool autoStartEnabled = true,
        int autoStartDelaySeconds = 15)
    {
        return new TimerSettings
        {
            PomodoroMinutes = pomodoroMinutes,
            ShortBreakMinutes = shortBreakMinutes,
            LongBreakMinutes = longBreakMinutes,
            SoundEnabled = soundEnabled,
            NotificationsEnabled = notificationsEnabled,
            AutoStartEnabled = autoStartEnabled,
            AutoStartDelaySeconds = autoStartDelaySeconds
        };
    }
    
    /// <summary>
    /// TimerSettingsRecord is internal, so we use a dynamic object for mocking.
    /// This helper creates a record-like object for testing.
    /// </summary>
    protected static object CreateSettingsRecord(
        int pomodoroMinutes = 30,
        int shortBreakMinutes = 10,
        int longBreakMinutes = 20,
        bool soundEnabled = true,
        bool notificationsEnabled = true,
        bool autoStartEnabled = true,
        int autoStartDelaySeconds = 15)
    {
        return new
        {
            Id = Constants.Storage.DefaultSettingsId,
            PomodoroMinutes = pomodoroMinutes,
            ShortBreakMinutes = shortBreakMinutes,
            LongBreakMinutes = longBreakMinutes,
            SoundEnabled = soundEnabled,
            NotificationsEnabled = notificationsEnabled,
            AutoStartEnabled = autoStartEnabled,
            AutoStartDelaySeconds = autoStartDelaySeconds
        };
    }
}
