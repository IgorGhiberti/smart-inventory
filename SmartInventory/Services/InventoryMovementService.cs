using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.DTOs.InventoryMovementDTOs;
using SmartInventory.Entities;
using SmartInventory.Models;
using SmartInventory.Interfaces;
using System.Linq;

namespace SmartInventory.Services;

public class InventoryMovementService : IInventoryMovementService
{
    private readonly AppDbContext _dbContext;

    public InventoryMovementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<MovementActionResultDto>> CreateAsync(int userId, UserRole userRole, CreateInventoryMovementDto dto)
    {
        if (dto.QuantityMoved <= 0)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Quantidade deve ser maior que zero.", StatusCodes.Status400BadRequest);
        }

        if (dto.IsManual && string.IsNullOrWhiteSpace(dto.Reason))
        {
            return ServiceResult<MovementActionResultDto>.Fail("Motivo e obrigatorio para movimentacao manual.", StatusCodes.Status400BadRequest);
        }

        var movementType = await _dbContext.MovementTypes.FirstOrDefaultAsync(x => x.Id == dto.MovementTypeId);
        if (movementType is null)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Tipo de movimentacao nao encontrado.", StatusCodes.Status404NotFound);
        }

        if (movementType.StockEffect is < -1 or > 1 || movementType.StockEffect == 0)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Tipo de movimentacao com efeito de estoque invalido.", StatusCodes.Status400BadRequest);
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == dto.ProductId);
        if (product is null || !product.IsActive)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Produto nao encontrado ou inativo.", StatusCodes.Status404NotFound);
        }

        var signedQuantity = movementType.StockEffect * dto.QuantityMoved;

        var lastQuantity = product.CurrentQuantity;
        var currentQuantity = lastQuantity + signedQuantity;

        if (currentQuantity < 0)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Estoque nao pode ficar menor que zero.", StatusCodes.Status400BadRequest);
        }

        if (currentQuantity < product.MinimumQuantity)
        {
            if (userRole == UserRole.Admin)
            {
                var movement = await ApplyMovementAsync(userId, dto, product, movementType, lastQuantity, currentQuantity, true);
                return ServiceResult<MovementActionResultDto>.Ok(new MovementActionResultDto
                {
                    Applied = true,
                    MovementId = movement.Id,
                    Message = "Movimentacao aplicada abaixo do minimo com ciencia do admin."
                }, StatusCodes.Status201Created);
            }

            var request = new MovementApprovalRequest
            {
                ProductId = product.Id,
                RequestedByUserId = userId,
                MovementTypeId = movementType.Id,
                QuantityMoved = dto.QuantityMoved,
                IsManual = dto.IsManual,
                Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
                LastInventoryQuantity = lastQuantity,
                ProposedInventoryQuantity = currentQuantity,
                Status = ApprovalStatus.Pending,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };

            _dbContext.MovementApprovalRequests.Add(request);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<MovementActionResultDto>.Ok(new MovementActionResultDto
            {
                Applied = false,
                ApprovalRequestId = request.Id,
                Message = "Movimentacao requer aprovacao de admin."
            }, StatusCodes.Status202Accepted);
        }

        var appliedMovement = await ApplyMovementAsync(userId, dto, product, movementType, lastQuantity, currentQuantity, false);
        return ServiceResult<MovementActionResultDto>.Ok(new MovementActionResultDto
        {
            Applied = true,
            MovementId = appliedMovement.Id,
            Message = "Movimentacao aplicada com sucesso."
        }, StatusCodes.Status201Created);
    }

    public async Task<List<InventoryMovementResponseDto>> ListAsync(int? productId, int? userId, int? movementTypeId, DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var query = from im in _dbContext.InventoryMovements.AsNoTracking()
                    join p in _dbContext.Products on im.ProductId equals p.Id
                    join u in _dbContext.Users on im.UserId equals u.Id
                    join mt in _dbContext.MovementTypes on im.MovementTypeId equals mt.Id
                    select new
                    {
                        Id = im.Id,
                        ProductId = im.ProductId,
                        UserId = im.UserId,
                        MovementTypeId = im.MovementTypeId,
                        MovementDate = im.MovementDate,
                        ProductName = p.Name,
                        UserName = u.Name,
                        MovementTypeCode = mt.Code,
                        QuantityMoved = im.QuantityMoved,
                        LastInventoryQuantity = im.LastInventoryQuantity,
                        CurrentInventoryQuantity = im.CurrentInventoryQuantity,
                        IsManual = im.IsManual,
                        Reason = im.Reason,
                        IsBelowMinimumAfterMovement = im.IsBelowMinimumAfterMovement
                    };


        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (movementTypeId.HasValue)
        {
            query = query.Where(x => x.MovementTypeId == movementTypeId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.MovementDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(x => x.MovementDate)
            .Select(x => new InventoryMovementResponseDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                UserId = x.UserId,
                UserName = x.UserName,
                MovementTypeId = x.MovementTypeId,
                MovementTypeCode = x.MovementTypeCode,
                QuantityMoved = x.QuantityMoved,
                LastInventoryQuantity = x.LastInventoryQuantity,
                CurrentInventoryQuantity = x.CurrentInventoryQuantity,
                IsManual = x.IsManual,
                Reason = x.Reason,
                IsBelowMinimumAfterMovement = x.IsBelowMinimumAfterMovement,
                MovementDate = x.MovementDate
            })
            .ToListAsync();
    }

    private async Task<InventoryMovement> ApplyMovementAsync(
        int userId,
        CreateInventoryMovementDto dto,
        Product product,
        MovementType movementType,
        int lastQuantity,
        int currentQuantity,
        bool adminAcknowledged)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        product.CurrentQuantity = currentQuantity;

        var movement = new InventoryMovement
        {
            ProductId = product.Id,
            UserId = userId,
            MovementTypeId = movementType.Id,
            QuantityMoved = dto.QuantityMoved,
            LastInventoryQuantity = lastQuantity,
            CurrentInventoryQuantity = currentQuantity,
            IsManual = dto.IsManual,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
            MovementDate = DateTimeOffset.UtcNow,
            IsBelowMinimumAfterMovement = currentQuantity < product.MinimumQuantity
        };

        _dbContext.InventoryMovements.Add(movement);
        await _dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return movement;
    }
}
