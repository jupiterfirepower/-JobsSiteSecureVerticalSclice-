using ArangoDBNetStandard;
using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.DatabaseApi;
using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.Transport.Http;

namespace Jobs.Core.Repositories;

public class ArangoDbAdditionalDbOperations(string connectionString, string sysUserName, 
    string sysPassword, string userName, string password, 
    string dbName = null, string collectionName = null)
{
    private const string CollectionName = "akeys";
    private const string DbName = "jobs-api-keys";
    private const string SysDbName = "_system";
    
    public async Task CreateDbAsync()
    {
        // You must use the _system database to create databases
        using var systemDbTransport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString), // "http://localhost:8529/"
            SysDbName,
            sysUserName,
            sysPassword);
        var systemDb = new DatabaseApiClient(systemDbTransport);

        // Create a new database with one user.
        var task = await systemDb.PostDatabaseAsync(
                new PostDatabaseBody
                {
                    Name = dbName ?? DbName,
                    Users = new List<DatabaseUser>
                    {
                        new()
                        {
                            Username = userName,
                            Passwd = password
                        }
                    }
                });
    }
    
    public async Task CreateCollectionAsync()
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            dbName ?? DbName,
            userName,
            password);

        using var client = new ArangoDBClient(transport);

        // Create a collection in the database
        await client.Collection.PostCollectionAsync(
            new PostCollectionBody
            {
                Name = collectionName ?? CollectionName
                // A whole heap of other options exist to define key options, 
                // sharding options, etc
            });
    }
    
    public async Task DeleteDbAsync()
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            SysDbName,
            sysUserName,
            sysPassword);

        using var client = new ArangoDBClient(transport);

        // Delete database
        await client.Database.DeleteDatabaseAsync(dbName ?? DbName);
    }
    
    public async Task DeleteDbAsync(string databaseName)
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            SysDbName,
            sysUserName,
            sysPassword);

        using var client = new ArangoDBClient(transport);

        // Delete database
        await client.Database.DeleteDatabaseAsync(databaseName);
    }
    
    public async Task<GetDatabasesResponse> GetDatabasesAsync()
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            SysDbName,
            sysUserName,
            sysPassword);

        using var client = new ArangoDBClient(transport);

        // Get databases
        return await client.Database.GetDatabasesAsync();
    }
    
    public async Task RemoveAllDatabasesAsync()
    {
        var transport = HttpApiTransport.UsingBasicAuth(
            new Uri(connectionString),
            SysDbName,
            sysUserName,
            sysPassword);

        using var client = new ArangoDBClient(transport);

        // Get databases
        var response = await client.Database.GetDatabasesAsync();
        
        // Optimized PipeLine bottom style
        foreach (var current in response.Result.Where(x=>!x.Equals(SysDbName)))
        {
            _ = DeleteDbAsync(current);
        }
        /*response.Result
            .Where(x=>!x.Equals(SysDbName))
            .ToList()
            .ForEach(x=> _ = DeleteDbAsync(x));*/
    }
}
