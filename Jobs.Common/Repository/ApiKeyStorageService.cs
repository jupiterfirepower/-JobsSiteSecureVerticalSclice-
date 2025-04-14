using System.Collections.Concurrent;
using Jobs.Common.Contracts;

namespace Jobs.Common.Repository;

public class ApiKeyStorageService<T> : IApiKeyStorageService<T> where T : class
{
    // Concurrent Dictionary Collection is Thread-Safe;
    // yet, it is a shared resource that requires protection in a multithreaded environment.
    private readonly ConcurrentDictionary<string, T> _concurrentDictionary = new();

    // Initializing the variable during class startup and preparing it for use later on,
    // this variable will hold the Singleton Instance.
    private static readonly ApiKeyStorageService<T> StorageService = new();

    // The Singleton Instance will be returned by the subsequent Static Method.
    // A thread-safe method that makes use of eager loading
    public static ApiKeyStorageService<T> GetInstance()
    {
        return StorageService;
    }

    // To prevent class instantiation from outside of this class, the constructor must be private.
    private ApiKeyStorageService()
    {
        Console.WriteLine("Singleton cache instance created.");
        Console.WriteLine();
    }

    // The Singleton Instance can be used to access the following methods from outside of the class.

    // A Key-Value Pair can be added to the Cache using this function.
    public bool Add(string key, T value)
    {
        return _concurrentDictionary.TryAdd(key, value);
    }

    // A Key-Value Pair can be added or updated into the Cache using this function.
    // Add the key-value pair if the key is unavailable.
    // Update the key's value if it has already been added.
    public bool AddOrUpdate(string key, T value)
    {
        if (_concurrentDictionary.ContainsKey(key))
        {
            _concurrentDictionary.TryRemove(key, out _);
        }

        return _concurrentDictionary.TryAdd(key, value);
    }

    // If the specified key is in the cache, this function is used to return its value.
    // If not, return null.
    public T Get(string key)
    {
        return _concurrentDictionary.GetValueOrDefault(key);
    }

    // Using this technique, the specified key is deleted from the cache.
    // Return true if removed; return false otherwise.
    public bool Remove(string key)
    {
        return _concurrentDictionary.TryRemove(key, out _);
    }

    // Using this technique, the specified key is deleted from the cache.
    // Return true if removed; return false otherwise.
    public void Clear()
    {
        // Eliminates every key and value from the Cache, or the ConcurrentDictionary.
        _concurrentDictionary.Clear();
    }

    public List<T> GetAll()
    {
        return _concurrentDictionary.Values.ToList();
    }

}