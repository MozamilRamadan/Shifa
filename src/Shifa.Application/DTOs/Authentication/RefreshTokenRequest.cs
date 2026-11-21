using System.ComponentModel.DataAnnotations;

namespace Shifa.Application.DTOs.Authentication;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}