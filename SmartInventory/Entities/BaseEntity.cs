using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Entities;

public abstract class BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
