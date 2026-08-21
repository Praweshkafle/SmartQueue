using MediatR;
using SmartQueue.Application.Common;


namespace SmartQueue.Application.Tokens.Queries.GetMyToken;

public record GetMyTokenQuery(Guid UserId)
    : IRequest<Result<MyTokenResponse>>;

public record MyTokenResponse(
    Guid TokenId,
    int TokenNumber,
    string ServiceName,
    int Position,
    string EstimatedWait,
    string Status,
    DateTime ExpiresAt
);