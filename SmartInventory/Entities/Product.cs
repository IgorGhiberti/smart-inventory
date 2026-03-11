using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SmartInventory.Entities;

[Index(nameof(Sku), IsUnique = true)]
public class Product : BaseEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    public ICollection<MovementApprovalRequest> ApprovalRequests { get; set; } = new List<MovementApprovalRequest>();
}
