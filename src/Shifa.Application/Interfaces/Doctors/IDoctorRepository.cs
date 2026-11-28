using Shifa.Application.DTOs.Doctors;

namespace Shifa.Application.Interfaces.Doctors;

public interface IDoctorRepository
{
    Task<DoctorProfileResponse?> GetByUserIdAsync(int userId);

    Task<DoctorProfileResponse?> CreateAsync(
        int userId,
        CreateDoctorProfileRequest request);

    Task<bool> LicenseNumberExistsAsync(string licenseNumber);
}