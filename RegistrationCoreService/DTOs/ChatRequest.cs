using System.ComponentModel.DataAnnotations;

namespace RegistrationCoreService.DTOs;

public class ChatRequest
{
    [Required]
    public string Prompt { get; set; } = string.Empty;
}