using Shifa.Application.DTOs.Permissions;
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.Permissions;
using Shifa.Domain.Entities.Permissions;
using Shifa.Domain.Enums;

namespace Shifa.Application.Services.Permissions;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;

    public PermissionService(IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateRequestAsync(
        int patientId,
        string? reason,
        int currentUserId)
    {
        var doctorId =
            await _repository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException("Doctor profile not found.");

        if (!await _repository.PatientExistsAsync(patientId))
            throw new NotFoundException("Patient not found.");

        if (await _repository.PendingRequestExistsAsync(
                patientId, doctorId.Value))
        {
            throw new ConflictException(
                "A pending request already exists.");
        }

        var request = new PermissionRequest
        {
            PatientId = patientId,
            DoctorId = doctorId.Value,
            Reason = reason,
            Status = PermissionStatus.Pending
        };

        await _repository.AddAsync(request);
        await _repository.SaveChangesAsync();

        return request.Id;
    }

    public async Task<PagedPermissionsResponse<PermissionRequestResponse>>
        GetMyRequestsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        var patientId =
            await _repository.GetPatientIdByUserIdAsync(currentUserId);

        if (patientId is null)
            throw new NotFoundException("Patient profile not found.");

        return await _repository.GetPatientRequestsAsync(
            patientId.Value, pageNumber, pageSize);
    }

    public async Task<PermissionResponse> RespondToRequestAsync(
        int requestId,
        bool approve,
        int currentUserId)
    {
        var patientId =
            await _repository.GetPatientIdByUserIdAsync(currentUserId);

        if (patientId is null)
            throw new NotFoundException("Patient profile not found.");

        var request =
            await _repository.GetRequestEntityByIdAsync(
                requestId, patientId.Value);

        if (request is null)
            throw new NotFoundException("Permission request not found.");

        if (request.Status != PermissionStatus.Pending)
            throw new ConflictException(
                "Request has already been processed.");

        request.Status = approve
            ? PermissionStatus.Approved
            : PermissionStatus.Rejected;

        request.RespondedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return MapResponse(request);
    }

    public async Task<PermissionResponse> RevokePermissionAsync(
        int requestId,
        int currentUserId)
    {
        var patientId =
            await _repository.GetPatientIdByUserIdAsync(currentUserId);

        if (patientId is null)
            throw new NotFoundException("Patient profile not found.");

        var request =
            await _repository.GetRequestEntityByIdAsync(
                requestId, patientId.Value);

        if (request is null)
            throw new NotFoundException("Permission request not found.");

        if (request.Status != PermissionStatus.Approved)
            throw new ConflictException(
                "Only approved permissions can be revoked.");

        request.Status = PermissionStatus.Revoked;
        request.RespondedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return MapResponse(request);
    }

    public async Task<PagedPermissionsResponse<ApprovedPatientResponse>>
        GetMyApprovedPatientsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        var doctorId =
            await _repository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException("Doctor profile not found.");

        return await _repository.GetApprovedPatientsAsync(
            doctorId.Value, pageNumber, pageSize);
    }

    private static void ValidatePagination(
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException(
                "Invalid pagination parameters.");

        if (pageSize > 100)
            throw new ArgumentException(
                "Page size cannot exceed 100.");
    }

    private static PermissionResponse MapResponse(
        PermissionRequest request)
    {
        return new PermissionResponse
        {
            Id = request.Id,
            Status = request.Status.ToString(),
            RespondedAt = request.RespondedAt
        };
    }
}
