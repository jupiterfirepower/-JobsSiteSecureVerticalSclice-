using System.ComponentModel.DataAnnotations;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class WorkType : IEntityBase
{
    public int Id => WorkTypeId;
    
    [Key]
    public int WorkTypeId { get; init; }
    
    [Required]
    [StringLength(100)]
    public string WorkTypeName { get; init; }
    
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; init; } = DateTime.UtcNow;
}