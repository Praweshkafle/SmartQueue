using Microsoft.Extensions.Logging;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Enums;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Infrastructure.Jobs;

public class TokenExpiryJob
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ILogger<TokenExpiryJob> _logger;

    public TokenExpiryJob(
        IUnitOfWork uow,
        ICacheService cache,
        ILogger<TokenExpiryJob> logger)
    {
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Token expiry job started at {Time}", DateTime.UtcNow);

        var expiredTokens = await _uow.Tokens.GetExpiredTokensAsync(CancellationToken.None);
        var tokenList = expiredTokens.ToList();

        if (!tokenList.Any())
        {
            _logger.LogInformation("No expired tokens found.");
            return;
        }

        var affectedServiceIds = new HashSet<Guid>();

        foreach (var token in tokenList)
        {
            if (token.Status != TokenStatus.Waiting) continue;

            token.Status = TokenStatus.Expired;
            _uow.Tokens.Update(token);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                TokenId = token.Id,
                Action = "Expired",
                PerformedBy = "System",
                PerformedAt = DateTime.UtcNow,
                Notes = "Token expired automatically by system job."
            }, CancellationToken.None);

            affectedServiceIds.Add(token.ServiceId);
        }

        await _uow.SaveChangesAsync(CancellationToken.None);

        foreach (var serviceId in affectedServiceIds)
            await _cache.DeleteAsync($"queue:status:{serviceId}", CancellationToken.None);

        _logger.LogInformation("Expired {Count} tokens.", tokenList.Count);
    }
}