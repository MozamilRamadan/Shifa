using Shifa.Application.DTOs.Permissions;

namespace Shifa.Application.Interfaces.Permissions;

public interface IPermissionService
{
    Task<int> CreateRequestAsync(
        int patientId,
        string? reason,
        int currentUserId);

    Task<PagedPermissionsResponse<PermissionRequestResponse>>
        GetMyRequestsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize);

    Task<PermissionResponse> RespondToRequestAsync(
        int requestId,
        bool approve,
        int currentUserId);

    Task<PermissionResponse> RevokePermissionAsync(
        int requestId,
        int currentUserId);

    Task<PagedPermissionsResponse<ApprovedPatientResponse>>
        GetMyApprovedPatientsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize);
}