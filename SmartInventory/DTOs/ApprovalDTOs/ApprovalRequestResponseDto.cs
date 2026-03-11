using SmartInventory.Models;

namespace SmartInventory.DTOs.ApprovalDTOs;

public sealed class ApprovalRequestResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public int MovementTypeId { get; set; }
    public string MovementTypeCode { get; set; } = string.Empty;
    public int QuantityMoved { get; set; }
    public bool IsManual { get; set; }
    public string? Reason { get; set; }
    public int LastInventoryQuantity { get; set; }
    public int ProposedInventoryQuantity { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
