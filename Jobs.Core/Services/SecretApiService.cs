using Jobs.Core.Contracts;

namespace Jobs.Core.Services;

public class SecretApiService(string secretApi) : ISecretApiService
{
    public string SecretApi => secretApi;
}