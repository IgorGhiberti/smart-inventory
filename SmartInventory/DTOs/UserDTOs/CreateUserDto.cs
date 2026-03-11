using SmartInventory.Models;

namespace SmartInventory.DTOs.UserDTOs;

public sealed class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
}
