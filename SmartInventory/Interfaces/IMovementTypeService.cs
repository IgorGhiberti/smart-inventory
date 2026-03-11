using SmartInventory.DTOs.MovementTypeDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IMovementTypeService
{
    Task<List<MovementTypeResponseDto>> ListAsync();
    Task<ServiceResult<MovementTypeResponseDto>> CreateAsync(CreateMovementTypeDto dto);
    Task<ServiceResult<MovementTypeResponseDto>> UpdateAsync(int id, UpdateMovementTypeDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
