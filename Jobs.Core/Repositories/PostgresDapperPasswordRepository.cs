using Dapper;
using Jobs.Core.Contracts;
using Jobs.Core.DataModel;
using Npgsql;

namespace Jobs.Core.Repositories;

public class PostgresDapperPasswordRepository(string connectionString) : IPwdStoreRepository, IDisposable
{
        private readonly NpgsqlConnection _connection = new (connectionString); // ;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20;"
        
        private const string AddCommandText = "call sp_add_credentials(@email,@pwd);";
        // private const string SelectCommandText = "SELECT \"Email\", \"Pwd\" as \"Password\" FROM fn_get_credentials_by_email(@email);";
        private const string SelectCommandText = "SELECT \"Email\", \"Password\" FROM fn_get_credentials_by_email(@email);";

        public async Task AddAsync(ExternalUserCredential credential)
        {
            //await using var connection = new NpgsqlConnection(connectionString);
            try
            {
                await _connection.OpenAsync();
            
                var queryArguments = new
                {
                    email = credential.Email,
                    pwd = credential.Password
                };

                await _connection.ExecuteAsync(AddCommandText, queryArguments);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        
        public void Add(ExternalUserCredential credential)
        {
            //using var connection = new NpgsqlConnection(connectionString);
            try
            {
                _connection.Open();
            
                var queryArguments = new
                {
                    email = credential.Email,
                    pwd = credential.Password
                };

                _connection.Execute(AddCommandText, queryArguments);
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<ExternalUserCredential> GetAsync(string email)
        {
            //await using var connection = new NpgsqlConnection(connectionString);
            try
            {
                await _connection.OpenAsync();
                var queryArgs = new { email = email };
                return await _connection.QueryFirstOrDefaultAsync<ExternalUserCredential>(SelectCommandText, queryArgs);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
        
        public ExternalUserCredential Get(string email)
        {
            using var connection = new NpgsqlConnection(connectionString);
            try
            {
                connection.Open();
                var queryArgs = new { email = email };
                return connection.QueryFirstOrDefault<ExternalUserCredential>(SelectCommandText, queryArgs);
            }
            finally
            {
                _connection.Close();
            }
        }
        
        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
}