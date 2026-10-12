namespace ApprovalService.Models;

public class ApprovalRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public int RequesterUserId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
    public string? RequesterEmail { get; set; }

    public int? DecidedByUserId { get; set; }
    public string? DecisionComment { get; set; }
    public DateTime? DecidedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
