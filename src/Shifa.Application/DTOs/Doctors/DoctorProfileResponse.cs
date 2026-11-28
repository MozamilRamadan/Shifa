
using Shifa.Domain.Enums;

namespace Shifa.Application.DTOs.Doctors;

public class DoctorProfileResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Specialization { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;

    public string? Qualification { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? ClinicAddress { get; set; }

    public DoctorApplicationStatus ApplicationStatus { get; set; }
}
