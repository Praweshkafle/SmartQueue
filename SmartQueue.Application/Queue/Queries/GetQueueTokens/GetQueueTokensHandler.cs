using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Queue.Queries.GetQueueTokens;

public class GetQueueTokensHandler
    : IRequestHandler<GetQueueTokensQuery, Result<IEnumerable<QueueTokenResponse>>>
{
    private readonly IUnitOfWork _uow;
    public GetQueueTokensHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<QueueTokenResponse>>> Handle(
        GetQueueTokensQuery request, CancellationToken ct)
    {
        var tokens = await _uow.Tokens.GetWaitingByServiceAsync(request.ServiceId, ct);
        var response = tokens.Select(t => new QueueTokenResponse(
            t.Id, t.TokenNumber, t.UserId,
            t.Status.ToString(), t.IssuedAt, t.ExpiresAt));
        return Result<IEnumerable<QueueTokenResponse>>.Success(response);
    }
}