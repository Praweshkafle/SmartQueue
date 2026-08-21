using SmartQueue.Domain.Entities;

namespace SmartQueue.Domain.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByTokenIdAsync(Guid tokenId, CancellationToken ct);
}