namespace Shifa.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public int DoctorId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
}