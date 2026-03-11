using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.DTOs.ApprovalDTOs;
using SmartInventory.DTOs.InventoryMovementDTOs;
using SmartInventory.Entities;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Services;

public class ApprovalService : IApprovalService
{
    private readonly AppDbContext _dbContext;

    public ApprovalService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ApprovalRequestResponseDto>> ListPendingAsync()
    {
        var now = DateTimeOffset.UtcNow;
        await ExpireOldRequestsAsync(now);
        return await (from request in _dbContext.MovementApprovalRequests.AsNoTracking()
                      join product in _dbContext.Products.AsNoTracking() on request.ProductId equals product.Id
                      join user in _dbContext.Users.AsNoTracking() on request.RequestedByUserId equals user.Id
                      join movementType in _dbContext.MovementTypes.AsNoTracking() on request.MovementTypeId equals movementType.Id
                      where request.Status == ApprovalStatus.Pending
                      orderby request.ExpiresAt
                      select new ApprovalRequestResponseDto
                      {
                          Id = request.Id,
                          ProductId = request.ProductId,
                          ProductName = product.Name,
                          RequestedByUserId = request.RequestedByUserId,
                          RequestedByUserName = user.Name,
                          MovementTypeId = request.MovementTypeId,
                          MovementTypeCode = movementType.Code,
                          QuantityMoved = request.QuantityMoved,
                          IsManual = request.IsManual,
                          Reason = request.Reason,
                          LastInventoryQuantity = request.LastInventoryQuantity,
                          ProposedInventoryQuantity = request.ProposedInventoryQuantity,
                          Status = request.Status,
                          ExpiresAt = request.ExpiresAt
                      }).ToListAsync();
    }

    public async Task<ServiceResult<MovementActionResultDto>> ApproveAsync(int approvalId, int adminUserId, ApproveApprovalRequestDto dto)
    {
        var request = await _dbContext.MovementApprovalRequests
            .Include(x => x.Product)
            .Include(x => x.MovementType)
            .FirstOrDefaultAsync(x => x.Id == approvalId);
       
        if (request is null)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Solicitacao nao encontrada.", StatusCodes.Status404NotFound);
        }

        if (request.Status != ApprovalStatus.Pending)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Solicitacao nao esta pendente.", StatusCodes.Status409Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        if (request.ExpiresAt <= now)
        {
            request.Status = ApprovalStatus.Expired;
            await _dbContext.SaveChangesAsync();
            return ServiceResult<MovementActionResultDto>.Fail("Solicitacao expirada.", StatusCodes.Status409Conflict);
        }

        if (!dto.AdminBelowMinimumAcknowledged)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Admin deve confirmar ciencia para aprovar.", StatusCodes.Status400BadRequest);
        }

        if (request.MovementType.StockEffect is < -1 or > 1 || request.MovementType.StockEffect == 0)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Tipo de movimentacao com efeito de estoque invalido.", StatusCodes.Status400BadRequest);
        }

        var signedQuantity = request.MovementType.StockEffect * request.QuantityMoved;

        var lastQuantity = request.Product.CurrentQuantity;
        var currentQuantity = lastQuantity + signedQuantity;

        if (currentQuantity < 0)
        {
            return ServiceResult<MovementActionResultDto>.Fail("Aprovacao invalida: estoque resultante menor que zero.", StatusCodes.Status400BadRequest);
        }

        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        request.Product.CurrentQuantity = currentQuantity;
        request.Status = ApprovalStatus.Approved;
        request.ApprovedByUserId = adminUserId;
        request.ApprovedAt = now;

        var movement = new InventoryMovement
        {
            ProductId = request.ProductId,
            UserId = request.RequestedByUserId,
            MovementTypeId = request.MovementTypeId,
            QuantityMoved = request.QuantityMoved,
            LastInventoryQuantity = lastQuantity,
            CurrentInventoryQuantity = currentQuantity,
            IsManual = request.IsManual,
            Reason = request.Reason,
            MovementDate = now,
            IsBelowMinimumAfterMovement = currentQuantity < request.Product.MinimumQuantity
        };

        _dbContext.InventoryMovements.Add(movement);
        await _dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return ServiceResult<MovementActionResultDto>.Ok(new MovementActionResultDto
        {
            Applied = true,
            MovementId = movement.Id,
            ApprovalRequestId = request.Id,
            Message = "Solicitacao aprovada e movimentacao aplicada."
        });
    }

    public async Task<ServiceResult<ApprovalRequestResponseDto>> RejectAsync(int approvalId, int adminUserId, RejectApprovalRequestDto dto)
    {
        var request = await _dbContext.MovementApprovalRequests
            .Include(x => x.Product)
            .Include(x => x.RequestedByUser)
            .Include(x => x.MovementType)
            .FirstOrDefaultAsync(x => x.Id == approvalId);

        if (request is null)
        {
            return ServiceResult<ApprovalRequestResponseDto>.Fail("Solicitacao nao encontrada.", StatusCodes.Status404NotFound);
        }

        if (request.Status != ApprovalStatus.Pending)
        {
            return ServiceResult<ApprovalRequestResponseDto>.Fail("Solicitacao nao esta pendente.", StatusCodes.Status409Conflict);
        }

        if (request.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            request.Status = ApprovalStatus.Expired;
            await _dbContext.SaveChangesAsync();
            return ServiceResult<ApprovalRequestResponseDto>.Fail("Solicitacao expirada.", StatusCodes.Status409Conflict);
        }

        request.Status = ApprovalStatus.Rejected;
        request.ApprovedByUserId = adminUserId;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        request.RejectionReason = string.IsNullOrWhiteSpace(dto.RejectionReason) ? null : dto.RejectionReason.Trim();

        await _dbContext.SaveChangesAsync();

        return ServiceResult<ApprovalRequestResponseDto>.Ok(new ApprovalRequestResponseDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            ProductName = request.Product.Name,
            RequestedByUserId = request.RequestedByUserId,
            RequestedByUserName = request.RequestedByUser.Name,
            MovementTypeId = request.MovementTypeId,
            MovementTypeCode = request.MovementType.Code,
            QuantityMoved = request.QuantityMoved,
            IsManual = request.IsManual,
            Reason = request.Reason,
            LastInventoryQuantity = request.LastInventoryQuantity,
            ProposedInventoryQuantity = request.ProposedInventoryQuantity,
            Status = request.Status,
            ExpiresAt = request.ExpiresAt
        });
    }

    private async Task ExpireOldRequestsAsync(DateTimeOffset now)
    {
        var expired = await _dbContext.MovementApprovalRequests
            .Where(x => x.Status == ApprovalStatus.Pending && x.ExpiresAt <= now)
            .ToListAsync();

        if (expired.Any())
        {
            return;
        }

        foreach (var item in expired)
        {
            item.Status = ApprovalStatus.Expired;
        }

        await _dbContext.SaveChangesAsync();
    }
}
