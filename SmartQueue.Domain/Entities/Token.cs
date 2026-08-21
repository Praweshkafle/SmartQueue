using SmartQueue.Domain.Enums;

namespace SmartQueue.Domain.Entities;

public class Token
{
    public Guid Id { get; set; }
    public int TokenNumber { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public TokenStatus Status { get; set; } = TokenStatus.Waiting;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public uint RowVersion { get; set; }

    public User User { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}