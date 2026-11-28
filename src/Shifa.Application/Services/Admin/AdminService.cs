using Shifa.Application.DTOs.Admin;
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.Admin;
using Shifa.Domain.Enums;

namespace Shifa.Application.Services.Admin;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public Task<AdminDashboardResponse> GetDashboardAsync()
    {
        return _adminRepository.GetDashboardAsync();
    }

    public async Task<PagedAdminResponse<AdminUserResponse>> GetUsersAsync(
        int pageNumber,
        int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        return await _adminRepository.GetUsersAsync(pageNumber, pageSize);
    }

    public async Task<PagedAdminResponse<AdminDoctorResponse>> GetDoctorsAsync(
        int pageNumber,
        int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        return await _adminRepository.GetDoctorsAsync(pageNumber, pageSize);
    }

    public async Task UpdateUserStatusAsync(
        int userId,
        UpdateUserStatusRequest request)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        ArgumentNullException.ThrowIfNull(request);

        var updated = await _adminRepository.UpdateUserStatusAsync(
            userId, request.IsActive);

        if (!updated)
            throw new NotFoundException("User not found.");
    }

    public async Task UpdateDoctorApplicationStatusAsync(
       int doctorId,
       UpdateDoctorVerificationRequest request)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor ID.");

        ArgumentNullException.ThrowIfNull(request);

        if (request.ApplicationStatus == DoctorApplicationStatus.Pending)
            throw new ArgumentException("Invalid application status.");

        if (!Enum.IsDefined(request.ApplicationStatus))
            throw new ArgumentException("Invalid application status.");

        var updated = await _adminRepository.UpdateDoctorApplicationStatusAsync(
            doctorId,
            request.ApplicationStatus);

        if (!updated)
            throw new NotFoundException("Doctor not found.");
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than zero.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100.");
    }
}