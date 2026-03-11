using SmartInventory.DTOs.ProductDTOs;
using SmartInventory.Interfaces;

namespace SmartInventory.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").RequireAuthorization();

        group.MapPost("/", async (CreateProductDto dto, IProductService service) =>
        {
            var result = await service.CreateAsync(dto);

            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Created($"/products/{result.Data.Id}", result.Data);
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IProductService service) =>
        {
            var products = await service.ListAsync();
            return Results.Ok(products);
        });

        group.MapGet("/low-stock", async (IProductService service) =>
        {
            var products = await service.ListLowStockAsync();
            return Results.Ok(products);
        });

        group.MapPatch("/{id:int}", async (int id, UpdateProductDto dto, IProductService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            if (!result.Success || result.Data is null)
            {
                return Results.Json(new { message = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Data);
        }).RequireAuthorization("AdminOnly");

        return app;
    }
}
