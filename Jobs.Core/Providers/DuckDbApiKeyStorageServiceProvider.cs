using Jobs.Core.Contracts.Providers;
using DuckDB.NET.Data;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class DuckDbApiKeyStorageServiceProvider : IApiKeyStorageServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly DuckDBConnection _duckDbConnection = new("Data Source=:memory:");
    
    public DuckDbApiKeyStorageServiceProvider()
    {
        _duckDbConnection.Open();
        
        using var command = _duckDbConnection.CreateCommand();
        command.CommandText = "CREATE TABLE apikeys (id INTEGER PRIMARY KEY, apikey Text NOT NULL, expired TIMESTAMP DEFAULT NULL);";
        command.ExecuteNonQuery();
        
        using var commandSequence = _duckDbConnection.CreateCommand();
        commandSequence.CommandText = "CREATE SEQUENCE serial START WITH 1 INCREMENT BY 1;";
        commandSequence.ExecuteNonQuery();
    }
    
    private DuckDBCommand GetExpiredCountCommand(string key)
    {
        var commandGetCount = _duckDbConnection.CreateCommand();
        commandGetCount.CommandText = commandGetCount.CommandText = 
            "SELECT expired, Count(*) FROM apikeys WHERE apikey = $ApiKey GROUP BY expired;"; 
        
        //Values added to parameters
        commandGetCount.Parameters.Add(new DuckDBParameter("ApiKey", key));
        
        return commandGetCount;
    }

    private DuckDBCommand GetDeleteApiKeyCommand(string key)
    {
        var commandDelete = _duckDbConnection.CreateCommand();
        commandDelete.CommandText = "DELETE FROM apikeys WHERE apikey = $ApiKey;";
        commandDelete.Parameters.Add(new DuckDBParameter("ApiKey", key));
        return commandDelete;
    }

    private bool IsApiKeyValidWithDelete(DateTime? expiredValue, string key)
    {
        if (expiredValue.HasValue && DateTime.Now <= expiredValue.Value)
        {
            using var commandDelete = GetDeleteApiKeyCommand(key);
            var rowsDeleted = commandDelete.ExecuteNonQuery();
            Console.WriteLine($"Rows Deleted = {rowsDeleted}");
            
            return true;
        }
        
        return false;
    }
    
    private async Task<bool> IsApiKeyValidWithDeleteAsync(DateTime? expiredValue, string key)
    {
        if (expiredValue.HasValue && DateTime.Now <= expiredValue.Value)
        {
            await using var commandDelete = GetDeleteApiKeyCommand(key);
            var rowsDeleted = await commandDelete.ExecuteNonQueryAsync();
            Console.WriteLine($"Rows Deleted = {rowsDeleted}");
            
            return true;
        }
        
        return false;
    }
    
    public bool IsKeyValid(string key)
    {
        Console.WriteLine($"Key = {key}");

        using var commandExpCount = GetExpiredCountCommand(key);
        
        DateTime? expiredValue = null;
        long countValue = 0;
            
        var reader = commandExpCount.ExecuteReader();
        
        while (reader.Read())
        {
            expiredValue = reader.GetDateTime(0);
            countValue = reader.GetInt64(1);
        }
       
        if(countValue > 0 && expiredValue == null)
        {
            return true;
        }

        return IsApiKeyValidWithDelete(expiredValue, key);
    }
    
    public bool IsDefaultKeyValid(string key)
    {
        Console.WriteLine($"Key = {key}");

        using var commandExpCount = GetExpiredCountCommand(key);
        
        DateTime? expiredValue = null;
        long countValue = 0;
            
        var reader = commandExpCount.ExecuteReader();
        
        while (reader.Read())
        {
            expiredValue = reader.GetDateTime(0);
            countValue = reader.GetInt64(1);
        }
       
        if(countValue > 0 && expiredValue == null)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        await using var commandExpCount = GetExpiredCountCommand(key);
        
        DateTime? expiredValue = null;
        long countValue = 0;
            
        var reader = await commandExpCount.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            expiredValue = reader.GetDateTime(0);
            countValue = reader.GetInt64(1);
        }
       
        if(countValue > 0 && expiredValue == null)
        {
            return true;
        }

        return await IsApiKeyValidWithDeleteAsync(expiredValue, key);
    }
    
    public void AddApiKey(ApiKey key)
    {
        using var commandInsert = GetAddApiKeyCommand(key);
        var rowsChanged = commandInsert.ExecuteNonQuery();
        Console.WriteLine($"Rows Changed = {rowsChanged}");
    }

    private DuckDBCommand GetAddApiKeyCommand(ApiKey key)
    {
        var commandInsert = _duckDbConnection.CreateCommand();
        commandInsert.CommandText = "INSERT INTO apikeys (id,apikey,expired) VALUES (nextval('serial'),$ApiKey,$Expired)";

        commandInsert.Parameters.Add(new DuckDBParameter("ApiKey", key.Key));
        commandInsert.Parameters.Add(new DuckDBParameter("Expired", key.Expiration == null ? DBNull.Value : key.Expiration));
        
        return commandInsert;
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        await using var commandInsert = GetAddApiKeyCommand(key);
        var rowsChanged = await commandInsert.ExecuteNonQueryAsync();
        Console.WriteLine($"Rows Changed = {rowsChanged}");
        
        return await Task.FromResult(true);
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        await using var commandDelete = _duckDbConnection.CreateCommand();
        commandDelete.CommandText = "DELETE FROM apikeys WHERE Expired < now();"; 
        var rowsDeleted = await commandDelete.ExecuteNonQueryAsync();
        Console.WriteLine($"Rows Deleted = {rowsDeleted}");
        return await Task.FromResult(rowsDeleted);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _duckDbConnection?.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() => await DisposeAsync(true);
    
    private async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_duckDbConnection != null) await _duckDbConnection.DisposeAsync();
            }
        }
        _disposed = true;
    }
}