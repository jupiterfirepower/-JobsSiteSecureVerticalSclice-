using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts.Providers;

public interface IPasswordStorageProvider
{
    void AddUserCredential(ExternalUserCredential data);

    ExternalUserCredential GetUserCredential(string email);
}