namespace SmartInventory.DTOs.MovementTypeDTOs;

public sealed class UpdateMovementTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StockEffect { get; set; }
}
