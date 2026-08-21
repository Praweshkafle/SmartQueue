using SmartQueue.Domain.Entities;

namespace SmartQueue.Domain.Interfaces;

public interface ITokenRepository : IGenericRepository<Token>
{
    Task<Token?> GetByIdempotencyKeyAsync(string key, CancellationToken ct);
    Task<int> GetWaitingCountAsync(Guid serviceId, CancellationToken ct);
    Task<int> GetNextTokenNumberAsync(Guid serviceId, CancellationToken ct);
    Task<IEnumerable<Token>> GetWaitingByServiceAsync(Guid serviceId, CancellationToken ct);
    Task<Token?> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<IEnumerable<Token>> GetExpiredTokensAsync(CancellationToken ct);
    Task<Token?> GetCurrentCalledTokenAsync(Guid serviceId, CancellationToken ct);
    Task<Token?> GetNextWaitingTokenAsync(Guid serviceId, CancellationToken ct);
    Task<IEnumerable<Token>> GetStaleCalledTokensAsync(int graceMinutes, CancellationToken ct);

    Task<IEnumerable<Token>> GetCalledAndWaitingTokensAsync(CancellationToken ct);
}
