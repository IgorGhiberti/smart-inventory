using SmartInventory.DTOs.ApprovalDTOs;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class ApprovalEndpoints
{
    public static IEndpointRouteBuilder MapApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/approvals").RequireAuthorization("AdminOnly");

        group.MapGet("/pending", async (IApprovalService service) =>
        {
            var requests = await service.ListPendingAsync();
            return Results.Ok(requests);
        });

        group.MapPost("/{id:int}/approve", async (int id, ApproveApprovalRequestDto dto, HttpContext httpContext, IApprovalService service) =>
        {
            var adminId = httpContext.User.GetUserId();
            if (!adminId.HasValue)
            {
                return Results.Unauthorized();
            }

            var result = await service.ApproveAsync(id, adminId.Value, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        });

        group.MapPost("/{id:int}/reject", async (int id, RejectApprovalRequestDto dto, HttpContext httpContext, IApprovalService service) =>
        {
            var adminId = httpContext.User.GetUserId();
            if (!adminId.HasValue)
            {
                return Results.Unauthorized();
            }

            var result = await service.RejectAsync(id, adminId.Value, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        });

        return app;
    }
}
