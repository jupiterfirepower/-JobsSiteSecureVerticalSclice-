using Microsoft.EntityFrameworkCore;

namespace Jobs.Entities.Models;

[PrimaryKey(nameof(VacancyId), nameof(WorkTypeId))]
public class VacancyWorkTypes
{
    public int VacancyId { get; init; }
    public int WorkTypeId { get; init; }
}