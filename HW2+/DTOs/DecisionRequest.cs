using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class DecisionRequest
{
    [StringLength(500)]
    public string? Comment { get; set; }
}
