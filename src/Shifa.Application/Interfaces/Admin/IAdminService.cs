using Shifa.Application.DTOs.Admin;

namespace Shifa.Application.Interfaces.Admin;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync();

    Task<PagedAdminResponse<AdminUserResponse>> GetUsersAsync(
        int pageNumber,
        int pageSize);

    Task<PagedAdminResponse<AdminDoctorResponse>> GetDoctorsAsync(
        int pageNumber,
        int pageSize);

    Task UpdateUserStatusAsync(
        int userId,
        UpdateUserStatusRequest request);

    Task UpdateDoctorVerificationAsync(
        int doctorId,
        UpdateDoctorVerificationRequest request);
}