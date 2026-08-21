using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Constants;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Tokens.Commands.IssueToken;

public class IssueTokenHandler : IRequestHandler<IssueTokenCommand, Result<IssueTokenResponse>>
{
    private readonly IUnitOfWork _uow;

    private readonly ICacheService _cache;
    public IssueTokenHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<IssueTokenResponse>> Handle(
        IssueTokenCommand request, CancellationToken ct)
    {
        var existing = await _uow.Tokens.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
        {
            var pos = await _uow.Tokens.GetWaitingCountAsync(request.ServiceId, ct);
            return Result<IssueTokenResponse>.Success(MapResponse(existing, pos));
        }

        var service = await _uow.Services.GetByIdAsync(request.ServiceId, ct);
        if (service is null)
            return Result<IssueTokenResponse>.Failure("Service not found.", 404);
        if (!service.IsActive)
            return Result<IssueTokenResponse>.Failure("Service is currently inactive.", 400);

        var waitingCount = await _uow.Tokens.GetWaitingCountAsync(request.ServiceId, ct);
        if (waitingCount >= service.MaxQueueSize)
            return Result<IssueTokenResponse>.Failure("Queue is full. Please try again later.", 400);

        var tokenNumber = await _uow.Tokens.GetNextTokenNumberAsync(request.ServiceId, ct);
        var token = new Token
        {
            Id = Guid.NewGuid(),
            TokenNumber = tokenNumber,
            UserId = request.UserId,
            ServiceId = request.ServiceId,
            IdempotencyKey = request.IdempotencyKey,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        await _uow.Tokens.AddAsync(token, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.DeleteAsync(CacheKeys.QueueStatus(request.ServiceId), ct);
        return Result<IssueTokenResponse>.Success(MapResponse(token, waitingCount + 1));
    }

    private static IssueTokenResponse MapResponse(Token token, int position) =>
        new(token.Id,
            token.TokenNumber,
            position,
            $"{position * 5} mins",
            token.ExpiresAt);
}