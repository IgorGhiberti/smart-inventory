using SmartInventory.DTOs.UserDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserDto dto);
    Task<List<UserResponseDto>> ListAsync();
    Task<ServiceResult<UserResponseDto>> UpdateRoleAsync(int id, UpdateUserRoleDto dto);
    Task<ServiceResult<UserResponseDto>> SetActiveAsync(int id);
    Task<ServiceResult<string>> DeleteAsync(int id);
}
