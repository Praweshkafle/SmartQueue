using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Tokens.Queries.GetMyToken;

public class GetMyTokenHandler : IRequestHandler<GetMyTokenQuery, Result<MyTokenResponse>>
{
    private readonly IUnitOfWork _uow;
    public GetMyTokenHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<MyTokenResponse>> Handle(
        GetMyTokenQuery request, CancellationToken ct)
    {
        var token = await _uow.Tokens.GetActiveByUserAsync(request.UserId, ct);
        if (token is null)
            return Result<MyTokenResponse>.Failure("No active token found.", 404);

        var position = await _uow.Tokens.GetWaitingCountAsync(token.ServiceId, ct);

        return Result<MyTokenResponse>.Success(new MyTokenResponse(
            token.Id,
            token.TokenNumber,
            token.Service.Name,
            position,
            $"{position * 5} mins",
            token.Status.ToString(),
            token.ExpiresAt));
    }
}