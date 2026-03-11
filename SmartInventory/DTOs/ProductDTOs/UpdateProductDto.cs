namespace SmartInventory.DTOs.ProductDTOs;

public sealed class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public bool IsActive { get; set; }
}
