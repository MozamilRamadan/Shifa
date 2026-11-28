using Shifa.Application.DTOs.Doctors;

namespace Shifa.Application.Interfaces.Doctors;

public interface IDoctorService
{
    Task<DoctorProfileResponse?> GetMyProfileAsync(int userId);

    Task<DoctorProfileResponse> CreateProfileAsync(
        int userId,
        CreateDoctorProfileRequest request);
}