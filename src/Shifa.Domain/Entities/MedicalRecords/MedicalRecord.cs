
using Shifa.Domain.Entities.Common;
using Shifa.Domain.Entities.Patients;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Enums;

namespace Shifa.Domain.Entities.MedicalRecords;

public class MedicalRecord : AuditableEntity
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Treatment { get; set; }

    public string? Prescription { get; set; }

    public DateTime RecordDate { get; set; } = DateTime.UtcNow;

    public RecordStatus Status { get; set; } = RecordStatus.Published;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public ICollection<MedicalFile> Files { get; set; }
        = new List<MedicalFile>();
}
