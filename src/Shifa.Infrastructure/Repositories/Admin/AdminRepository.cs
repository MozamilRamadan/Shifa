using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.Admin;
using Shifa.Application.Interfaces.Admin;
using Shifa.Domain.Enums;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly ShifaDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminRepository(
        ShifaDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        return new AdminDashboardResponse
        {
            TotalUsers = await _context.Users.CountAsync(),

            TotalPatients = await _context.Patients.CountAsync(),

            TotalDoctors = await _context.Doctors.CountAsync(),

            VerifiedDoctors = await _context.Doctors
                .CountAsync(d => d.IsVerified),

            PendingDoctors = await _context.Doctors
                .CountAsync(d => !d.IsVerified),

            TotalAppointments = await _context.Appointments.CountAsync(),

            PendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Pending),

            TotalMedicalRecords = await _context.MedicalRecords.CountAsync()
        };
    }

    public async Task<PagedAdminResponse<AdminUserResponse>> GetUsersAsync(
        int pageNumber,
        int pageSize)
    {
        var query = _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            items.Add(new AdminUserResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = string.Join(", ", roles),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return new PagedAdminResponse<AdminUserResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedAdminResponse<AdminDoctorResponse>> GetDoctorsAsync(
        int pageNumber,
        int pageSize)
    {
        var query =
            from doctor in _context.Doctors.AsNoTracking()
            join user in _context.Users.AsNoTracking()
                on doctor.UserId equals user.Id
            orderby doctor.CreatedAt descending
            select new AdminDoctorResponse
            {
                DoctorId = doctor.Id,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                IsVerified = doctor.IsVerified,
                CreatedAt = doctor.CreatedAt
            };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedAdminResponse<AdminDoctorResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> UpdateUserStatusAsync(
        int userId,
        bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }

    public async Task<bool> UpdateDoctorVerificationAsync(
        int doctorId,
        bool isVerified)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor is null)
            return false;

        doctor.IsVerified = isVerified;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}