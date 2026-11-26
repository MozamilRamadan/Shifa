using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.Patients;
using Shifa.Application.Interfaces.Patients;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.Patients;

public class PatientRepository : IPatientRepository
{
    private readonly ShifaDbContext _context;

    public PatientRepository(ShifaDbContext context)
    {
        _context = context;
    }

    public async Task<PatientProfileResponse?> GetByIdAsync(
        int patientId)
    {
        return await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == patientId)
            .Select(p => new PatientProfileResponse
            {
                Id = p.Id,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                BloodType = p.BloodType,
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PatientProfileResponse?> GetByUserIdAsync(
        int userId)
    {
        return await _context.Patients
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new PatientProfileResponse
            {
                Id = p.Id,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                BloodType = p.BloodType,
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone
            })
            .FirstOrDefaultAsync();
    }
}