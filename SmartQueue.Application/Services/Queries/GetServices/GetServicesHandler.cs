using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Services.Queries.GetServices;

public class GetServicesHandler : IRequestHandler<GetServicesQuery, Result<IEnumerable<ServiceResponse>>>
{
    private readonly IUnitOfWork _uow;
    public GetServicesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<ServiceResponse>>> Handle(
        GetServicesQuery request, CancellationToken ct)
    {
        var services = await _uow.Services.GetActiveAsync(ct);
        var response = services.Select(s => new ServiceResponse(
            s.Id, s.Name, s.Description, s.MaxQueueSize, s.IsActive));
        return Result<IEnumerable<ServiceResponse>>.Success(response);
    }
}