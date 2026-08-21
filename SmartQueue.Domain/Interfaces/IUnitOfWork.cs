namespace SmartQueue.Domain.Interfaces;

public interface IUnitOfWork 
{
    ITokenRepository Tokens { get; }
    IServiceRepository Services { get; }
    IUserRepository Users { get; }
    
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct); 
}