
using Shifa.Domain.Entities.Common;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Domain.Entities.Permissions;
using Shifa.Domain.Entities.Appointments;

namespace Shifa.Domain.Entities.Doctors;

public class Doctor : AuditableEntity
{
    public int UserId { get; set; }

    public string Specialization { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;

    public string? Qualification { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? ClinicAddress { get; set; }

    public bool IsVerified { get; set; }


    public ICollection<MedicalRecord> MedicalRecords { get; set; }
        = new List<MedicalRecord>();

    public ICollection<PermissionRequest> PermissionRequests { get; set; }
        = new List<PermissionRequest>();

    public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
}
