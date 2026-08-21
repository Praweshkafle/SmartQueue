using SmartQueue.Domain.Entities;

namespace SmartQueue.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}