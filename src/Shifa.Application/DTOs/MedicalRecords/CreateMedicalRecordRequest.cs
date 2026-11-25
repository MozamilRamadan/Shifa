namespace Shifa.Application.DTOs.MedicalRecords;

public class CreateMedicalRecordRequest
{
    public int PatientId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Treatment { get; set; }

    public string? Prescription { get; set; }
}