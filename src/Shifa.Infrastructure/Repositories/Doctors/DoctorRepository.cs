using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.Doctors;
using Shifa.Application.Interfaces.Doctors;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Enums;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.Doctors;

public class DoctorRepository : IDoctorRepository
{
    private readonly ShifaDbContext _context;

    public DoctorRepository(ShifaDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorProfileResponse?> GetByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => new DoctorProfileResponse
            {
                Id = d.Id,
                UserId = d.UserId,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Qualification = d.Qualification,
                YearsOfExperience = d.YearsOfExperience,
                ClinicAddress = d.ClinicAddress,
                ApplicationStatus = d.ApplicationStatus
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DoctorProfileResponse?> CreateAsync(
        int userId,
        CreateDoctorProfileRequest request)
    {
        var doctor = new Doctor
        {
            UserId = userId,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            Qualification = request.Qualification,
            YearsOfExperience = request.YearsOfExperience,
            ClinicAddress = request.ClinicAddress,
            ApplicationStatus = DoctorApplicationStatus.Pending
        };

        await _context.Doctors.AddAsync(doctor);
        await _context.SaveChangesAsync();

        return await GetByUserIdAsync(userId);
    }

    public Task<bool> LicenseNumberExistsAsync(string licenseNumber)
    {
        return _context.Doctors
            .AnyAsync(d => d.LicenseNumber == licenseNumber);
    }
}