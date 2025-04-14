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

public class VacancyClientService(HttpClient client, IOptions<ServicesSettings> settings): IVacancyClientService
{
    private string? _lastApiKey;
    private string _serviceBaseUrl = settings.Value.VacancyApiBaseUrl.TrimEnd('/');

    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerSetting.JsonSerializerOptions;
    
    public async Task<List<WorkTypeDto>> GetWorkTypesAsync()
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.VacancyFirstApiKey, SecureApiKey.VacancySecretKey);
        
        var workTypesResponse = await client.GetAsync($"{_serviceBaseUrl}/api/v1/worktypes").ConfigureAwait(false);
        workTypesResponse.EnsureSuccessStatusCode();
        
        var result = await workTypesResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"GetWorkTypesResponse - {result}");

        return JsonSerializer.Deserialize<List<WorkTypeDto>>(result, _jsonSerializerOptions)!;
    }
    
    public async Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync()
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.VacancyFirstApiKey, SecureApiKey.VacancySecretKey);
        
        var employmenTypesResponse = await client.GetAsync($"{_serviceBaseUrl}/api/v1/employmenttypes").ConfigureAwait(false);
        employmenTypesResponse.EnsureSuccessStatusCode();
        
        var result = await employmenTypesResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"GetEmploymentTypesResponse - {result}");
        
        return JsonSerializer.Deserialize<List<EmploymentTypeDto>>(result, _jsonSerializerOptions)!;
    }
    
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.VacancyFirstApiKey, SecureApiKey.VacancySecretKey);
        
        var categoriesResponse = await client.GetAsync($"{_serviceBaseUrl}/api/v1/categories").ConfigureAwait(false);
        categoriesResponse.EnsureSuccessStatusCode();
        
        var result = await categoriesResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"GetCategoriesResponse - {result}");
        
        return JsonSerializer.Deserialize<List<CategoryDto>>(result, _jsonSerializerOptions)!;
    }
    
    public async Task<bool> AddVacancyAsync(VacancyInDto vacancy) 
    {
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.VacancyFirstApiKey, SecureApiKey.VacancySecretKey);
        
        var data = JsonSerializer.Serialize(vacancy);
        var content = new StringContent(data, Encoding.UTF8, "application/json");
        
        var companyResponse = await client.PostAsync($"{_serviceBaseUrl}/api/v1/vacancies", content).ConfigureAwait(false);
        companyResponse.EnsureSuccessStatusCode();
        
        var result = await companyResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Vacancy Adapted added. - {result}");
        
        /*var addedCompany = JsonSerializer.Deserialize<VacancyDto>(result, _jsonSerializerOptions);
        var record = new CompanyDataInDto(addedCompany.CompanyId, addedCompany.CompanyName,
            addedCompany.CompanyDescription, addedCompany.CompanyLogoPath, addedCompany.CompanyLink,
            addedCompany.IsVisible, addedCompany.IsActive);*/

        return true;
    }
}