using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public class GoogleTasksService : IGoogleTasksService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly ILogger<GoogleTasksService> _logger;

    public GoogleTasksService(IJSRuntime jsRuntime, IGoogleDriveService googleDriveService, ILogger<GoogleTasksService> logger)
    {
        _jsRuntime = jsRuntime;
        _googleDriveService = googleDriveService;
        _logger = logger;
    }

    public Task<bool> IsConnectedAsync() => Task.FromResult(_googleDriveService.IsConnected);

    public async Task<IReadOnlyList<GoogleTaskList>> GetTaskListsAsync()
    {
        return await ExecuteWithRetryAsync(async token =>
        {
            var json = await _jsRuntime.InvokeAsync<string>(Constants.GoogleTasksJsFunctions.ListTaskLists, token);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var items = data.GetProperty("items").EnumerateArray();
            var result = new List<GoogleTaskList>();
            foreach (var item in items)
            {
                result.Add(new GoogleTaskList
                {
                    Id = item.GetProperty("id").GetString() ?? string.Empty,
                    Title = item.GetProperty("title").GetString() ?? string.Empty
                });
            }
            _logger.LogInformation(Constants.SyncMessages.LogTasksListSuccess, result.Count);
            return result;
        });
    }

    public async Task<IReadOnlyList<GoogleTask>> GetTasksAsync(string listId, string? updatedMin = null)
    {
        return await ExecuteWithRetryAsync(async token =>
        {
            var options = new { updatedMin, showDeleted = false };
            var json = await _jsRuntime.InvokeAsync<string>(Constants.GoogleTasksJsFunctions.ListTasks, token, listId, options);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            var items = data.EnumerateArray();
            var result = new List<GoogleTask>();
            foreach (var item in items)
            {
                result.Add(MapGoogleTask(item));
            }
            _logger.LogInformation(Constants.SyncMessages.LogTasksListSuccess, result.Count);
            return result;
        });
    }

    private static GoogleTask MapGoogleTask(JsonElement item)
    {
        string? due = null;
        if (item.TryGetProperty("due", out var dueProp) && dueProp.ValueKind != JsonValueKind.Null)
            due = dueProp.GetString();

        string? notes = null;
        if (item.TryGetProperty("notes", out var notesProp) && notesProp.ValueKind != JsonValueKind.Null)
            notes = notesProp.GetString();

        string? parent = null;
        if (item.TryGetProperty("parent", out var parentProp) && parentProp.ValueKind != JsonValueKind.Null)
            parent = parentProp.GetString();

        string? position = null;
        if (item.TryGetProperty("position", out var positionProp) && positionProp.ValueKind != JsonValueKind.Null)
            position = positionProp.GetString();

        string? etag = null;
        if (item.TryGetProperty("etag", out var etagProp) && etagProp.ValueKind != JsonValueKind.Null)
            etag = etagProp.GetString();

        return new GoogleTask
        {
            Id = item.GetProperty("id").GetString() ?? string.Empty,
            Title = item.GetProperty("title").GetString() ?? string.Empty,
            Notes = notes,
            Due = due,
            Status = item.GetProperty("status").GetString() ?? "needsAction",
            Updated = item.GetProperty("updated").GetString() ?? string.Empty,
            Parent = parent,
            Position = position,
            ETag = etag
        };
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<string, Task<T>> action, int maxRetries = 3)
    {
        var token = await _googleDriveService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired);

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await action(token);
            }
            catch (JSException ex) when (ex.Message.Contains("401"))
            {
                _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
                throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired, ex);
            }
            catch (JSException ex) when (ex.Message.Contains("403"))
            {
                _logger.LogWarning(ex, Constants.SyncMessages.LogTasksForbidden);
                throw new UnauthorizedAccessException(Constants.SyncMessages.TasksAccessForbidden, ex);
            }
            catch (JSException ex) when (ex.Message.Contains("429"))
            {
                if (attempt < maxRetries - 1)
                {
                    var delay = (int)Math.Pow(2, attempt) * 1000;
                    _logger.LogWarning(Constants.SyncMessages.LogTasksRateLimited);
                    await Task.Delay(delay);
                    token = await _googleDriveService.GetAccessTokenAsync();
                    if (string.IsNullOrEmpty(token))
                        throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired);
                }
                else
                {
                    _logger.LogWarning(Constants.SyncMessages.LogTasksRateLimited);
                    throw new UnauthorizedAccessException(Constants.SyncMessages.TasksRateLimitExceeded, ex);
                }
            }
            catch (JSException ex)
            {
                _logger.LogError(ex, Constants.SyncMessages.LogTasksApiError, ex.Message);
                throw;
            }
        }

        throw new UnauthorizedAccessException(Constants.SyncMessages.TasksUnavailable);
    }
}
