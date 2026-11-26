using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.Permissions;
using Shifa.Application.Interfaces.Permissions;
using Shifa.Domain.Entities.Permissions;
using Shifa.Domain.Enums;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.Permissions;

public class PermissionRepository : IPermissionRepository
{
    private readonly ShifaDbContext _context;

    public PermissionRepository(ShifaDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetDoctorIdByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Where(d => d.UserId == userId)
            .Select(d => (int?)d.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetPatientIdByUserIdAsync(int userId)
    {
        return await _context.Patients
            .Where(p => p.UserId == userId)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();
    }

    public Task<bool> PatientExistsAsync(int patientId)
    {
        return _context.Patients
            .AnyAsync(p => p.Id == patientId);
    }

    public Task<bool> PendingRequestExistsAsync(
        int patientId,
        int doctorId)
    {
        return _context.PermissionRequests.AnyAsync(p =>
            p.PatientId == patientId &&
            p.DoctorId == doctorId &&
            p.Status == PermissionStatus.Pending);
    }

    public async Task AddAsync(PermissionRequest request)
    {
        await _context.PermissionRequests.AddAsync(request);
    }

    public async Task<PermissionRequest?> GetRequestEntityByIdAsync(
        int requestId,
        int patientId)
    {
        return await _context.PermissionRequests
            .FirstOrDefaultAsync(p =>
                p.Id == requestId &&
                p.PatientId == patientId);
    }

    public async Task<PagedPermissionsResponse<PermissionRequestResponse>>
        GetPatientRequestsAsync(
            int patientId,
            int pageNumber,
            int pageSize)
    {
        var query = _context.PermissionRequests
            .AsNoTracking()
            .Where(p => p.PatientId == patientId);

        var totalCount = await query.CountAsync();

        var requests = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PermissionRequestResponse
            {
                Id = p.Id,
                DoctorId = p.DoctorId,
                Reason = p.Reason,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                ExpiresAt = p.ExpiresAt
            })
            .ToListAsync();

        return new PagedPermissionsResponse<PermissionRequestResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            Data = requests
        };
    }

    public async Task<PagedPermissionsResponse<ApprovedPatientResponse>>
        GetApprovedPatientsAsync(
            int doctorId,
            int pageNumber,
            int pageSize)
    {
        var now = DateTime.UtcNow;

        var query =
            from permission in _context.PermissionRequests.AsNoTracking()
            join patient in _context.Patients
                on permission.PatientId equals patient.Id
            join user in _context.Users
                on patient.UserId equals user.Id
            where permission.DoctorId == doctorId
                && permission.Status == PermissionStatus.Approved
                && (permission.ExpiresAt == null ||
                    permission.ExpiresAt > now)
            select new ApprovedPatientResponse
            {
                PatientId = patient.Id,
                FullName = user.FirstName + " " + user.LastName,
                Status = permission.Status.ToString(),
                ExpiresAt = permission.ExpiresAt
            };

        var totalCount = await query.CountAsync();

        var patients = await query
            .OrderBy(p => p.PatientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedPermissionsResponse<ApprovedPatientResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            Data = patients
        };
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}