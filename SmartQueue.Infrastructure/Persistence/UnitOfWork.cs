using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public ITokenRepository Tokens { get; }
    public IServiceRepository Services { get; }
    public IUserRepository Users { get; }
    public IAuditLogRepository AuditLogs { get; }
    
    public UnitOfWork(AppDbContext db, ITokenRepository tokens, IServiceRepository services, IUserRepository users,
        IAuditLogRepository auditLogs)
    {
        _db = db;
        Tokens = tokens;
        AuditLogs = auditLogs;
        Users = users;
        Services = services;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);
}
