using System.ComponentModel.DataAnnotations;

namespace RegistrationCoreService.DTOs;

public class DecisionRequest
{
    [StringLength(500)]
    public string? Comment { get; set; }
}
