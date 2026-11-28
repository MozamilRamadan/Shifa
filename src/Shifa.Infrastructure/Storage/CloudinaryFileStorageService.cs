
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Shifa.Application.DTOs.Storage;
using Shifa.Application.Interfaces.Storage;
using System.Net.Http;

namespace Shifa.Infrastructure.Storage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private const string Folder = "shifa/medical-files";

    private readonly Cloudinary _cloudinary;
    private readonly HttpClient _httpClient;
    public CloudinaryFileStorageService(CloudinarySettings settings, HttpClient httpClient)
    {
        _httpClient = httpClient;

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
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

    public async Task<Stream> DownloadAsync(
    string storageIdentifier,
    CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storageIdentifier))
            throw new ArgumentException(
                "Storage identifier is required.",
                nameof(storageIdentifier));

        var fileUrl = _cloudinary.Api.Url
            .ResourceType("raw")
            .Type("authenticated")
            .Signed(true)
            .BuildUrl(storageIdentifier);

        using var response = await _httpClient.GetAsync(
            fileUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        const long maxFileSize = 20 * 1024 * 1024;

        if (response.Content.Headers.ContentLength > maxFileSize)
            throw new InvalidOperationException(
                "The requested file exceeds the maximum allowed size.");

        await using var sourceStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var memoryStream = new MemoryStream();
        var buffer = new byte[81920];
        long totalBytes = 0;

        try
        {
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(
                       buffer.AsMemory(0, buffer.Length),
                       cancellationToken)) > 0)
            {
                totalBytes += bytesRead;

                if (totalBytes > maxFileSize)
                    throw new InvalidOperationException(
                        "The requested file exceeds the maximum allowed size.");

                await memoryStream.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch
        {
            await memoryStream.DisposeAsync();
            throw;
        }
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
