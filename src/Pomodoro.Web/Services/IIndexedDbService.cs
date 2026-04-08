namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for IndexedDB operations
/// Provides async persistent storage for the application
/// </summary>
public interface IIndexedDbService
{
    /// <summary>
    /// Event raised when a storage error occurs
    /// </summary>
    event Action<string>? OnStorageError;
    
    /// <summary>
    /// Initialize the database and create schema if needed
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Get a single item by key from a store
    /// </summary>
    Task<T?> GetAsync<T>(string storeName, string key);
    
    /// <summary>
    /// Get all items from a store
    /// </summary>
    Task<List<T>> GetAllAsync<T>(string storeName);
    
    /// <summary>
    /// Query items by index value
    /// </summary>
    Task<List<T>> QueryByIndexAsync<T>(string storeName, string indexName, object value);
    
    /// <summary>
    /// Query items by date range using an index
    /// </summary>
    Task<List<T>> QueryByDateRangeAsync<T>(string storeName, string indexName, string startDate, string endDate);
    
    /// <summary>
    /// Add or update an item in a store
    /// </summary>
    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> PutAsync<T>(string storeName, T item);
    
    /// <summary>
    /// Add or update multiple items in a store
    /// </summary>
    /// <returns>True if all items were saved successfully, false if any error occurred</returns>
    Task<bool> PutAllAsync<T>(string storeName, List<T> items);
    
    /// <summary>
    /// Delete an item by key from a store
    /// </summary>
    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> DeleteAsync(string storeName, string key);
    
    /// <summary>
    /// Clear all items from a store
    /// </summary>
    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> ClearAsync(string storeName);
    
    /// <summary>
    /// Get count of items in a store
    /// </summary>
    Task<int> GetCountAsync(string storeName);
    
    /// <summary>
    /// Initializes JavaScript constants with user settings for consistency across JS interop
    /// </summary>
    Task InitializeJsConstantsAsync(int pomodoroMinutes, int shortBreakMinutes, int longBreakMinutes);
}
