namespace Shifa.Application.DTOs.Appointments;

public class AppointmentResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
}