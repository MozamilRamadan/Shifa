namespace Shifa.Application.DTOs.Permissions;

public class PagedPermissionsResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IReadOnlyList<T> Data { get; set; } = [];
}