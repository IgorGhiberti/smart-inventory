namespace SmartInventory.DTOs.MovementTypeDTOs;

public sealed class MovementTypeResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StockEffect { get; set; }
}
