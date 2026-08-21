using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Constants;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Queue.Queries.GetQueueStatus;

public class GetQueueStatusHandler
    : IRequestHandler<GetQueueStatusQuery, Result<QueueStatusResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetQueueStatusHandler(IUnitOfWork uow, ICacheService cache)
    {
        _cache = cache;
        _uow = uow;
    } 

    public async Task<Result<QueueStatusResponse>> Handle(
        GetQueueStatusQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.QueueStatus(request.ServiceId);

        var cached = await _cache.GetAsync<QueueStatusResponse>(cacheKey, ct);
        if (cached is not null)
            return Result<QueueStatusResponse>.Success(cached);

        var service = await _uow.Services.GetByIdAsync(request.ServiceId, ct);
        if (service is null)
            return Result<QueueStatusResponse>.Failure("Service not found.", 404);

        var currentToken = await _uow.Tokens
            .GetCurrentCalledTokenAsync(request.ServiceId, ct);
        var waitingCount = await _uow.Tokens
            .GetWaitingCountAsync(request.ServiceId, ct);

        var response = new QueueStatusResponse(
            service.Id,
            service.Name,
            currentToken?.TokenNumber,
            waitingCount,
            $"{waitingCount * 5} mins");

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromSeconds(30), ct);

        return Result<QueueStatusResponse>.Success(response);
    }
}