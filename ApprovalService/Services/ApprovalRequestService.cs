using ApprovalService.DTOs;
using ApprovalService.Exceptions;
using ApprovalService.Models;
using ApprovalService.Repositories;

namespace ApprovalService.Services;

public class ApprovalRequestService(
    IApprovalRequestRepository repository,
    IUserRepositoryClient userDirectoryClient,
    ILogger<ApprovalRequestService> logger) : IApprovalRequestService
{
    private readonly IApprovalRequestRepository _repository = repository;
    private readonly IUserRepositoryClient _userDirectoryClient = userDirectoryClient;
    private readonly ILogger<ApprovalRequestService> _logger = logger;

    public async Task<ApprovalRequestResponse> CreateAsync(
        int requesterUserId,
        string accessToken,
        CreateApprovalRequest request)
    {
        _logger.LogInformation(
            "Creating approval request for user {UserId} with title {Title}",
            requesterUserId,
            request.Title);

        var requester = await _userDirectoryClient.GetUserAsync(requesterUserId, accessToken);
        if (requester is null)
        {
            _logger.LogWarning(
                "Approval request rejected: user {UserId} does not exist in the users service",
                requesterUserId);
            throw new KeyNotFoundException("Requester was not found in the users service.");
        }

        var entity = new ApprovalRequest
        {
            Title = request.Title,
            Description = request.Description,
            Status = ApprovalStatus.Pending,
            RequesterUserId = requester.Id,
            RequesterUsername = requester.Username,
            RequesterEmail = requester.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);

        _logger.LogInformation(
            "Approval request {RequestId} created by user {UserId}",
            created.Id,
            created.RequesterUserId);

        return ToResponse(created);
    }

    public async Task<IReadOnlyCollection<ApprovalRequestResponse>> GetAsync(FilterApprovalRequestsRequest filter)
    {
        _logger.LogInformation(
            "Fetching approval requests with filter: status {Status}, requester {RequesterUserId}, created from {CreatedFrom}, created to {CreatedTo}",
            filter.Status,
            filter.RequesterUserId,
            filter.CreatedFrom,
            filter.CreatedTo);

        var requests = await _repository.GetAllAsync(
            filter.Status,
            filter.RequesterUserId,
            filter.CreatedFrom,
            filter.CreatedTo);

        _logger.LogInformation("Fetched {RequestCount} approval requests", requests.Count);

        return requests.Select(ToResponse).ToList();
    }

    public async Task<ApprovalRequestResponse> GetByIdAsync(int id)
    {
        var entity = await GetExistingAsync(id);

        _logger.LogInformation("Approval request {RequestId} fetched", id);

        return ToResponse(entity);
    }

    public async Task<ApprovalRequestResponse> UpdateAsync(
        int id,
        int currentUserId,
        UpdateApprovalRequest request)
    {
        var entity = await GetExistingAsync(id);

        EnsureOwnership(entity, currentUserId);

        if (entity.Status != ApprovalStatus.Pending)
        {
            _logger.LogWarning(
                "Update of approval request {RequestId} rejected: request is already {Status}",
                id,
                entity.Status);
            throw new InvalidOperationException("Only pending requests can be updated.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            entity.Title = request.Title;
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description;
        }

        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);

        _logger.LogInformation("Approval request {RequestId} updated by user {UserId}", id, currentUserId);

        return ToResponse(updated);
    }

    public Task<ApprovalRequestResponse> ApproveAsync(int id, int deciderUserId, DecisionRequest request)
    {
        return DecideAsync(id, deciderUserId, ApprovalStatus.Approved, request);
    }

    public Task<ApprovalRequestResponse> RejectAsync(int id, int deciderUserId, DecisionRequest request)
    {
        return DecideAsync(id, deciderUserId, ApprovalStatus.Rejected, request);
    }

    public async Task<bool> DeleteAsync(int id, int currentUserId)
    {
        var entity = await GetExistingAsync(id);

        EnsureOwnership(entity, currentUserId);

        var deleted = await _repository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Approval request {RequestId} deleted by user {UserId}", id, currentUserId);
        }

        return deleted;
    }

    private async Task<ApprovalRequestResponse> DecideAsync(
        int id,
        int deciderUserId,
        ApprovalStatus decision,
        DecisionRequest request)
    {
        var entity = await GetExistingAsync(id);

        if (entity.RequesterUserId == deciderUserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to decide on their own approval request {RequestId}",
                deciderUserId,
                id);
            throw new ForbiddenException("A user cannot decide on their own request.");
        }

        if (entity.Status != ApprovalStatus.Pending)
        {
            _logger.LogWarning(
                "Decision on approval request {RequestId} rejected: request is already {Status}",
                id,
                entity.Status);
            throw new InvalidOperationException("Only pending requests can be decided on.");
        }

        entity.Status = decision;
        entity.DecidedByUserId = deciderUserId;
        entity.DecisionComment = request.Comment;
        entity.DecidedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);

        _logger.LogInformation(
            "Approval request {RequestId} {Status} by user {UserId}",
            id,
            decision,
            deciderUserId);

        return ToResponse(updated);
    }

    private async Task<ApprovalRequest> GetExistingAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Approval request {RequestId} not found", id);
            throw new KeyNotFoundException("Approval request not found.");
        }

        return entity;
    }

    private static void EnsureOwnership(ApprovalRequest entity, int currentUserId)
    {
        if (entity.RequesterUserId != currentUserId)
        {
            throw new ForbiddenException("Only the requester can modify this approval request.");
        }
    }

    private static ApprovalRequestResponse ToResponse(ApprovalRequest entity)
    {
        return new ApprovalRequestResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            RequesterUserId = entity.RequesterUserId,
            RequesterUsername = entity.RequesterUsername,
            RequesterEmail = entity.RequesterEmail,
            DecidedByUserId = entity.DecidedByUserId,
            DecisionComment = entity.DecisionComment,
            DecidedAt = entity.DecidedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
