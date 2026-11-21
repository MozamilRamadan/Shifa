
using Shifa.Domain.Entities.Common;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Domain.Entities.Permissions;
using Shifa.Domain.Entities.Appointments;

namespace Shifa.Domain.Entities.Patients;

public class Patient : AuditableEntity
{
    public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodType { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }


    public ICollection<MedicalRecord> MedicalRecords { get; set; }
        = new List<MedicalRecord>();

    public ICollection<PermissionRequest> PermissionRequests { get; set; }
        = new List<PermissionRequest>();

    public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
}
