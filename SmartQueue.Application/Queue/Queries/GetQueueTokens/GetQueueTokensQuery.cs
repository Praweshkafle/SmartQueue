using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Queue.Queries.GetQueueTokens;

public record GetQueueTokensQuery(Guid ServiceId)
    : IRequest<Result<IEnumerable<QueueTokenResponse>>>;

public record QueueTokenResponse(
    Guid TokenId,
    int TokenNumber,
    Guid UserId,
    string Status,
    DateTime IssuedAt,
    DateTime ExpiresAt
);