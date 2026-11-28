namespace Shifa.Application.Interfaces.MedicalRecords;

public interface IMedicalFileService
{
    Task<int> UploadAsync(
        int recordId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<(Stream FileStream, string FileName, string FileType)> DownloadAsync(
    int fileId,
    int currentUserId,
    CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int fileId,
        int currentUserId,
        CancellationToken cancellationToken = default);

}