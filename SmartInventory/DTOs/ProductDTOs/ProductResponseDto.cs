namespace SmartInventory.DTOs.ProductDTOs;

public sealed class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsBelowMinimum => CurrentQuantity < MinimumQuantity;
}
