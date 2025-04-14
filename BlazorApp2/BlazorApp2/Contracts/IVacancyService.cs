using Jobs.DTO;
using Jobs.DTO.In;

namespace BlazorApp2.Contracts;

public interface IVacancyService
{
    Task<List<WorkTypeDto>> GetWorkTypesAsync();
    Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync();
    Task<List<CategoryDto>> GetCategoriesAsync();

    Task<bool> AddVacancyAsync(VacancyInDto vacancy);
}