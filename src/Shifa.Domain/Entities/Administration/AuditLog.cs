
using Shifa.Domain.Entities.Common;

namespace Shifa.Domain.Entities.Administration;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
