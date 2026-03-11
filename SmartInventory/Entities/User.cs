using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Models;

namespace SmartInventory.Entities;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Operator;

    public bool IsActive { get; set; } = true;

    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    public ICollection<MovementApprovalRequest> RequestedApprovals { get; set; } = new List<MovementApprovalRequest>();
    public ICollection<MovementApprovalRequest> ProcessedApprovals { get; set; } = new List<MovementApprovalRequest>();
}
