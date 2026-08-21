using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Services.Commands.UpdateService;

public class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdateServiceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(
        UpdateServiceCommand request, CancellationToken ct)
    {
        var service = await _uow.Services.GetByIdAsync(request.Id, ct);
        if (service is null)
            return Result<bool>.Failure("Service not found.", 404);

        service.Name = request.Name;
        service.Description = request.Description;
        service.MaxQueueSize = request.MaxQueueSize;
        service.IsActive = request.IsActive;

        _uow.Services.Update(service);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}