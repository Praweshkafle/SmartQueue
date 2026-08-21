using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string Description,
    int MaxQueueSize,
    bool IsActive
) : IRequest<Result<bool>>;