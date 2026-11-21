using System.ComponentModel.DataAnnotations;

namespace Shifa.Application.DTOs.Authentication;

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}