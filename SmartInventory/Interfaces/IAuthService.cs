using SmartInventory.DTOs.AuthDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthUserDto>> LoginAsync(string email, string password);
}
