namespace SmartInventory.DTOs.InventoryMovementDTOs;

public sealed class MovementActionResultDto
{
    public bool Applied { get; set; }
    public int? MovementId { get; set; }
    public int? ApprovalRequestId { get; set; }
    public string Message { get; set; } = string.Empty;
}
