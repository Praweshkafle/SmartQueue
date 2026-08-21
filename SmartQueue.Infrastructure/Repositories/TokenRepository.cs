using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Enums;
using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.Infrastructure.Repositories;

public class TokenRepository : GenericRepository<Token>, ITokenRepository
{
    public TokenRepository(AppDbContext db) : base(db) { }

    public async Task<Token?> GetByIdempotencyKeyAsync(string key, CancellationToken ct) =>
        await _set.FirstOrDefaultAsync(t => t.IdempotencyKey == key, ct);

    public async Task<int> GetWaitingCountAsync(Guid serviceId, CancellationToken ct) =>
        await _set.CountAsync(
            t => t.ServiceId == serviceId && t.Status == TokenStatus.Waiting, ct);

    public async Task<int> GetNextTokenNumberAsync(Guid serviceId, CancellationToken ct)
    {
        var max = await _set
            .Where(t => t.ServiceId == serviceId)
            .MaxAsync(t => (int?)t.TokenNumber, ct);
        return (max ?? 0) + 1;
    }
    public async Task<IEnumerable<Token>> GetWaitingByServiceAsync(Guid serviceId, CancellationToken ct) =>
        await _set
            .Where(t => t.ServiceId == serviceId && t.Status == TokenStatus.Waiting)
            .OrderBy(t => t.TokenNumber)
            .ToListAsync(ct);

    public async Task<Token?> GetActiveByUserAsync(Guid userId, CancellationToken ct) => 
        await _set
            .Include(s=> s.Service)
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.Status == TokenStatus.Waiting ||
                t.Status == TokenStatus.Called, ct);

    public async Task<IEnumerable<Token>> GetExpiredTokensAsync(CancellationToken ct) =>
        await _set
            .Where(t =>
                t.Status == TokenStatus.Waiting &&
                t.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<Token?> GetCurrentCalledTokenAsync(Guid serviceId, CancellationToken ct) =>
        await _set
            .Where(t => t.ServiceId == serviceId && t.Status == TokenStatus.Called)
            .OrderBy(t => t.TokenNumber)
            .FirstOrDefaultAsync(ct);
    
    public async Task<Token?> GetNextWaitingTokenAsync(Guid serviceId, CancellationToken ct) =>
        await _set
            .Where(t => t.ServiceId == serviceId && t.Status == TokenStatus.Waiting)
            .OrderBy(t => t.TokenNumber)
            .FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Token>> GetStaleCalledTokensAsync(
        int graceMinutes, CancellationToken ct) =>
        await _set
            .Where(t =>
                t.Status == TokenStatus.Called &&
                t.IssuedAt <= DateTime.UtcNow.AddMinutes(-graceMinutes))
            .ToListAsync(ct);

    public async Task<IEnumerable<Token>> GetCalledAndWaitingTokensAsync(CancellationToken ct)
    {
       return await _set
            .Where(t => t.Status == TokenStatus.Waiting && t.Status == TokenStatus.Called)
            .ToListAsync(ct);
    }
}
