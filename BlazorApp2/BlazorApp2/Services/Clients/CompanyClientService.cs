using System.Text;
using System.Text.Json;
using BlazorApp2.Contracts.Clients;
using BlazorApp2.Settings;
using Jobs.Common.Helpers;
using Jobs.Common.SerializationSettings;
using Jobs.DTO;
using Jobs.DTO.In;
using Microsoft.Extensions.Options;

namespace BlazorApp2.Services.Clients;

public class CompanyClientService(HttpClient client, IOptions<ServicesSettings> settings) : ICompanyClientService
{
    private string? _lastApiKey;
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerSetting.JsonSerializerOptions;
    
    public async Task<CompanyDataInDto> AddCompanyAsync(CompanyDataInDto company)
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.CompanyFirstApiKey, SecureApiKey.CompanySecretKey);
        //using var client = HttClientHeaderHelper.CreateHttpClientWithSecurityHeaders(serviceApiKey, companySecretKey);
        
        var data = JsonSerializer.Serialize(company);
        var content = new StringContent(data, Encoding.UTF8, "application/json");
        
        var companyResponse = await client.PostAsync(settings.Value.CompanyApiServiceUrl, content).ConfigureAwait(false);
        companyResponse.EnsureSuccessStatusCode();
        
        var result = await companyResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Company added - {result}");
        
        var addedCompany = JsonSerializer.Deserialize<CompanyDto>(result, _jsonSerializerOptions);
        
        var record = new CompanyDataInDto(addedCompany.CompanyId, addedCompany.CompanyName,
            addedCompany.CompanyDescription, addedCompany.CompanyLogoPath, addedCompany.CompanyLink,
            addedCompany.IsVisible, addedCompany.IsActive);
        
        return record;
    }
    
    public async Task<bool> UpdateCompanyAsync(CompanyDataInDto company)
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.CompanyFirstApiKey, SecureApiKey.CompanySecretKey);
        
        var data = JsonSerializer.Serialize(company);
        
        using var content = new StringContent(data, Encoding.UTF8, "application/json");
        
        var adminResponse = await client.PutAsync($"{settings.Value.CompanyApiServiceUrl}/{company.CompanyId}", content).ConfigureAwait(false);
        adminResponse.EnsureSuccessStatusCode();

        return true;
    }
    
    public async Task<CompanyDto> GetCompanyByIdAsync(int companyId)
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.CompanyFirstApiKey, SecureApiKey.CompanySecretKey);
        
        var companyResponse = await client.GetAsync($"{settings.Value.CompanyApiServiceUrl}/{companyId}").ConfigureAwait(false);
        companyResponse.EnsureSuccessStatusCode();
        
        var result = await companyResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"GetCompanyResponse - {result}");
        
        return JsonSerializer.Deserialize<CompanyDto>(result, _jsonSerializerOptions)!;
    }
}