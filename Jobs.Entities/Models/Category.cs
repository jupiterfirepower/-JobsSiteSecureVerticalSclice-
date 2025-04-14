using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class Category : IEntityBase
{
    public int Id => CategoryId;
    [Key]
    public int CategoryId { get; set; }
    [Required]
    [StringLength(256)]
    public string CategoryName { get; set; }
    
    [ForeignKey("ParentCategory")]
    public int? ParentId { get; set; }

    public Category ParentCategory { get; init; } 
    [Required]
    public bool IsVisible { get; set; } = true;
    [Required]
    public bool IsActive { get; set; } = true;
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; init; } = DateTime.UtcNow;
}