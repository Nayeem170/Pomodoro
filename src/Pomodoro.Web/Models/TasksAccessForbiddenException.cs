namespace Pomodoro.Web.Models;

public sealed class TasksAccessForbiddenException : Exception
{
    public TasksAccessForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
