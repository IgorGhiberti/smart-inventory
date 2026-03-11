using System.ComponentModel.DataAnnotations;
using SmartInventory.Models;

namespace SmartInventory.Entities;

public class MovementApprovalRequest : BaseEntity
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = null!;

    public int MovementTypeId { get; set; }
    public MovementType MovementType { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int QuantityMoved { get; set; }

    public bool IsManual { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public int LastInventoryQuantity { get; set; }
    public int ProposedInventoryQuantity { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTimeOffset ExpiresAt { get; set; }

    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
