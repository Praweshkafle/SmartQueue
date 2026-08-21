namespace SmartQueue.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid TokenId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    public Token Token { get; set; } = null!;
}