using Shifa.Application.DTOs.MedicalRecords;

namespace Shifa.Application.Interfaces.MedicalRecords;

public interface IMedicalRecordService
{
    Task<int?> GetPatientIdByRecordIdAsync(int recordId);
    Task<PagedMedicalRecordsResponse> GetPatientRecordsAsync(
        int patientId,
        int currentUserId,
        bool isOwnerOrAdmin,
        int pageNumber,
        int pageSize);

    Task<MedicalRecordDetailsResponse?> GetByIdAsync(
        int recordId,
        int currentUserId,
        bool isOwnerOrAdmin);

    Task<int> CreateAsync(
        CreateMedicalRecordRequest request,
        int currentUserId);
}