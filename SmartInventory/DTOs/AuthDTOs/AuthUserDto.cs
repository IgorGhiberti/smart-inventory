using SmartInventory.Models;

namespace SmartInventory.DTOs.AuthDTOs;

public sealed class AuthUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
