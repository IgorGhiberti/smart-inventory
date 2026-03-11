using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Entities;

public class InventoryMovement : BaseEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantityMoved { get; set; }

    public int LastInventoryQuantity { get; set; }
    public int CurrentInventoryQuantity { get; set; }

    public bool IsManual { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTimeOffset MovementDate { get; set; }

    public bool IsBelowMinimumAfterMovement { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int MovementTypeId { get; set; }
    public MovementType MovementType { get; set; } = null!;
}
