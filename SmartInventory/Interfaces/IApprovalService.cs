using SmartInventory.DTOs.ApprovalDTOs;
using SmartInventory.DTOs.InventoryMovementDTOs;
using SmartInventory.Models;

namespace SmartInventory.Interfaces;

public interface IApprovalService
{
    Task<List<ApprovalRequestResponseDto>> ListPendingAsync();
    Task<ServiceResult<MovementActionResultDto>> ApproveAsync(int approvalId, int adminUserId, ApproveApprovalRequestDto dto);
    Task<ServiceResult<ApprovalRequestResponseDto>> RejectAsync(int approvalId, int adminUserId, RejectApprovalRequestDto dto);
}
