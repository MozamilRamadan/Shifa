using Shifa.Application.DTOs.Patients;

namespace Shifa.Application.Interfaces.Patients;

public interface IPatientRepository
{
    Task<PatientProfileResponse?> GetByIdAsync(int patientId);

    Task<PatientProfileResponse?> GetByUserIdAsync(int userId);
}