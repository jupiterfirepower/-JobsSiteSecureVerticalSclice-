using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class Vacancy : IEntityBase
{
    public int Id => VacancyId;
    [Key]
    public int VacancyId { get; init; }

    [ForeignKey("Company")]
    public int CompanyId { get; set; }
    public Company Company{ get; init; }
    
    [ForeignKey("Category")]
    public int CategoryId{ get; init; }
    public Category Category{ get; init; }
    
    [Required]
    [StringLength(256)]
    public string VacancyTitle { get; set; }
    [Required]
    [StringLength(10000)]
    public string VacancyDescription { get; set; }
    
    public double? SalaryFrom { get; set; }
    public double? SalaryTo { get; set; }
    
    [Required]
    public bool IsVisible { get; set; } = true;
    [Required]
    public bool IsActive { get; set; } = true;
   
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime? Modified { get; set; } = DateTime.UtcNow;
}