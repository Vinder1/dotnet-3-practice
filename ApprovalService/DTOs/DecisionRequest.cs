using System.ComponentModel.DataAnnotations;

namespace ApprovalService.DTOs;

public class DecisionRequest
{
    [StringLength(500)]
    public string? Comment { get; set; }
}
