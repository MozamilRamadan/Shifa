
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Application.Interfaces.Storage;
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Application.Services.MedicalRecords;

public class MedicalFileService : IMedicalFileService
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private readonly IMedicalFileRepository _fileRepository;
    private readonly IMedicalRecordRepository _recordRepository;
    private readonly IFileStorageService _storageService;

    public MedicalFileService(
        IMedicalFileRepository fileRepository,
        IMedicalRecordRepository recordRepository,
        IFileStorageService storageService)
    {
        _fileRepository = fileRepository;
        _recordRepository = recordRepository;
        _storageService = storageService;
    }

    public async Task<int> UploadAsync(
        int recordId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (recordId <= 0)
            throw new ArgumentOutOfRangeException(nameof(recordId));

        if (fileStream == null || !fileStream.CanRead)
            throw new ArgumentException("Invalid file stream.");

        if (fileSize <= 0 || fileSize > MaxFileSize)
            throw new ArgumentException(
                "File size must be between 1 byte and 20 MB.");

        var patientId =
            await _recordRepository.GetPatientIdByRecordIdAsync(recordId);

        if (patientId is null)
            throw new NotFoundException("Medical record not found.");

        var doctorId =
            await _recordRepository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException(
                "Only registered doctors can upload medical files.");

        var hasPermission =
            await _recordRepository.HasActivePermissionAsync(
                patientId.Value,
                doctorId.Value);

        if (!hasPermission)
            throw new ForbiddenException(
                "You do not have permission to access this patient's records.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        var allowedTypes = new Dictionary<string, string>
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

        if (!allowedTypes.TryGetValue(extension, out var expectedContentType) ||
            !string.Equals(
                contentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only PDF, JPG, JPEG and PNG files are allowed.");
        }

        if (!fileStream.CanSeek)
            throw new ArgumentException(
                "The file stream must support seeking.");

        var originalPosition = fileStream.Position;
        var header = new byte[8];

        try
        {
            var bytesRead = await fileStream.ReadAsync(
                header.AsMemory(0, header.Length),
                cancellationToken);

            fileStream.Position = originalPosition;

            var isValidSignature = extension switch
            {
                ".pdf" => bytesRead >= 5 &&
                          header[0] == 0x25 &&
                          header[1] == 0x50 &&
                          header[2] == 0x44 &&
                          header[3] == 0x46 &&
                          header[4] == 0x2D,

                ".jpg" or ".jpeg" => bytesRead >= 3 &&
                                     header[0] == 0xFF &&
                                     header[1] == 0xD8 &&
                                     header[2] == 0xFF,

                ".png" => bytesRead >= 8 &&
                          header[0] == 0x89 &&
                          header[1] == 0x50 &&
                          header[2] == 0x4E &&
                          header[3] == 0x47 &&
                          header[4] == 0x0D &&
                          header[5] == 0x0A &&
                          header[6] == 0x1A &&
                          header[7] == 0x0A,

                _ => false
            };

            if (!isValidSignature)
                throw new ArgumentException(
                    "The file content does not match its extension.");
        }
        finally
        {
            fileStream.Position = originalPosition;
        }

        var storageResult = await _storageService.UploadAsync(
            fileStream,
            Path.GetFileName(fileName),
            contentType,
            cancellationToken);

        try
        {
            var medicalFile = new MedicalFile
            {
                MedicalRecordId = recordId,
                FileName = Path.GetFileName(fileName),
                FileUrl = storageResult.FileUrl,
                StorageIdentifier = storageResult.StorageIdentifier,
                FileType = storageResult.FileFormat,
                FileSize = storageResult.FileSize,
                UploadedAt = DateTime.UtcNow
            };

            await _fileRepository.AddAsync(medicalFile);
            await _fileRepository.SaveChangesAsync();

            return medicalFile.Id;
        }
        catch
        {
            await _storageService.DeleteAsync(
                storageResult.StorageIdentifier,
                cancellationToken);

            throw;
        }
    }

    public async Task<(Stream FileStream, string FileName, string FileType)> DownloadAsync(
    int fileId,
    int currentUserId,
    CancellationToken cancellationToken = default)
    {
        if (fileId <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileId));

        var file = await _fileRepository.GetByIdAsync(fileId);

        if (file is null)
            throw new NotFoundException("Medical file not found.");

        var patientId =
            await _recordRepository.GetPatientIdByRecordIdAsync(
                file.MedicalRecordId);

        if (patientId is null)
            throw new NotFoundException("Medical record not found.");

        var doctorId =
            await _recordRepository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException(
                "Only registered doctors can download medical files.");

        var hasPermission =
            await _recordRepository.HasActivePermissionAsync(
                patientId.Value,
                doctorId.Value);

        if (!hasPermission)
            throw new ForbiddenException(
                "You do not have permission to access this patient's records.");

        if (string.IsNullOrWhiteSpace(file.StorageIdentifier))
            throw new InvalidOperationException(
                "The file storage identifier is missing.");

        var stream = await _storageService.DownloadAsync(
            file.StorageIdentifier,
            cancellationToken);

        var contentType = file.FileType.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            _ => "application/octet-stream"
        };

        return (stream, file.FileName, contentType);
    }

    public async Task DeleteAsync(
        int fileId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);

        if (file is null)
            throw new NotFoundException("Medical file not found.");

        var patientId =
            await _recordRepository.GetPatientIdByRecordIdAsync(
                file.MedicalRecordId);

        if (patientId is null)
            throw new NotFoundException("Medical record not found.");

        var doctorId =
            await _recordRepository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new ForbiddenException(
                "Only registered doctors can delete medical files.");

        var hasPermission =
            await _recordRepository.HasActivePermissionAsync(
                patientId.Value,
                doctorId.Value);

        if (!hasPermission)
            throw new ForbiddenException(
                "You do not have permission to access this patient's records.");

        if (string.IsNullOrWhiteSpace(file.StorageIdentifier))
            throw new InvalidOperationException(
                "The file storage identifier is missing.");

        await _storageService.DeleteAsync(
            file.StorageIdentifier,
            cancellationToken);

        await _fileRepository.DeleteAsync(file);
        await _fileRepository.SaveChangesAsync();
    }
}
