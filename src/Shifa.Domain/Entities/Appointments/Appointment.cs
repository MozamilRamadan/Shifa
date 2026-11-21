
using Shifa.Domain.Entities.Common;
using Shifa.Domain.Entities.Patients;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Enums;

namespace Shifa.Domain.Entities.Appointments;

public class Appointment : AuditableEntity
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; }
        = AppointmentStatus.Pending;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;
}
