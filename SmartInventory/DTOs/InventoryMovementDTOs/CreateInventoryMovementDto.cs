namespace SmartInventory.DTOs.InventoryMovementDTOs;

public sealed class CreateInventoryMovementDto
{
    public int ProductId { get; set; }
    public int MovementTypeId { get; set; }
    public int QuantityMoved { get; set; }
    public bool IsManual { get; set; }
    public string? Reason { get; set; }
}
