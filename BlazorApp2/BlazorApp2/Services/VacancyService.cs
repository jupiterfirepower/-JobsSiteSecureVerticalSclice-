using BlazorApp2.Contracts;
using BlazorApp2.Contracts.Clients;
using Jobs.DTO;
using Jobs.DTO.In;

namespace BlazorApp2.Services;

public class VacancyService(IVacancyClientService client) : IVacancyService
{
    public async Task<List<WorkTypeDto>> GetWorkTypesAsync() => await client.GetWorkTypesAsync();
    
    public async Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync() => await client.GetEmploymentTypesAsync();
    
    public async Task<List<CategoryDto>> GetCategoriesAsync() => await client.GetCategoriesAsync();
    
    public async Task<bool> AddVacancyAsync(VacancyInDto vacancy) => await client.AddVacancyAsync(vacancy);
    
}