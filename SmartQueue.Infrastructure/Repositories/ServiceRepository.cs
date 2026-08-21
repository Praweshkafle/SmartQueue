using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.Infrastructure.Repositories;


public class ServiceRepository : GenericRepository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Service>> GetActiveAsync(CancellationToken ct) =>
        await _set.Where(s => s.IsActive).ToListAsync(ct);
}
