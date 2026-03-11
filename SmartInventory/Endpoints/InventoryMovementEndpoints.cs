using SmartInventory.DTOs.InventoryMovementDTOs;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class InventoryMovementEndpoints
{
    public static IEndpointRouteBuilder MapInventoryMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movements").RequireAuthorization();

        group.MapPost("/", async (CreateInventoryMovementDto dto, HttpContext httpContext, IInventoryMovementService service) =>
        {
            var userId = httpContext.User.GetUserId();
            var role = httpContext.User.GetUserRole();

            if (!userId.HasValue || !role.HasValue)
            {
                return Results.Unauthorized();
            }

            var result = await service.CreateAsync(userId.Value, role.Value, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Json(result.Data, statusCode: result.StatusCode);
        });

        group.MapGet("/", async (
            int? productId,
            int? userId,
            int? movementTypeId,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            IInventoryMovementService service) =>
        {
            var movements = await service.ListAsync(productId, userId, movementTypeId, startDate, endDate);
            return Results.Ok(movements);
        });

        return app;
    }
}
