namespace Shifa.Application.DTOs.Storage;

public class FileStorageResult
{
    public string FileUrl { get; set; } = string.Empty;

    public string StorageIdentifier { get; set; } = string.Empty;

    public string FileFormat { get; set; } = string.Empty;

    public long FileSize { get; set; }
}