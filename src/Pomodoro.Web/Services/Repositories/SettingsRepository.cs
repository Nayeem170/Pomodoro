using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Repositories;

/// <summary>
/// Repository implementation for settings persistence using IndexedDB
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly IIndexedDbService _indexedDb;

    public SettingsRepository(IIndexedDbService indexedDb)
    {
        _indexedDb = indexedDb;
    }

    public async Task<TimerSettings?> GetAsync()
    {
        var record = await _indexedDb.GetAsync<TimerSettingsRecord>(Constants.Storage.SettingsStore, Constants.Storage.DefaultSettingsId);
        if (record == null) return null;

        return new TimerSettings
        {
            PomodoroMinutes = record.PomodoroMinutes,
            ShortBreakMinutes = record.ShortBreakMinutes,
            LongBreakMinutes = record.LongBreakMinutes,
            SoundEnabled = record.SoundEnabled,
            NotificationsEnabled = record.NotificationsEnabled,
            AutoStartEnabled = record.AutoStartEnabled,
            AutoStartDelaySeconds = record.AutoStartDelaySeconds
        };
    }

    public async Task<bool> SaveAsync(TimerSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        var record = new TimerSettingsRecord
        {
            Id = Constants.Storage.DefaultSettingsId,
            PomodoroMinutes = settings.PomodoroMinutes,
            ShortBreakMinutes = settings.ShortBreakMinutes,
            LongBreakMinutes = settings.LongBreakMinutes,
            SoundEnabled = settings.SoundEnabled,
            NotificationsEnabled = settings.NotificationsEnabled,
            AutoStartEnabled = settings.AutoStartEnabled,
            AutoStartDelaySeconds = settings.AutoStartDelaySeconds
        };
        return await _indexedDb.PutAsync(Constants.Storage.SettingsStore, record);
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaults = new TimerSettings();
        await SaveAsync(defaults);
    }
}

/// <summary>
/// Record for storing timer settings in IndexedDB
/// </summary>
public class TimerSettingsRecord
{
    public string Id { get; set; } = Constants.Storage.DefaultSettingsId;
    public int PomodoroMinutes { get; set; }
    public int ShortBreakMinutes { get; set; }
    public int LongBreakMinutes { get; set; }
    public bool SoundEnabled { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool AutoStartEnabled { get; set; }
    public int AutoStartDelaySeconds { get; set; }
}
