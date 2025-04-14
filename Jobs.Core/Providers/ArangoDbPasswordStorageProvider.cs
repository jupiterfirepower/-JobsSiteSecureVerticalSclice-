using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Repositories;

namespace Jobs.Core.Providers;

public class ArangoDbPasswordStorageProvider(IEncryptionService cryptoService, IPwdStoreRepository repository) : IPasswordStorageProvider, IDisposable
{
    public void AddUserCredential(ExternalUserCredential data)
    {
        data.Password = cryptoService.Decrypt(data.Password);
        repository.Add(data);
    }

    public ExternalUserCredential GetUserCredential(string email)
    {
        var data = repository.Get(email);
        data.Password = cryptoService.Decrypt(data.Password);
        return data;
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                (repository as IDisposable)?.Dispose();
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