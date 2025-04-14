using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts;

public interface IPwdStoreRepository
{
    Task AddAsync(ExternalUserCredential credential);
    Task<ExternalUserCredential> GetAsync(string email);

    void Add(ExternalUserCredential credential);
    ExternalUserCredential Get(string email);
}