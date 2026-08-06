namespace Pomodoro.Web.Services;

public interface IIndexedDbService
{
    event Action<string>? OnStorageError;

    Task InitializeAsync();

    Task<T?> GetAsync<T>(string storeName, string key);

    Task<List<T>> GetAllAsync<T>(string storeName);

    Task<List<T>> QueryByIndexAsync<T>(string storeName, string indexName, object value);

    Task<List<T>> QueryByDateRangeAsync<T>(string storeName, string indexName, string startDate, string endDate);

    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> PutAsync<T>(string storeName, T item);

    /// <returns>True if all items were saved successfully, false if any error occurred</returns>
    Task<bool> PutAllAsync<T>(string storeName, List<T> items);

    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> DeleteAsync(string storeName, string key);

    /// <returns>True if successful, false if an error occurred</returns>
    Task<bool> ClearAsync(string storeName);

    Task<int> GetCountAsync(string storeName);

    Task InitializeJsConstantsAsync(int pomodoroMinutes, int shortBreakMinutes, int longBreakMinutes);
}
