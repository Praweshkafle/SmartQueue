using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<AuditLog>> GetByTokenIdAsync(Guid tokenId, CancellationToken ct) =>
        await _set
            .Where(a => a.TokenId == tokenId)
            .OrderBy(a => a.PerformedAt)
            .ToListAsync(ct);
}