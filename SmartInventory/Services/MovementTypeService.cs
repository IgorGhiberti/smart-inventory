using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.DTOs.MovementTypeDTOs;
using SmartInventory.Entities;
using SmartInventory.Interfaces;
using SmartInventory.Models;

namespace SmartInventory.Services;

public class MovementTypeService : IMovementTypeService
{
    private readonly AppDbContext _dbContext;

    public MovementTypeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MovementTypeResponseDto>> ListAsync()
    {
        return await _dbContext.MovementTypes
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new MovementTypeResponseDto
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                StockEffect = x.StockEffect
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<MovementTypeResponseDto>> CreateAsync(CreateMovementTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("Code e Description sao obrigatorios.", StatusCodes.Status400BadRequest);
        }

        if (dto.StockEffect is < -1 or > 1 || dto.StockEffect == 0)
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("StockEffect deve ser 1 (entrada) ou -1 (saida).", StatusCodes.Status400BadRequest);
        }

        var code = dto.Code.Trim().ToLowerInvariant();
        var exists = await _dbContext.MovementTypes.AnyAsync(x => x.Code == code);
        if (exists)
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("Code ja existe.", StatusCodes.Status409Conflict);
        }

        var entity = new MovementType
        {
            Code = code,
            Description = dto.Description.Trim(),
            StockEffect = dto.StockEffect
        };

        _dbContext.MovementTypes.Add(entity);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<MovementTypeResponseDto>.Ok(ToDto(entity), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<MovementTypeResponseDto>> UpdateAsync(int id, UpdateMovementTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("Code e Description sao obrigatorios.", StatusCodes.Status400BadRequest);
        }

        if (dto.StockEffect is < -1 or > 1 || dto.StockEffect == 0)
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("StockEffect deve ser 1 (entrada) ou -1 (saida).", StatusCodes.Status400BadRequest);
        }

        var entity = await _dbContext.MovementTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("Tipo de movimentacao nao encontrado.", StatusCodes.Status404NotFound);
        }

        var code = dto.Code.Trim().ToLowerInvariant();
        var exists = await _dbContext.MovementTypes.AnyAsync(x => x.Code == code && x.Id != id);
        if (exists)
        {
            return ServiceResult<MovementTypeResponseDto>.Fail("Code ja existe.", StatusCodes.Status409Conflict);
        }

        entity.Code = code;
        entity.Description = dto.Description.Trim();
        entity.StockEffect = dto.StockEffect;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<MovementTypeResponseDto>.Ok(ToDto(entity));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _dbContext.MovementTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return ServiceResult<bool>.Fail("Tipo de movimentacao nao encontrado.", StatusCodes.Status404NotFound);
        }

        var hasMovements = await _dbContext.InventoryMovements.AnyAsync(x => x.MovementTypeId == id)
            || await _dbContext.MovementApprovalRequests.AnyAsync(x => x.MovementTypeId == id);

        if (hasMovements)
        {
            return ServiceResult<bool>.Fail("Nao e possivel excluir tipo de movimentacao ja utilizado.", StatusCodes.Status409Conflict);
        }

        _dbContext.MovementTypes.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private static MovementTypeResponseDto ToDto(MovementType entity)
    {
        return new MovementTypeResponseDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            StockEffect = entity.StockEffect
        };
    }
}
