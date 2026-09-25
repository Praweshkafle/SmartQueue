using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartQueue.Infrastructure.Persistence;

namespace SmartQueue.API.Extensions;

public static class DatabaseExtension
{
   public static string GetDatabaseConnectionString(IConfiguration config)
    {
        var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrWhiteSpace(envDatabaseUrl))
        {
            var databaseUri = new Uri(envDatabaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':', 2);

            return new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = Uri.UnescapeDataString(userInfo[1]),
                Database = databaseUri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            }.ToString();
        }

        var connectionString = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is missing. " +
                "Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
        }

        return connectionString;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = GetDatabaseConnectionString(config);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

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
            throw;
        }

        return app;
    }
}