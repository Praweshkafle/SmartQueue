using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Queue.Commands.AdvanceQueue;

public record AdvanceQueueCommand(
    Guid ServiceId,
    string PerformedBy
) : IRequest<Result<AdvanceQueueResponse>>;

public record AdvanceQueueResponse(
    int? CompletedToken,
    int? NextToken,
    string Message
);