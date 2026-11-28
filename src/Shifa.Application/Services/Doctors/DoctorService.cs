using Shifa.Application.DTOs.Doctors;
using Shifa.Application.Interfaces.Doctors;

namespace Shifa.Application.Services.Doctors;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repository;

    public DoctorService(IDoctorRepository repository)
    {
        _repository = repository;
    }

    public Task<DoctorProfileResponse?> GetMyProfileAsync(int userId)
    {
        return _repository.GetByUserIdAsync(userId);
    }

    public async Task<DoctorProfileResponse> CreateProfileAsync(
        int userId,
        CreateDoctorProfileRequest request)
    {
        if (await _repository.GetByUserIdAsync(userId) is not null)
            throw new InvalidOperationException(
                "Doctor profile already exists.");

        if (await _repository.LicenseNumberExistsAsync(request.LicenseNumber))
            throw new InvalidOperationException(
                "License number already exists.");

        var profile = await _repository.CreateAsync(userId, request);

        return profile ?? throw new InvalidOperationException(
            "Failed to create doctor profile.");
    }
}