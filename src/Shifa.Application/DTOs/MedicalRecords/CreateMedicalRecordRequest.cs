using System.ComponentModel.DataAnnotations;

namespace Shifa.Application.DTOs.MedicalRecords;

public class CreateMedicalRecordRequest
{
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(5000)]
    public string? Treatment { get; set; }

    [MaxLength(5000)]
    public string? Prescription { get; set; }
}