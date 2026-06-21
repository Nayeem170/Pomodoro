namespace Pomodoro.Web.Models;

public record TaskListRef(string Id, string Title, string Color, int Count, bool IsVisible, bool IsLocal);
