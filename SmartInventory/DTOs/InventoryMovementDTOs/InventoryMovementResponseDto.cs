namespace SmartInventory.DTOs.InventoryMovementDTOs;

public sealed class InventoryMovementResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int MovementTypeId { get; set; }
    public string MovementTypeCode { get; set; } = string.Empty;
    public int QuantityMoved { get; set; }
    public int LastInventoryQuantity { get; set; }
    public int CurrentInventoryQuantity { get; set; }
    public bool IsManual { get; set; }
    public string? Reason { get; set; }
    public bool IsBelowMinimumAfterMovement { get; set; }
    public DateTimeOffset MovementDate { get; set; }
}
