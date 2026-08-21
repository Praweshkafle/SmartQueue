using Microsoft.Extensions.Logging;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Enums;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Infrastructure.Jobs;

public class QueueAutoAdvanceJob
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ILogger<QueueAutoAdvanceJob> _logger;

    private const int GraceMinutes = 10;

    public QueueAutoAdvanceJob(
        IUnitOfWork uow,
        ICacheService cache,
        ILogger<QueueAutoAdvanceJob> logger)
    {
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Queue auto-advance job started at {Time}", DateTime.UtcNow);

        var staleTokens = await _uow.Tokens
            .GetStaleCalledTokensAsync(GraceMinutes, CancellationToken.None);

        if (!staleTokens.Any())
        {
            _logger.LogInformation("No stale tokens found.");
            return;
        }
        
        foreach (var staleToken in staleTokens)
        {
            if (staleToken.Status != TokenStatus.Called) continue;

            staleToken.Status = TokenStatus.Expired;
            _uow.Tokens.Update(staleToken);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                TokenId = staleToken.Id,
                Action = "AutoAdvanced",
                PerformedBy = "System",
                PerformedAt = DateTime.UtcNow,
                Notes = $"Token auto-expired after {GraceMinutes} min grace period."
            }, CancellationToken.None);

            var next = await _uow.Tokens
                .GetNextWaitingTokenAsync(staleToken.ServiceId, CancellationToken.None);

            if (next is not null)
            {
                next.Status = TokenStatus.Called;
                _uow.Tokens.Update(next);

                await _uow.AuditLogs.AddAsync(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TokenId = next.Id,
                    Action = "Called",
                    PerformedBy = "System",
                    PerformedAt = DateTime.UtcNow,
                    Notes = "Token called via auto-advance job."
                }, CancellationToken.None);
            }

            await _uow.SaveChangesAsync(CancellationToken.None);
            await _cache.DeleteAsync(
                $"queue:status:{staleToken.ServiceId}", CancellationToken.None);

            _logger.LogInformation(
                "Auto-advanced queue for service {ServiceId}", staleToken.ServiceId);
        }
    }
}