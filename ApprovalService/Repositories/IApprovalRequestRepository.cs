using ApprovalService.Models;

namespace ApprovalService.Repositories;

public interface IApprovalRequestRepository
{
    Task<IReadOnlyCollection<ApprovalRequest>> GetAllAsync(
        ApprovalStatus? status = null,
        int? requesterUserId = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null);
    Task<ApprovalRequest?> GetByIdAsync(int id);
    Task<ApprovalRequest> AddAsync(ApprovalRequest request);
    Task<ApprovalRequest> UpdateAsync(ApprovalRequest request);
    Task<bool> DeleteAsync(int id);
}
