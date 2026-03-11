using System.Security.Claims;

namespace SmartInventory.Models;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    public static UserRole? GetUserRole(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(role, true, out var parsedRole) ? parsedRole : null;
    }
}
