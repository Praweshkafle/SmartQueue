using MediatR;
using SmartQueue.Application.Common;

namespace SmartQueue.Application.Services.Commands.CreateService;

public record CreateServiceCommand(
    string Name,
    string Description,
    int MaxQueueSize
) : IRequest<Result<Guid>>;