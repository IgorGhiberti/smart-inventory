using SmartInventory.DTOs.ProductDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IProductService
{
    Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto);
    Task<List<ProductResponseDto>> ListAsync();
    Task<List<ProductResponseDto>> ListLowStockAsync();
    Task<ServiceResult<ProductResponseDto>> UpdateAsync(int id, UpdateProductDto dto);
}
