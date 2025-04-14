using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Services;
using Jobs.Core.DataModel;
using Jobs.Core.Services;

namespace Jobs.Core.Providers;

public class MemoryPasswordStorageProvider(IEncryptionService cryptoService) : IPasswordStorageProvider
{
    private readonly IConcurrentStorageService<ExternalUserCredential> _cache = ConcurrentStorageService<ExternalUserCredential>.GetInstance();
    
    public void AddUserCredential(ExternalUserCredential data)
    {
        var item = _cache.Get(data.Email);
        if (item == null)
        {
            data.Password = cryptoService.Encrypt(data.Password);
            _cache.Add(data.Email, data);
        }
    }

    public ExternalUserCredential GetUserCredential(string email)
    {
        var item = _cache.Get(email);
        if (item != null)
        {
            item.Password = cryptoService.Decrypt(item.Password);
        }

        return item;
    }
}