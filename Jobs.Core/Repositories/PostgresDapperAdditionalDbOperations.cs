using Dapper;
using Npgsql;

namespace Jobs.Core.Repositories;

public class PostgresDapperAdditionalDbOperations
{
    private readonly NpgsqlConnection _connection;

    public PostgresDapperAdditionalDbOperations(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public async Task CreateTableIfNotExists()
    {
        var sql = $"CREATE TABLE if not exists UserPwdStore" +
                  $"(" +
                  $"Id serial PRIMARY KEY, " +
                  $"Email VARCHAR (100) NOT NULL, " +
                  $"Pwd VARCHAR (100) NOT NULL " +
                  $")";

        await _connection.ExecuteAsync(sql);
    }

    public async Task<string> GetVersion()
    {
        var commandText = "SELECT version()";

        var value = await _connection.ExecuteScalarAsync<string>(commandText);
        return value;
    }
}