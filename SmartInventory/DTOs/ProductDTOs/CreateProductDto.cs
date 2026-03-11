namespace SmartInventory.DTOs.ProductDTOs;

public sealed class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public int CurrentQuantity { get; set; }
}
