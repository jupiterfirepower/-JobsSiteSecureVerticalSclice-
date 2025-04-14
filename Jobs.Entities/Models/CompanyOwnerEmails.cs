using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class CompanyOwnerEmails: IEntityBase
{
    public int Id => CompanyOwnerEmailsId;
    [Key]
    public int CompanyOwnerEmailsId { get; set; }
    [Required]
    [ForeignKey("CurrentCompany")]
    public int CompanyId { get; set; }
    
    public Company CurrentCompany { get; init; } 
    
    [Required]
    [StringLength(100)]
    public string UserEmail { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;

    [Required] 
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; set; } = DateTime.UtcNow;
}