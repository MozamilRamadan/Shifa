using Shifa.Application.DTOs.Admin;
using Shifa.Domain.Enums;

namespace Shifa.Application.Interfaces.Admin;

public interface IAdminRepository
{
    Task<AdminDashboardResponse> GetDashboardAsync();

    Task<PagedAdminResponse<AdminUserResponse>> GetUsersAsync(
        int pageNumber,
        int pageSize);

    Task<PagedAdminResponse<AdminDoctorResponse>> GetDoctorsAsync(
        int pageNumber,
        int pageSize);

    Task<bool> UpdateUserStatusAsync(int userId, bool isActive);

    Task<bool> UpdateDoctorApplicationStatusAsync(
        int doctorId,
        DoctorApplicationStatus applicationStatus);
}