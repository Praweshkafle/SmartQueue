using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Queue.Queries.GetQueueStatus;

public record GetQueueStatusQuery(Guid ServiceId)
    : IRequest<Result<QueueStatusResponse>>;

public record QueueStatusResponse(
    Guid ServiceId,
    string ServiceName,
    int? CurrentToken,
    int WaitingCount,
    string EstimatedWait
);