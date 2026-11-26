using Shifa.Application.DTOs.Patients;

namespace Shifa.Application.Interfaces.Patients;

public interface IPatientService
{
    Task<PatientProfileResponse?> GetByIdAsync(int patientId);

    Task<PatientProfileResponse?> GetMyProfileAsync(int userId);
}