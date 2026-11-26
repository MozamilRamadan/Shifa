using Shifa.Application.DTOs.Patients;
using Shifa.Application.Interfaces.Patients;

namespace Shifa.Application.Services.Patients;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public Task<PatientProfileResponse?> GetByIdAsync(
        int patientId)
    {
        return _repository.GetByIdAsync(patientId);
    }

    public Task<PatientProfileResponse?> GetMyProfileAsync(
        int userId)
    {
        return _repository.GetByUserIdAsync(userId);
    }
}