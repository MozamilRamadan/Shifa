
using System.ComponentModel.DataAnnotations;

namespace Shifa.Application.DTOs.Doctors;

public class CreateDoctorProfileRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string LicenseNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Qualification { get; set; }

    [Range(0, 70)]
    public int? YearsOfExperience { get; set; }

    [StringLength(500)]
    public string? ClinicAddress { get; set; }
}
