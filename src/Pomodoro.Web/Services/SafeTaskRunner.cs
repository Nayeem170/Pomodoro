using Microsoft.Extensions.Logging;

namespace Pomodoro.Web.Services;

/// <summary>
/// Provides safe fire-and-forget task execution with exception logging.
/// Use this instead of discard pattern to ensure exceptions are logged.
/// </summary>
public static class SafeTaskRunner
{
    /// <summary>
    /// Runs a task asynchronously without awaiting, logging any exceptions.
    /// </summary>
    /// <param name="task">The async task to execute.</param>
    /// <param name="logger">The logger to use for exception logging.</param>
    /// <param name="operationName">A descriptive name for the operation for logging purposes.</param>
    public static void RunAndForget(
        Func<Task> task,
        ILogger logger,
        string operationName = Constants.SafeTaskOperations.UnknownOperation)
    {
        _ = ExecuteSafelyAsync(task, logger, operationName);
    }

    private static async Task ExecuteSafelyAsync(Func<Task> task, ILogger logger, string operationName)
    {
        try
        {
            await task().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, Constants.SafeTaskOperations.ErrorInOperationLogFormat, operationName);
        }
    }

    /// <summary>
    /// Runs a task asynchronously without awaiting, logging any exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The async task to execute.</param>
    /// <param name="logger">The logger to use for exception logging.</param>
    /// <param name="operationName">A descriptive name for the operation for logging purposes.</param>
    /// <param name="onSuccess">Optional callback when the task succeeds.</param>
    public static void RunAndForget<T>(
        Func<Task<T>> task,
        ILogger logger,
        string operationName = Constants.SafeTaskOperations.UnknownOperation,
        Action<T>? onSuccess = null)
    {
        _ = ExecuteSafelyAsync(task, logger, operationName, onSuccess);
    }

    private static async Task ExecuteSafelyAsync<T>(Func<Task<T>> task, ILogger logger, string operationName, Action<T>? onSuccess)
    {
        try
        {
            var result = await task().ConfigureAwait(false);
            onSuccess?.Invoke(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, Constants.SafeTaskOperations.ErrorInOperationLogFormat, operationName);
        }
    }
}
