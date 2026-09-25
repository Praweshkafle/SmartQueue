using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.API.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        
        // --- 1. POSTGRESQL (Npgsql) SETUP ---
        string dbConnectionString;
        var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrEmpty(envDatabaseUrl))
        {
            // Convert Railway's database URL format to standard Npgsql format
            var databaseUri = new Uri(envDatabaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            dbConnectionString = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require, // Railway PostgreSQL requires SSL in production
                TrustServerCertificate = true
            }.ToString();
        }
        else
        {
            // Fallback to local appsettings.json for local development
            dbConnectionString = config.GetConnectionString("DefaultConnection");
        }
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbConnectionString));

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