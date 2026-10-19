using HW2.DTOs;

namespace HW2.Services;

public interface IApprovalServiceClient
{
    Task<ApprovalServiceResult<ApprovalRequestResponse>> CreateAsync(
        string accessToken,
        CreateApprovalRequest request,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult<IReadOnlyCollection<ApprovalRequestResponse>>> GetAsync(
        string accessToken,
        FilterApprovalRequestsRequest filter,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult<ApprovalRequestResponse>> GetByIdAsync(
        string accessToken,
        int id,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult<ApprovalRequestResponse>> UpdateAsync(
        string accessToken,
        int id,
        UpdateApprovalRequest request,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult<ApprovalRequestResponse>> ApproveAsync(
        string accessToken,
        int id,
        DecisionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult<ApprovalRequestResponse>> RejectAsync(
        string accessToken,
        int id,
        DecisionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApprovalServiceResult> DeleteAsync(
        string accessToken,
        int id,
        CancellationToken cancellationToken = default);
}
