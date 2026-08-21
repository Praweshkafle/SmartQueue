using SmartQueue.Domain.Entities;

namespace SmartQueue.Domain.Interfaces;

public interface IServiceRepository : IGenericRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveAsync(CancellationToken ct);
}
