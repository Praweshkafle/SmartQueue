using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Constants;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Enums;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Tokens.Commands.CancleToken;

public class CancelTokenHandler : IRequestHandler<CancelTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    private readonly ICacheService _cache;
    public CancelTokenHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(
        CancelTokenCommand request, CancellationToken ct)
    {
        var token = await _uow.Tokens.GetByIdAsync(request.TokenId, ct);
        if (token is null)
            return Result<bool>.Failure("Token not found.", 404);

        var isAdmin = request.RequestingUserRole == Roles.Admin;
        var isOwner = token.UserId == request.RequestingUserId;

        if (!isAdmin && !isOwner)
            return Result<bool>.Failure("You can only cancel your own token.", 403);

        if (token.Status != TokenStatus.Waiting && token.Status != TokenStatus.Called)
            return Result<bool>.Failure("Only active tokens can be cancelled.", 400);

        token.Status = TokenStatus.Cancelled;
        _uow.Tokens.Update(token);
        await _uow.SaveChangesAsync(ct);
        await _cache.DeleteAsync(CacheKeys.QueueStatus(token.ServiceId), ct);
        return Result<bool>.Success(true);
    }
}