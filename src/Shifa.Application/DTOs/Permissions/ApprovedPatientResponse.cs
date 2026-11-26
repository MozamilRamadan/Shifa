namespace Shifa.Application.DTOs.Permissions;

public class ApprovedPatientResponse
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}