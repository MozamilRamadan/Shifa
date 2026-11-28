
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Shifa.Application.DTOs.Storage;
using Shifa.Application.Interfaces.Storage;

namespace Shifa.Infrastructure.Storage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private const string Folder = "shifa/medical-files";

    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<FileStorageResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        if (!fileStream.CanRead)
            throw new ArgumentException("File stream is not readable.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.");

        cancellationToken.ThrowIfCancellationRequested();

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("File extension is required.");

        // Do not use the original filename as the storage identifier.
        var publicId = $"{Folder}/{Guid.NewGuid():N}{extension}";

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(
                Path.GetFileName(fileName),
                fileStream),

            PublicId = publicId,

            Type = "authenticated"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException(
                $"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        return new FileStorageResult
        {
            FileUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
            StorageIdentifier = uploadResult.PublicId,
            FileFormat = extension.TrimStart('.'),
            FileSize = uploadResult.Bytes
        };
    }

    public async Task DeleteAsync(
        string storageIdentifier,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageIdentifier))
            throw new ArgumentException("Storage identifier is required.");

        if (!storageIdentifier.StartsWith(
                $"{Folder}/",
                StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid storage identifier.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var deleteParams = new DeletionParams(storageIdentifier)
        {
            ResourceType = ResourceType.Raw,
            Type = "authenticated"
        };

        var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

        if (deleteResult.Error != null)
        {
            throw new InvalidOperationException(
                $"Cloudinary deletion failed: {deleteResult.Error.Message}");
        }
    }
}
