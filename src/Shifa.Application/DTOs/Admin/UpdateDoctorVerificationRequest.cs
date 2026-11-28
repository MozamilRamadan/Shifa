using Shifa.Domain.Enums;

namespace Shifa.Application.DTOs.Admin;

public class UpdateDoctorVerificationRequest
{
    public DoctorApplicationStatus ApplicationStatus { get; set; }
}