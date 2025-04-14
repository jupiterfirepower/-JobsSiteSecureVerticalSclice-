using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class PostgresPasswordStorageProvider(IPwdStoreRepository repository): IPasswordStorageProvider
{
    public void AddUserCredential(ExternalUserCredential data)
    {
        var item = repository.Get(data.Email);
        if(item == null)
            repository.Add(data);
    }

    public ExternalUserCredential GetUserCredential(string email) => repository.Get(email);
    
}