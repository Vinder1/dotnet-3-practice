using ApprovalService.DTOs;

namespace ApprovalService.Services;

public interface IApprovalRequestService
{
    Task<ApprovalRequestResponse> CreateAsync(
        int requesterUserId,
        string accessToken,
        CreateApprovalRequest request);
    Task<IReadOnlyCollection<ApprovalRequestResponse>> GetAsync(FilterApprovalRequestsRequest filter);
    Task<ApprovalRequestResponse> GetByIdAsync(int id);
    Task<ApprovalRequestResponse> UpdateAsync(int id, int currentUserId, UpdateApprovalRequest request);
    Task<ApprovalRequestResponse> ApproveAsync(int id, int deciderUserId, DecisionRequest request);
    Task<ApprovalRequestResponse> RejectAsync(int id, int deciderUserId, DecisionRequest request);
    Task<bool> DeleteAsync(int id, int currentUserId);
}
