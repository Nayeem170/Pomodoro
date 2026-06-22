namespace Pomodoro.Web.Models;

public record GoogleTasksSettings(Dictionary<string, ListSetting> Lists);

public record ListSetting(bool IsVisible, string Color, DateTime? LastSync);
