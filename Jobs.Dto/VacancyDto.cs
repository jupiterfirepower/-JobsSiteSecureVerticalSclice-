namespace Jobs.DTO;

public record VacancyDto(int VacancyId, 
    int CompanyId, 
    int CategoryId,
    string VacancyTitle, 
    string VacancyDescription,
    double? SalaryFrom = null, 
    double? SalaryTo = null,
    bool IsVisible = true, 
    bool IsActive = true, 
    DateTime Created = default,
    DateTime? Modified = default
);