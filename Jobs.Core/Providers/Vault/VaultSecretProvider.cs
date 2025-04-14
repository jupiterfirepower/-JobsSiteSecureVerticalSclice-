using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace Jobs.Core.Providers.Vault;

public class VaultSecretProvider
{
    private readonly IVaultClient _vaultClient;

    public VaultSecretProvider(string vaultUri, string vaultToken)
    {
        var authMethod = new TokenAuthMethodInfo(vaultToken);
        var vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);
        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<string> GetSecretValueAsync(string path, string key, string mountPoint)
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: path, mountPoint: mountPoint);
        if (secret.Data.Data.TryGetValue(key, out var value))
        {
            return value.ToString();
        }
        throw new KeyNotFoundException($"Key '{key}' not found in Vault at path '{path}'");
    }
}