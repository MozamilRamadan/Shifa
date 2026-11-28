
namespace Shifa.Application.DTOs.MedicalRecords;

public class PagedMedicalRecordsResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public IReadOnlyList<MedicalRecordListItemResponse> Data { get; set; }
        = [];
}

public class MedicalRecordListItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public DateTime RecordDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    public IReadOnlyList<MedicalRecordFileResponse> Files { get; set; }
        = [];
}

public class MedicalRecordDetailsResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public DateTime RecordDate { get; set; }

    public IReadOnlyList<MedicalRecordFileResponse> Files { get; set; }
        = [];
}

public class MedicalRecordFileResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    //public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}
