using MediatR;
using SmartQueue.Application.Common;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Application.Services.Commands.CreateService;

public class CreateServiceHandler : IRequestHandler<CreateServiceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public CreateServiceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        CreateServiceCommand request, CancellationToken ct)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            MaxQueueSize = request.MaxQueueSize,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Services.AddAsync(service, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(service.Id);
    }
}