
using Shifa.Application.DTOs.MedicalRecords;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Application.Exceptions;
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Application.Services.MedicalRecords;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _repository;

    public MedicalRecordService(IMedicalRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<int?> GetPatientIdByRecordIdAsync(int recordId)
    {
        return await _repository.GetPatientIdByRecordIdAsync(recordId);
    }

    public async Task<PagedMedicalRecordsResponse?> GetPatientRecordsAsync(
        int patientId,
        int currentUserId,
        bool isOwnerOrAdmin,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        if (!isOwnerOrAdmin)
        {
            var doctorId =
                await _repository.GetDoctorIdByUserIdAsync(currentUserId);

            if (doctorId is null)
                throw new ForbiddenException(
                    "You do not have permission to access this resource.");

            var hasPermission =
                await _repository.HasActivePermissionAsync(
                    patientId, doctorId.Value);

            if (!hasPermission)
                throw new ForbiddenException(
                    "You do not have permission to access this patient's records.");
        }

        var (records, totalCount) =
            await _repository.GetPatientRecordsAsync(
                patientId, pageNumber, pageSize);

        return new PagedMedicalRecordsResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize),
            Data = records
        };
    }

    public async Task<MedicalRecordDetailsResponse?> GetByIdAsync(
        int recordId,
        int currentUserId,
        bool isOwnerOrAdmin)
    {
        var record =
            await _repository.GetDetailsByIdAsync(recordId);

        if (record is null)
            return null;

        if (!isOwnerOrAdmin)
        {
            var doctorId =
                await _repository.GetDoctorIdByUserIdAsync(currentUserId);

            if (doctorId is null)
                throw new ForbiddenException(
                    "You do not have permission to access this resource.");

            var hasPermission =
                await _repository.HasActivePermissionAsync(
                    record.PatientId, doctorId.Value);

            if (!hasPermission)
                throw new ForbiddenException(
                    "You do not have permission to access this medical record.");
        }

        return record;
    }

    public async Task<int> CreateAsync(
        CreateMedicalRecordRequest request,
        int currentUserId)
    {
        var doctorId =
            await _repository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException(
                "Only registered doctors can create medical records.");

        var patientExists =
            await _repository.PatientExistsAsync(request.PatientId);

        if (!patientExists)
            throw new NotFoundException("Patient not found.");

        var hasPermission =
            await _repository.HasActivePermissionAsync(
                request.PatientId, doctorId.Value);

        if (!hasPermission)
            throw new ForbiddenException(
                "You do not have permission to create a record for this patient.");

        var record = new MedicalRecord
        {
            PatientId = request.PatientId,
            DoctorId = doctorId.Value,
            Title = request.Title,
            Diagnosis = request.Diagnosis,
            Description = request.Description,
            Treatment = request.Treatment,
            Prescription = request.Prescription,
            RecordDate = DateTime.UtcNow
        };

        await _repository.AddAsync(record);
        await _repository.SaveChangesAsync();

        return record.Id;
    }
}
