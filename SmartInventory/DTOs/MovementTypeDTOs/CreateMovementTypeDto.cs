namespace SmartInventory.DTOs.MovementTypeDTOs;

public sealed class CreateMovementTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StockEffect { get; set; }
}
