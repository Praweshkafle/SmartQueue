using SmartQueue.Domain.Entities;

namespace SmartQueue.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<bool> ExistsAsync(string email, CancellationToken ct);
}