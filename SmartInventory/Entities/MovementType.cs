using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SmartInventory.Entities;

[Index(nameof(Code), IsUnique = true)]
public class MovementType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [Range(-1, 1)]
    public int StockEffect { get; set; }

    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    public ICollection<MovementApprovalRequest> ApprovalRequests { get; set; } = new List<MovementApprovalRequest>();
}
