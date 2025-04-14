using System.ComponentModel.DataAnnotations;
using Jobs.Entities.Contracts;

namespace Jobs.Entities.Models;

public class SecretApiKey : IEntityBase
{
    public int Id => KeyId;
    
    [Key]
    public int KeyId { get; set; }
    
    [Required]
    [StringLength(64)]
    public string Key { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Modified { get; init; } = DateTime.UtcNow;
}