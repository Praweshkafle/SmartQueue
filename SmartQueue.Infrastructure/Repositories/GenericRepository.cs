using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public GenericRepository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _set.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct) =>
        await _set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct) =>
        await _set.AddAsync(entity, ct);

    public void Update(T entity) =>
        _set.Update(entity);

    public void Delete(T entity) =>
        _set.Remove(entity);
}