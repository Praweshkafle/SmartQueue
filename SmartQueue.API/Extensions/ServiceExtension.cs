using SmartQueue.Domain.Interfaces;
using SmartQueue.Infrastructure.Jobs;
using SmartQueue.Infrastructure.Persistence;
using SmartQueue.Infrastructure.Services;

namespace SmartQueue.API.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<TokenExpiryJob>();
        services.AddScoped<QueueAutoAdvanceJob>();
        services.AddScoped<RedisCacheService>();
        services.AddScoped<ICacheService, ResilientCacheService>();
        return services;

    }
}