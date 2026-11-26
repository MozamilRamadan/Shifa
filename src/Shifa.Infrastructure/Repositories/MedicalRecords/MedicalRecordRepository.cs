
using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.MedicalRecords;
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Domain.Enums;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.MedicalRecords;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly ShifaDbContext _context;

    public MedicalRecordRepository(ShifaDbContext context)
    {
        _context = context;
    }


    public async Task<(IReadOnlyList<MedicalRecordListItemResponse> Records, int TotalCount)>
        GetPatientRecordsAsync(
            int patientId,
            int pageNumber,
            int pageSize)
    {
        var query =
            from record in _context.MedicalRecords.AsNoTracking()
            join doctor in _context.Doctors.AsNoTracking()
                on record.DoctorId equals doctor.Id
            join user in _context.Users.AsNoTracking()
                on doctor.UserId equals user.Id
            where record.PatientId == patientId
            orderby record.RecordDate descending
            select new MedicalRecordListItemResponse
            {
                Id = record.Id,
                Title = record.Title,
                Diagnosis = record.Diagnosis,
                Description = record.Description,
                Treatment = record.Treatment,
                Prescription = record.Prescription,
                RecordDate = record.RecordDate,
                DoctorName = user.FirstName + " " + user.LastName,

                Files = record.Files.Select(file =>
                    new MedicalRecordFileResponse
                    {
                        Id = file.Id,
                        FileName = file.FileName,
                        FileUrl = file.FileUrl,
                        FileType = file.FileType,
                        FileSize = file.FileSize,
                        UploadedAt = file.UploadedAt
                    }).ToList()
            };

        var totalCount = await query.CountAsync();

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (records, totalCount);
    }

    public async Task<MedicalRecordDetailsResponse?> GetDetailsByIdAsync(
        int recordId)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .Where(record => record.Id == recordId)
            .Select(record => new MedicalRecordDetailsResponse
            {
                Id = record.Id,
                PatientId = record.PatientId,
                Title = record.Title,
                Diagnosis = record.Diagnosis,
                Description = record.Description,
                Treatment = record.Treatment,
                Prescription = record.Prescription,
                RecordDate = record.RecordDate,

                Files = record.Files.Select(file =>
                    new MedicalRecordFileResponse
                    {
                        Id = file.Id,
                        FileName = file.FileName,
                        FileUrl = file.FileUrl,
                        FileType = file.FileType,
                        FileSize = file.FileSize,
                        UploadedAt = file.UploadedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetDoctorIdByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Where(doctor => doctor.UserId == userId)
            .Select(doctor => (int?)doctor.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> PatientExistsAsync(int patientId)
    {
        return await _context.Patients
            .AnyAsync(patient => patient.Id == patientId);
    }

    public async Task<bool> HasActivePermissionAsync(
        int patientId,
        int doctorId)
    {
        var now = DateTime.UtcNow;

        return await _context.PermissionRequests
            .AnyAsync(permission =>
                permission.PatientId == patientId &&
                permission.DoctorId == doctorId &&
                permission.Status == PermissionStatus.Approved &&
                (permission.ExpiresAt == null ||
                 permission.ExpiresAt > now));
    }

    public async Task AddAsync(MedicalRecord record)
    {
        await _context.MedicalRecords.AddAsync(record);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task<int?> GetPatientIdByRecordIdAsync(int recordId)
    {
        throw new ForbiddenException("No Record Found ");
    }
}
