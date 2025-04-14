using System.ComponentModel.DataAnnotations;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class Company : IEntityBase
{
    public int Id => CompanyId;
    [Key]
    public int CompanyId { get; set; }
    [Required]
    [StringLength(256)]
    public string CompanyName { get; set; }
    
    [Required]
    [StringLength(2000)]
    public string CompanyDescription { get; set; }
    
    [Required]
    [StringLength(256)]
    public string CompanyLogoPath { get; set; }
    
    [StringLength(256)]
    public string CompanyLink { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    [Required]
    public bool IsVisible { get; set; } = true;
    
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; set; } = DateTime.UtcNow;
}