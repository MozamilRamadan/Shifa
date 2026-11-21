
using Shifa.Domain.Entities.Common;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Entities.Patients;
using Shifa.Domain.Enums;

namespace Shifa.Domain.Entities.Permissions;

public class PermissionRequest : AuditableEntity
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string? Reason { get; set; }

    public PermissionStatus Status { get; set; }
        = PermissionStatus.Pending;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;
}
