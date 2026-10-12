using ApprovalService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApprovalService.Repositories;

public class ApprovalRequestRepository(AppDbContext context, ILogger<ApprovalRequestRepository> logger)
    : IApprovalRequestRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ApprovalRequestRepository> _logger = logger;

    public async Task<IReadOnlyCollection<ApprovalRequest>> GetAllAsync(
        ApprovalStatus? status = null,
        int? requesterUserId = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null)
    {
        IQueryable<ApprovalRequest> query = _context.ApprovalRequests;

        if (status is not null)
        {
            query = query.Where(r => r.Status == status);
        }

        if (requesterUserId is not null)
        {
            query = query.Where(r => r.RequesterUserId == requesterUserId);
        }

        if (createdFrom is not null)
        {
            query = query.Where(r => r.CreatedAt >= createdFrom);
        }

        if (createdTo is not null)
        {
            query = query.Where(r => r.CreatedAt <= createdTo);
        }

        return await query
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<ApprovalRequest?> GetByIdAsync(int id)
    {
        return await _context.ApprovalRequests.FindAsync(id);
    }

    public async Task<ApprovalRequest> AddAsync(ApprovalRequest request)
    {
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Approval request {RequestId} persisted", request.Id);

        return request;
    }

    public async Task<ApprovalRequest> UpdateAsync(ApprovalRequest request)
    {
        _context.ApprovalRequests.Update(request);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Approval request {RequestId} changes persisted", request.Id);

        return request;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var request = await _context.ApprovalRequests.FindAsync(id);
        if (request is null)
        {
            _logger.LogDebug("Nothing to delete, approval request {RequestId} does not exist", id);
            return false;
        }

        _context.ApprovalRequests.Remove(request);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Approval request {RequestId} removed from database", id);

        return true;
    }
}
