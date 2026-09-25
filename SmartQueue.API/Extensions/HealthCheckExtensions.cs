using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SmartQueue.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration config)
    {
        var healthChecks = services.AddHealthChecks();

        var dbConnection = config.GetConnectionString("DefaultConnection");
        var redisConnection = config.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(dbConnection))
            healthChecks.AddNpgSql(
                dbConnection,
                name: "postgresql",
                tags: new[] { "ready", "db" });

        if (!string.IsNullOrEmpty(redisConnection))
            healthChecks.AddRedis(
                redisConnection,
                name: "redis",
                tags: new[] { "ready", "cache" });

        return services;
    }

    public static WebApplication UseHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponse
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        });

        return app;
    }

    public static Task WriteResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}