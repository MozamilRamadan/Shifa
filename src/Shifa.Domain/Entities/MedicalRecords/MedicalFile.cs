
using Shifa.Domain.Entities.Common;

namespace Shifa.Domain.Entities.MedicalRecords;

public class MedicalFile : BaseEntity
{
    public int MedicalRecordId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string? StorageIdentifier { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord MedicalRecord { get; set; } = null!;
}
