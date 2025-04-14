using Jobs.Common.Contracts;

namespace Jobs.Common.Services;

public class SecretApiService(string secretApi) : ISecretApiService
{
    public string SecretApi => secretApi;
}