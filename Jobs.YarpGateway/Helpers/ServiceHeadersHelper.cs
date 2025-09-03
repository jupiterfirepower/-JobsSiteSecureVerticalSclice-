using System.Collections.Concurrent;
using DotNetEnv;
using Jobs.Common.Constants;
using Jobs.Core.Providers.Vault;
using Jobs.Core.Services;
using Jobs.YarpGateway.Settings;
using VaultSharp;
using VaultSharp.Core;

namespace Jobs.YarpGateway.Helpers;

public class ServiceHeadersHelper
{
    private static readonly ConcurrentDictionary<string, string> Secrets = new();

    static ServiceHeadersHelper()
    {
        var vacData = LoadVaultSecretValues(ServiceNames.VacancyService,"secrets/services/vacancy").Result;
        var refData = LoadVaultSecretValues(ServiceNames.ReferenceService,"secrets/services/reference").Result;
        var companyData = LoadVaultSecretValues(ServiceNames.CompanyService,"secrets/services/company").Result;
        var accountData = LoadVaultSecretValues(ServiceNames.AccountService,"secrets/services/account").Result;
        
        AddDataToSecrets(vacData);
        AddDataToSecrets(refData);
        AddDataToSecrets(companyData);
        AddDataToSecrets(accountData);
    }

    private static void AddDataToSecrets(Dictionary<string, string> data)
    {
        foreach (var pair in data)
        {
            Secrets.TryAdd(pair.Key, pair.Value);
        }
    }

    private static async Task<Dictionary<string, string>> LoadVaultSecretValues(string serviceName, string path, string mountPoint = VaultSecretKeys.MountPoint)
    {
        // Load environment variables from the .env file
        Env.Load();
        //Env.TraversePath().Load();

        var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");
        Console.WriteLine($"VAULT_ADDR - {vaultUri}");

        vaultUri = AppSettings.VaultServerUrl;
        Console.WriteLine($"vaultUri: {vaultUri}");
        
        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);

            var vaultSecretKey =
                await vaultSecretsProvider.GetSecretValueAsync(path, VaultSecretKeys.SecretKey, mountPoint);
            Console.WriteLine($"vaultSecretKey: {vaultSecretKey}");
            var vaultDefaultApiKey =
                await vaultSecretsProvider.GetSecretValueAsync(path, VaultSecretKeys.DefaultApiKey, mountPoint);
            Console.WriteLine($"vaultDefaultApiKey: {vaultDefaultApiKey}");

            var aesCryptKey =
                await vaultSecretsProvider.GetSecretValueAsync(path, VaultSecretKeys.PKey, mountPoint);
            Console.WriteLine($"aesCryptKey: {aesCryptKey}");

            var aesCryptIv =
                await vaultSecretsProvider.GetSecretValueAsync(path, VaultSecretKeys.IV, mountPoint);
            Console.WriteLine($"aesCryptIV: {aesCryptIv}");

            var data = new Dictionary<string, string>
            {
                { $"{serviceName}{VaultSecretKeys.SecretKey}", vaultSecretKey },
                { $"{serviceName}{VaultSecretKeys.DefaultApiKey}", vaultDefaultApiKey },
                { $"{serviceName}Crypt{VaultSecretKeys.PKey}", aesCryptKey },
                { $"{serviceName}Crypt{VaultSecretKeys.IV}", aesCryptIv }
            };

            return data;
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        throw new InvalidDataException("error get vault secret keys from vault server.");
    }
    public (string, string, string) GetHeadersValues(string serviceName, string apiKey)
    {
        try
        {
            var resultSecretKey = Secrets.TryGetValue($"{serviceName}{VaultSecretKeys.SecretKey}", out var vaultSecretKey);
            Console.WriteLine($"{serviceName}{VaultSecretKeys.SecretKey} - {resultSecretKey}");
            if (resultSecretKey &&
                Secrets.TryGetValue($"{serviceName}{VaultSecretKeys.DefaultApiKey}", out var vaultDefaultApiKey) &&
                Secrets.TryGetValue($"{serviceName}Crypt{VaultSecretKeys.PKey}", out var aesCryptKey) &&
                Secrets.TryGetValue($"{serviceName}Crypt{VaultSecretKeys.IV}", out var aesCryptIv))
            {
                var encryptionService = new NaiveEncryptionService(Convert.FromBase64String(aesCryptKey),
                    Convert.FromBase64String(aesCryptIv));
            
                var protectedSecretKey = encryptionService.Encrypt(vaultSecretKey);
                var protectedDefaultApiKey = !string.IsNullOrEmpty(apiKey) ? apiKey : encryptionService.Encrypt(vaultDefaultApiKey);
                var nonce = NonceHelper.GenerateNonce();
                var protectedNonce = encryptionService.Encrypt(nonce);
            
                return (protectedDefaultApiKey, protectedSecretKey, protectedNonce);
            }

            throw new InvalidDataException("error get vault secret keys from store.");
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        throw new InvalidDataException("error get vault secret keys from store.");
    }
}

