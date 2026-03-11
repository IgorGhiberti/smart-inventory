using SmartInventory.DTOs.MovementTypeDTOs;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class MovementTypeEndpoints
{
    public static IEndpointRouteBuilder MapMovementTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movement-types").RequireAuthorization();

        group.MapGet("/", async (IMovementTypeService service) =>
        {
            var movementTypes = await service.ListAsync();
            return Results.Ok(movementTypes);
        });

        group.MapPost("/", async (CreateMovementTypeDto dto, IMovementTypeService service) =>
        {
            var result = await service.CreateAsync(dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Created($"/movement-types/{result.Data.Id}", result.Data);
        }).RequireAuthorization("AdminOnly");

        group.MapPatch("/{id:int}", async (int id, UpdateMovementTypeDto dto, IMovementTypeService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        }).RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:int}", async (int id, IMovementTypeService service) =>
        {
            var result = await service.DeleteAsync(id);
            if (!result.Success)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(new { message = "Tipo de movimentacao excluido com sucesso." });
        }).RequireAuthorization("AdminOnly");

        return app;
    }
}
