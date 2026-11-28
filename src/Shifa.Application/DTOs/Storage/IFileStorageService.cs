using Shifa.Application.DTOs.Storage;

namespace Shifa.Application.Interfaces.Storage;

public interface IFileStorageService
{
    Task<FileStorageResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(
    string storageIdentifier,
    CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageIdentifier,
        CancellationToken cancellationToken = default);
}