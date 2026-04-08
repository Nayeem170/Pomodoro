using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for IndexedDB operations via JavaScript interop
/// Provides persistent storage with larger capacity than localStorage
/// </summary>
public class IndexedDbService : IIndexedDbService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<IndexedDbService> _logger;
    private bool _isInitialized;

    /// <summary>
    /// Event raised when a storage error occurs
    /// </summary>
    public event Action<string>? OnStorageError;

    public IndexedDbService(IJSRuntime jsRuntime, ILogger<IndexedDbService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // The indexedDbInterop.js is loaded via script tag in index.html, so we use _jsRuntime directly
            // rather than importing as ES module (the JS uses window.indexedDbInterop pattern)
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.InitDatabase);
            _isInitialized = true;
            _logger.LogDebug(Constants.Messages.LogInitializedSuccessfully);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogFailedToInitialize);
            throw;
        }
    }
    
    /// <summary>
    /// Initializes JavaScript constants with user settings for consistency across JS interop
    /// </summary>
    public async Task InitializeJsConstantsAsync(int pomodoroMinutes, int shortBreakMinutes, int longBreakMinutes)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.PomodoroConstantsInitialize, new
            {
                pomodoroMinutes,
                shortBreakMinutes,
                longBreakMinutes
            });
            _logger.LogDebug(Constants.Messages.LogJsConstantsInitializedFormat,
                pomodoroMinutes, shortBreakMinutes, longBreakMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.Messages.LogFailedToInitializeJsConstants);
        }
    }

    public async Task<T?> GetAsync<T>(string storeName, string key)
    {
        EnsureInitialized();
        
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItem, storeName, key);
            if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                return default;
            
            return DeserializeItem<T>(result, storeName, key);
        }
        catch (Exception ex)
        {
            // Log error for debugging - storage failures should be investigated
            _logger.LogError(ex, Constants.Messages.LogErrorGettingItem, storeName, key);
            // Notify subscribers of the error
            NotifyStorageError(string.Format(Constants.Messages.LogFailedToGetItem, storeName, ex.Message));
            return default;
        }
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        EnsureInitialized();
        
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetAllItems, storeName);
            if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                return new List<T>();
            
            return DeserializeList<T>(result, storeName, "all");
        }
        catch (Exception ex)
        {
            // Log error for debugging - storage failures should be investigated
            _logger.LogError(ex, Constants.Messages.LogErrorGettingAllItems, storeName);
            // Notify subscribers of the error
            NotifyStorageError(string.Format(Constants.Messages.LogFailedToGetAllItems, storeName, ex.Message));
            return new List<T>();
        }
    }

    public async Task<List<T>> QueryByIndexAsync<T>(string storeName, string indexName, object value)
    {
        EnsureInitialized();
        
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByIndex, storeName, indexName, value);
            if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                return new List<T>();
            
            return DeserializeList<T>(result, storeName, indexName);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, Constants.Messages.LogErrorQueryingIndex, indexName);
            return new List<T>();
        }
    }

    public async Task<List<T>> QueryByDateRangeAsync<T>(string storeName, string indexName, string startDate, string endDate)
    {
        EnsureInitialized();
        
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>(Constants.IndexedDbJsFunctions.GetItemsByDateRange, storeName, indexName, startDate, endDate);
            if (result.ValueKind == JsonValueKind.Null || result.ValueKind == JsonValueKind.Undefined)
                return new List<T>();
            
            return DeserializeList<T>(result, storeName, indexName);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, Constants.Messages.LogErrorQueryingDateRange);
            return new List<T>();
        }
    }

    public async Task<bool> PutAsync<T>(string storeName, T item)
    {
        EnsureInitialized();
        
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.PutItem, storeName, item);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogErrorPuttingItem, storeName);
            return false;
        }
    }

    /// <summary>
    /// Puts all items in a single transaction for better performance.
    /// Note: Uses a single transaction for atomicity - if one item fails, all changes are rolled back.
    /// </summary>
    public async Task<bool> PutAllAsync<T>(string storeName, List<T> items)
    {
        EnsureInitialized();
        
        if (items == null || items.Count == 0)
            return true;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.PutAllItems, storeName, items);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogErrorPuttingAllItems, storeName);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string storeName, string key)
    {
        EnsureInitialized();
        
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.DeleteItem, storeName, key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogErrorDeletingItem, storeName);
            return false;
        }
    }

    public async Task<bool> ClearAsync(string storeName)
    {
        EnsureInitialized();
        
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.IndexedDbJsFunctions.ClearStore, storeName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogErrorClearingStore, storeName);
            return false;
        }
    }

    public async Task<int> GetCountAsync(string storeName)
    {
        EnsureInitialized();
        
        try
        {
            return await _jsRuntime.InvokeAsync<int>(Constants.IndexedDbJsFunctions.GetCount, storeName);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, Constants.Messages.LogErrorGettingCount, storeName);
            return 0;
        }
    }

    public ValueTask DisposeAsync()
    {
        // No module to dispose since we're using _jsRuntime directly
        return ValueTask.CompletedTask;
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException(Constants.Messages.IndexedDbNotInitialized);
        }
    }
    
    /// <summary>
    /// Notifies subscribers of storage errors
    /// </summary>
    private void NotifyStorageError(string errorMessage)
    {
        OnStorageError?.Invoke(errorMessage);
    }

    private T? DeserializeItem<T>(JsonElement result, string storeName, string key)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(result.GetRawText(), _jsonOptions);
        }
        catch (JsonException jsonEx)
        {
            HandleDeserializationError(jsonEx, storeName, key);
            return default;
        }
    }

    private List<T> DeserializeList<T>(JsonElement result, string storeName, string key)
    {
        try
        {
            var list = JsonSerializer.Deserialize<List<T>>(result.GetRawText(), _jsonOptions);
            return list ?? new List<T>();
        }
        catch (JsonException jsonEx)
        {
            HandleDeserializationError(jsonEx, storeName, key);
            return new List<T>();
        }
    }

    private void HandleDeserializationError(JsonException jsonEx, string storeName, string key)
    {
        _logger.LogWarning(jsonEx, Constants.Messages.LogJsonDeserializationFailed, storeName, key);
        NotifyStorageError($"Data corruption detected in {storeName}");
    }
}
