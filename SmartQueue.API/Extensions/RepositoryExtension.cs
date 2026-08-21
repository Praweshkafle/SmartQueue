using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Repositories;

namespace SmartQueue.API.Extensions;

public static class RepositoryExtension
{
    public static IServiceCollection AddRepository(this IServiceCollection services )
    {
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        return services;
    }
}