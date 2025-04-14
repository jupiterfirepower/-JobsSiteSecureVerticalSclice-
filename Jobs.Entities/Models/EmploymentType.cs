using System.ComponentModel.DataAnnotations;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class EmploymentType : IEntityBase
{
    public int Id => EmploymentTypeId;
    [Key]
    public int EmploymentTypeId { get; init; }
    [Required]
    [StringLength(100)]
    public string EmploymentTypeName { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; init; } = DateTime.UtcNow;
}