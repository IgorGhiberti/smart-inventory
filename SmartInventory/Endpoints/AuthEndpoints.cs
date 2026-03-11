using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SmartInventory.DTOs.AuthDTOs;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/login", async (LoginRequestDto dto, IAuthService authService, HttpContext httpContext) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return Results.BadRequest(new { message = "Email e senha sao obrigatorios." });
            }

            var result = await authService.LoginAsync(dto.Email, dto.Password);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            var user = result.Data;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Ok(new { message = "Login realizado com sucesso." });
        }).AllowAnonymous();

        group.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logout realizado com sucesso." });
        }).RequireAuthorization();

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            var id = user.GetUserId();
            var role = user.GetUserRole();
            var name = user.FindFirstValue(ClaimTypes.Name);
            var email = user.FindFirstValue(ClaimTypes.Email);

            if (!id.HasValue || !role.HasValue)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                id = id.Value,
                name,
                email,
                role = role.Value
            });
        }).RequireAuthorization();

        return app;
    }
}
