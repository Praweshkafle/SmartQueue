using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Constants;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Enums;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Queue.Commands.AdvanceQueue;

public class AdvanceQueueHandler : IRequestHandler<AdvanceQueueCommand, Result<AdvanceQueueResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public AdvanceQueueHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<AdvanceQueueResponse>> Handle(
        AdvanceQueueCommand request, CancellationToken ct)
    {
        var current = await _uow.Tokens.GetCurrentCalledTokenAsync(request.ServiceId, ct);
        if (current is not null)
        {
            current.Status = TokenStatus.Completed;
            _uow.Tokens.Update(current);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                TokenId = current.Id,
                Action = "Completed",
                PerformedBy = request.PerformedBy,
                PerformedAt = DateTime.UtcNow,
                Notes = "Token completed via queue advance."
            }, ct);
        }

        var next = await _uow.Tokens.GetNextWaitingTokenAsync(request.ServiceId, ct);
        if (next is not null)
        {
            next.Status = TokenStatus.Called;
            _uow.Tokens.Update(next);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                TokenId = next.Id,
                Action = "Called",
                PerformedBy = request.PerformedBy,
                PerformedAt = DateTime.UtcNow,
                Notes = "Token called via queue advance."
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);

        await _cache.DeleteAsync(CacheKeys.QueueStatus(request.ServiceId),ct);
        return Result<AdvanceQueueResponse>.Success(new AdvanceQueueResponse(
            current?.TokenNumber,
            next?.TokenNumber,
            next is null ? "Queue is empty." : $"Now serving token {next.TokenNumber}."));
    }
}