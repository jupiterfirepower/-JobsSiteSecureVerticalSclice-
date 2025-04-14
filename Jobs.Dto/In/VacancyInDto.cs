namespace Jobs.DTO.In;

public record VacancyInDto(
    int VacancyId,
    int CompanyId,
    int CategoryId,
    string VacancyTitle,
    string VacancyDescription,
    List<int> WorkTypes = null,
    List<int> EmploymentTypes = null,
    double? SalaryFrom = null,
    double? SalaryTo = null,
    bool IsVisible = true,
    bool IsActive = true
);
