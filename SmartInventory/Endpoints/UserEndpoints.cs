using SmartInventory.DTOs.UserDTOs;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").RequireAuthorization("AdminOnly");

        group.MapPost("/", async (CreateUserDto dto, IUserService service) =>
        {
            var result = await service.CreateAsync(dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Created($"/users/{result.Data.Id}", result.Data);
        });

        group.MapGet("/", async (IUserService service) =>
        {
            var users = await service.ListAsync();
            return Results.Ok(users);
        });

        group.MapPatch("/{id:int}/role", async (int id, UpdateUserRoleDto dto, IUserService service) =>
        {
            var result = await service.UpdateRoleAsync(id, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        });

        group.MapPatch("/{id:int}/active", async (int id, IUserService service) =>
        {
            var result = await service.SetActiveAsync(id);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        });

        group.MapDelete("/{id:int}/disable", async (int id, IUserService service) =>
        {
            var result = await service.DeleteAsync(id);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        });

        return app;
    }
}
