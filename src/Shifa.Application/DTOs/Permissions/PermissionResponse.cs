namespace Shifa.Application.DTOs.Permissions;

public class PermissionResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? RespondedAt { get; set; }
}