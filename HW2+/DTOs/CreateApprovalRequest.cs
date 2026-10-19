using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class CreateApprovalRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}
