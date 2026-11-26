using Shifa.Application.DTOs.MedicalRecords;
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Application.Interfaces.MedicalRecords;

public interface IMedicalRecordRepository
{
    Task<int?> GetPatientIdByRecordIdAsync(int recordId);

    Task<(IReadOnlyList<MedicalRecordListItemResponse> Records, int TotalCount)>
        GetPatientRecordsAsync(
            int patientId,
            int pageNumber,
            int pageSize);

    Task<MedicalRecordDetailsResponse?> GetDetailsByIdAsync(int recordId);

    Task<int?> GetDoctorIdByUserIdAsync(int userId);

    Task<bool> PatientExistsAsync(int patientId);

    Task<bool> HasActivePermissionAsync(int patientId, int doctorId);

    Task AddAsync(MedicalRecord record);

    Task SaveChangesAsync();
}