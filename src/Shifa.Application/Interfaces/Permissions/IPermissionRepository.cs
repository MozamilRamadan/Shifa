using Shifa.Application.DTOs.Permissions;
using Shifa.Domain.Entities.Permissions;

namespace Shifa.Application.Interfaces.Permissions;

public interface IPermissionRepository
{
    Task<int?> GetDoctorIdByUserIdAsync(int userId);

    Task<int?> GetPatientIdByUserIdAsync(int userId);

    Task<bool> PatientExistsAsync(int patientId);

    Task<bool> PendingRequestExistsAsync(
        int patientId,
        int doctorId);

    Task AddAsync(PermissionRequest request);

    Task<PermissionRequest?> GetRequestEntityByIdAsync(
    int requestId,
    int patientId);

    Task<PagedPermissionsResponse<PermissionRequestResponse>>
        GetPatientRequestsAsync(
            int patientId,
            int pageNumber,
            int pageSize);

    Task<PagedPermissionsResponse<ApprovedPatientResponse>>
        GetApprovedPatientsAsync(
            int doctorId,
            int pageNumber,
            int pageSize);

    Task SaveChangesAsync();
}