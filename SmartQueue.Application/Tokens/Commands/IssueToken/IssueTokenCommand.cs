using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Tokens.Commands.IssueToken;

public record IssueTokenCommand(
    Guid ServiceId,
    Guid UserId,
    string IdempotencyKey
) : IRequest<Result<IssueTokenResponse>>;

public record IssueTokenResponse(
    Guid TokenId,
    int TokenNumber,
    int Position,
    string EstimatedWait,
    DateTime ExpiresAt
);
