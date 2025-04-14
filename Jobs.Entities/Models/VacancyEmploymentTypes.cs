using Microsoft.EntityFrameworkCore;

namespace Jobs.Entities.Models;

[PrimaryKey(nameof(VacancyId), nameof(EmploymentTypeId))]
public class VacancyEmploymentTypes
{
    public int VacancyId { get; init; }
    public int EmploymentTypeId { get; init; }
}