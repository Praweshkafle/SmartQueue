using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Entities;

namespace SmartQueue.Application.Services.Queries.GetServices;

public record GetServicesQuery : IRequest<Result<IEnumerable<ServiceResponse>>>;

public record ServiceResponse(
    Guid Id,
    string Name,
    string Description,
    int MaxQueueSize,
    bool IsActive
);