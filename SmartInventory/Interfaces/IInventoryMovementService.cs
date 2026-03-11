using SmartInventory.DTOs.InventoryMovementDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IInventoryMovementService
{
    Task<ServiceResult<MovementActionResultDto>> CreateAsync(int userId, UserRole userRole, CreateInventoryMovementDto dto);
    Task<List<InventoryMovementResponseDto>> ListAsync(int? productId, int? userId, int? movementTypeId, DateTimeOffset? startDate, DateTimeOffset? endDate);
}
