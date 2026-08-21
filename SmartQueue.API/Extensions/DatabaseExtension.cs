using Microsoft.EntityFrameworkCore;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.API.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Running database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration failed. Application cannot start.");
            throw; // stop the app — don't serve traffic with wrong schema
        }

        return app;
    }
}