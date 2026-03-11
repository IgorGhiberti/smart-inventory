using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.DTOs.ProductDTOs;
using SmartInventory.Entities;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto)
    {
        (bool flowControl, ServiceResult<ProductResponseDto> value) = ValidateProduct(dto);

        if (!flowControl)
        {
            return value;
        }

        var normalizedSku = dto.Sku.Trim().ToUpperInvariant();

        var exists = await _dbContext.Products.AnyAsync(x => x.Sku == normalizedSku);

        if (exists)
        {
            return ServiceResult<ProductResponseDto>.Fail("Produto com o mesmo SKU ja existe.", StatusCodes.Status409Conflict);
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Sku = normalizedSku,
            MinimumQuantity = dto.MinimumQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            IsActive = true
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<ProductResponseDto>.Ok(ToResponse(product), StatusCodes.Status201Created);
    }

    private static (bool flowControl, ServiceResult<ProductResponseDto> value) ValidateProduct(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Sku))
        {
            return (flowControl: false, value: ServiceResult<ProductResponseDto>.Fail("Nome e SKU sao obrigatorios.", StatusCodes.Status400BadRequest));
        }

        if (dto.MinimumQuantity < 0 || dto.CurrentQuantity < 0)
        {
            return (flowControl: false, value: ServiceResult<ProductResponseDto>.Fail("Quantidades nao podem ser negativas.", StatusCodes.Status400BadRequest));
        }

        return (flowControl: true, value: null);
    }

    public async Task<List<ProductResponseDto>> ListAsync()
    {
        return await _dbContext.Products
            .OrderBy(x => x.Name)
            .Select(x => new ProductResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                MinimumQuantity = x.MinimumQuantity,
                CurrentQuantity = x.CurrentQuantity,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductResponseDto>> ListLowStockAsync()
    {
        return await _dbContext.Products
            .Where(x => x.IsActive && x.CurrentQuantity < x.MinimumQuantity)
            .OrderBy(x => x.Name)
            .Select(x => new ProductResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                MinimumQuantity = x.MinimumQuantity,
                CurrentQuantity = x.CurrentQuantity,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<ProductResponseDto>> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return ServiceResult<ProductResponseDto>.Fail("Produto nao encontrado.", StatusCodes.Status404NotFound);
        }

        product.Name = dto.Name.Trim();
        product.MinimumQuantity = dto.MinimumQuantity;
        product.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<ProductResponseDto>.Ok(ToResponse(product));
    }

    private static ProductResponseDto ToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            MinimumQuantity = product.MinimumQuantity,
            CurrentQuantity = product.CurrentQuantity,
            IsActive = product.IsActive
        };
    }
}

