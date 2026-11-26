namespace Shifa.Application.DTOs.Permissions;

public class PermissionRequestResponse
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}