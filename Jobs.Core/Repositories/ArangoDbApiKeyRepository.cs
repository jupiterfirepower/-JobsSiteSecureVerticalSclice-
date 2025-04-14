using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;

namespace Jobs.Core.Repositories;

public class ArangoDbApiKeyRepository : IApiKeyStoreRepositoryExtended, IDisposable
{
    private readonly ArangoDBClient _client;
    private readonly string _collectionName = "akeys";
    private const string DbName = "jobs-api-keys";
    
    public ArangoDbApiKeyRepository(string connectionString, string userName, string password, string dbName = null, string collectionName = null)
    {
        var dbTransport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            dbName ?? DbName,
            userName,
            password);

        _collectionName = collectionName ?? _collectionName;
        
        _client = new ArangoDBClient(dbTransport);
    }
    
    private bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Add(ApiKey item)
    {
        Task.Run(async () => await AddAsync(item));
    }

    public async Task AddAsync(ApiKey item)
    {
        // Create document in the collection using strong type
        await _client.Document.PostDocumentAsync(
            _collectionName,
            item);
    }

    public ApiKey Get(string key)
    {
        var task = Task.Run(async () => await GetAsync(key));
        return task.GetAwaiter().GetResult();
    }

    public async Task<ApiKey> GetAsync(string key)
    {
        // Run AQL query (create a query cursor)
        var response = await _client.Cursor.PostCursorAsync<ApiKey>(
            @"FOR doc IN " + _collectionName + 
                   " FILTER doc.Key == '"+ key +"' RETURN doc");

        return response.Result.FirstOrDefault();
    }

    public void Remove(string key)
    {
        Task.Run(async () => await RemoveAsync(key));
    }

    public async Task RemoveAsync(string key)
    {
        // Run AQL query (create a query cursor)
        await _client.Cursor.PostCursorAsync<ApiKey>(
            @"FOR doc IN " + _collectionName + 
            " FILTER doc.Key == '"+ key + "'" +
            " REMOVE { _key: doc._key } IN "+ _collectionName);
    }

    public async Task<List<ApiKey>> GetAllAsync()
    {
        // Run AQL query (create a query cursor)
        var response = await _client.Cursor.PostCursorAsync<ApiKey>(
            @"FOR doc IN " + _collectionName + " RETURN doc");

        return response.Result.ToList();
    }
}