using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Managers;

public class PasswordManagerServiceProvider(IPasswordStorageProvider backendProvider) : IPasswordManagerServiceProvider
{
    public void AddUserCredential(ExternalUserCredential data) => backendProvider.AddUserCredential(data);

    public ExternalUserCredential GetUserCredential(string email) => backendProvider.GetUserCredential(email);
}