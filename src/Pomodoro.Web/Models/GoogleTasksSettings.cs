namespace Pomodoro.Web.Models;

public record GoogleTasksSettings(Dictionary<string, ListSetting> Lists, List<string>? ListIds = null)
{
    public string Id { get; init; } = Constants.Storage.DefaultSettingsId;
}

public record ListSetting(bool IsVisible, string Color, DateTime? LastSync);
