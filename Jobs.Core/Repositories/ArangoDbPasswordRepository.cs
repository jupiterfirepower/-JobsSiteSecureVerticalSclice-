using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;
using Jobs.Core.Contracts;
using Jobs.Core.DataModel;

namespace Jobs.Core.Repositories;

public class ArangoDbPasswordRepository : IPwdStoreRepository, IDisposable
{
    private readonly ArangoDBClient _client;
    private readonly string _collectionName = "pwd";
    private const string DbName = "jobs-pwd";
    
    public ArangoDbPasswordRepository(string connectionString, string userName, string password, string dbName = null, string collectionName = null)
    {
        var dbTransport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            dbName ?? DbName,
            userName,
            password);

        _collectionName = collectionName ?? _collectionName;
        
        _client = new ArangoDBClient(dbTransport);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                //_connection.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task AddAsync(ExternalUserCredential credential)
    {
        // Create document in the collection using strong type
        await _client.Document.PostDocumentAsync(
            _collectionName,
            credential);
    }
    
    public async Task<ExternalUserCredential> GetAsync(string email)
    {
        // Run AQL query (create a query cursor)
        var response = await _client.Cursor.PostCursorAsync<ExternalUserCredential>(
            @"FOR doc IN " + _collectionName + 
            " FILTER doc.Email == '"+ email +"' RETURN doc");

        return response.Result.FirstOrDefault();
    }

    public void Add(ExternalUserCredential credential)
    {
        Task.Run(async () => await AddAsync(credential));
    }

    public ExternalUserCredential Get(string email)
    {
        var task = Task.Run(async () => await GetAsync(email));
        return task.GetAwaiter().GetResult();
    }
}