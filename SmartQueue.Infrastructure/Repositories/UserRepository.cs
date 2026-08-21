using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Entities;
using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        await _set.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsAsync(string email, CancellationToken ct) =>
        await _set.AnyAsync(u => u.Email == email, ct);
}